-- Execute depois de aplicar as migrations (dotnet ef database update).
INSERT INTO Motos (Placa, Modelo, AnoFabricacao, ValorDiaria, Status)
VALUES
  (N'ABC1D23', N'Honda CG 160',      2023, 35.00, 0),
  (N'DEF4E56', N'Honda Biz 110',     2022, 28.00, 0),
  (N'GHI7F89', N'Yamaha Factor 125', 2024, 32.00, 0);
