-- Script T-SQL para criar o banco e as tabelas da MyDream API (demo).
-- Alternativa manual a "dotnet ef database update" - use se preferir não
-- rodar migrations do EF Core, ou pra ter algo pra mostrar no processo seletivo.

IF DB_ID('MyDreamDb') IS NULL
BEGIN
    CREATE DATABASE MyDreamDb;
END
GO

USE MyDreamDb;
GO

IF OBJECT_ID('dbo.Locacoes', 'U') IS NOT NULL DROP TABLE dbo.Locacoes;
IF OBJECT_ID('dbo.Condutores', 'U') IS NOT NULL DROP TABLE dbo.Condutores;
IF OBJECT_ID('dbo.Motos', 'U') IS NOT NULL DROP TABLE dbo.Motos;
GO

CREATE TABLE dbo.Motos (
    Id             INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Placa          NVARCHAR(10)      NOT NULL,
    Modelo         NVARCHAR(60)      NOT NULL,
    AnoFabricacao  INT               NOT NULL,
    ValorDiaria    DECIMAL(10,2)     NOT NULL,
    Status         INT               NOT NULL DEFAULT 0, -- 0=Disponivel 1=Reservada 2=Locada 3=Manutencao
    RowVersion     ROWVERSION        NOT NULL             -- concorrência otimista (reserva da moto)
);
GO

CREATE UNIQUE INDEX UX_Motos_Placa ON dbo.Motos (Placa);
-- sustenta a query paginada de "disponíveis" (keyset pagination) do GET de alta escala
CREATE INDEX IX_Motos_Status_Id ON dbo.Motos (Status, Id);
GO

CREATE TABLE dbo.Condutores (
    Id            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nome          NVARCHAR(120)     NOT NULL,
    Cnh           NVARCHAR(11)      NOT NULL,
    CategoriaCnh  INT               NOT NULL -- 0=A 1=B 2=AB
);
GO

CREATE UNIQUE INDEX UX_Condutores_Cnh ON dbo.Condutores (Cnh);
GO

CREATE TABLE dbo.Locacoes (
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MotoId       INT               NOT NULL,
    CondutorId   INT               NOT NULL,
    DataInicio   DATE              NOT NULL,
    PlanoDias    INT               NOT NULL,
    ValorTotal   DECIMAL(10,2)     NOT NULL,
    Status       INT               NOT NULL DEFAULT 0, -- 0=PendentePagamento 1=Confirmada 2=Cancelada 3=Finalizada
    CriadoEmUtc  DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Locacoes_Motos      FOREIGN KEY (MotoId)     REFERENCES dbo.Motos (Id),
    CONSTRAINT FK_Locacoes_Condutores FOREIGN KEY (CondutorId) REFERENCES dbo.Condutores (Id)
);
GO

CREATE INDEX IX_Locacoes_MotoId     ON dbo.Locacoes (MotoId);
CREATE INDEX IX_Locacoes_CondutorId ON dbo.Locacoes (CondutorId);
GO

-- Seed opcional para testar o GET /api/motos/disponiveis sem precisar cadastrar nada antes.
INSERT INTO dbo.Motos (Placa, Modelo, AnoFabricacao, ValorDiaria, Status)
VALUES
    (N'ABC1D23', N'Honda CG 160',      2023, 35.00, 0),
    (N'DEF4E56', N'Honda Biz 110',     2022, 28.00, 0),
    (N'GHI7F89', N'Yamaha Factor 125', 2024, 32.00, 0);
GO
