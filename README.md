# MyDream API (demo)

API demo em **.NET 10** simulando o domínio de **locação de motos** (estilo MyDream — motos
para entregadores/motoristas de app), pensada para uma avaliação técnica.

## Domínio (sugestão, ajuste como quiser)

- **Moto**: veículo disponível pra locação (placa, modelo, ano, valor da diária, status).
- **Condutor**: quem aluga. Regra de negócio real da MyDream: só aluga com **CNH categoria A ou AB**.
- **Locação**: vincula um Condutor a uma Moto por um plano de dias, com um valor total calculado.

## Por que só dois endpoints

Como pedido, o escopo é enxuto — mas cada endpoint carrega um requisito técnico diferente: 

| Endpoint | Verbo | O que demonstra |
|---|---|---|
| `/api/motos/disponiveis` | GET | Código pensado para **alta escala/performance** |
| `/api/locacoes` | POST | **Saga Pattern** (leve, orquestração em processo) |

## GET /api/motos/disponiveis — alta escala

Query string: `?cursor=0&pageSize=20`

Decisões de performance, todas comentadas no código (`Endpoints/MotoEndpoints.cs`):

1. **Paginação por keyset** (cursor = último `Id` da página anterior), em vez de `OFFSET/LIMIT` —
   usa o índice `(Status, Id)` e não degrada conforme a base cresce.
2. **`AsNoTracking()`** — sem change tracker, já que é uma leitura pura.
3. **Projeção direta pra DTO** — sem overfetching de colunas.
4. **Compiled query** (`EF.CompileAsyncQuery`) — evita recompilar a tradução LINQ → SQL a cada chamada.
5. **Cache em memória com `GetOrCreateAsync`** — atômico por chave (protege contra *cache
   stampede*), TTL curto (5s), adequado pra uma lista que não precisa ser 100% real-time.
6. **ETag** simples — cliente pode reaproveitar com `If-None-Match` e ganhar um 304.

Próximo passo fora do escopo do demo: cache distribuído (Redis) e uma read replica do SQL Server.

## POST /api/locacoes — Saga Pattern (leve)

Passos, em `Sagas/Steps/`:

```
ValidarCondutor -> ReservarMoto -> CriarLocacao -> ProcessarPagamento -> ConfirmarLocacao
```

Cada passo tem `ExecutarAsync` e `CompensarAsync`. Se um passo falhar, o orquestrador
(`Sagas/SagaOrchestrator.cs`) desfaz os passos já concluídos na ordem inversa (LIFO) — por
exemplo, se o pagamento (simulado) falhar, a moto reservada volta a "Disponivel" e a locação
criada é removida.

É uma saga **orquestrada e local** (sem message broker) — de propósito, já que peguei "de leve". O padrão (passos + compensação) é o mesmo que se usaria numa saga coreografada
com eventos entre microsserviços; aqui ele só roda dentro de um único processo/handler.

Pra ver a compensação em ação, envie `"simularFalhaPagamento": true` no corpo da requisição.

### Exemplo de request

```json
POST /api/locacoes
{
  "motoId": 1,
  "nomeCondutor": "João Silva",
  "cnh": "12345678900",
  "categoriaCnh": "A",
  "planoDias": 7,
  "simularFalhaPagamento": false
}
```

## Como rodar localmente

```bash
cd MyDream.Api
dotnet restore

# ajuste a connection string em appsettings.json antes disso
dotnet ef migrations add InicialCreate
dotnet ef database update

# opcional: popule algumas motos
sqlcmd -S localhost,1433 -U sa -P "SuaSenhaForte!123" -d MyDreamDb -i Scripts/seed.sql

dotnet run
```

Swagger disponível em `/swagger` no ambiente de Development.

## PENDENTE...

- Deploy no GCP (Cloud Run + Cloud SQL for SQL Server, provavelmente).
- Autenticação/autorização (hoje não há nenhuma).
- Testes automatizados dos passos da saga.
