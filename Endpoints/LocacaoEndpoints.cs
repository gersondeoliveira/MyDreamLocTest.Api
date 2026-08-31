using MyDream.Api.Data;
using MyDream.Api.DTOs;
using MyDream.Api.Sagas;
using MyDream.Api.Sagas.Steps;

namespace MyDream.Api.Endpoints;

public static class LocacaoEndpoints
{
    public static void MapLocacaoEndpoints(this WebApplication app)
    {
        // POST /api/locacoes — cria uma locação usando o Saga Pattern (orquestração local,
        // "de leve": sem message broker, tudo em processo — o que importa aqui é o padrão:
        // passos independentes + compensação explícita, não a infraestrutura de mensageria).
        //
        // Passos: ValidarCondutor -> ReservarMoto -> CriarLocacao -> ProcessarPagamento -> ConfirmarLocacao
        // Se qualquer passo falhar, os passos já executados são desfeitos em ordem inversa (LIFO):
        // ex. se o pagamento falhar, a moto reservada volta a "Disponivel" e a locação é removida.
        // Use "simularFalhaPagamento": true no corpo da requisição para ver a compensação em ação.
        app.MapPost("/api/locacoes", async (
                CriarLocacaoRequest request,
                AppDbContext db,
                ILoggerFactory loggerFactory,
                CancellationToken ct) =>
            {
                var contexto = new LocacaoSagaContext { Request = request };

                var passos = new ISagaStep<LocacaoSagaContext>[]
                {
                    new ValidarCondutorStep(db),
                    new ReservarMotoStep(db),
                    new CriarLocacaoStep(db),
                    new ProcessarPagamentoStep(db),
                    new ConfirmarLocacaoStep(db)
                };

                var saga = new SagaOrchestrator<LocacaoSagaContext>(
                    passos, loggerFactory.CreateLogger("LocacaoSaga"));

                try
                {
                    await saga.ExecutarAsync(contexto, ct);
                }
                catch (SagaStepException ex)
                {
                    return Results.UnprocessableEntity(new { erro = ex.Message });
                }

                return Results.Created(
                    $"/api/locacoes/{contexto.Locacao.Id}",
                    new LocacaoResponseDto(
                        contexto.Locacao.Id,
                        contexto.Locacao.Status.ToString(),
                        contexto.Locacao.ValorTotal,
                        "Locação confirmada com sucesso."));
            })
            .WithName("CriarLocacao")
            .WithSummary("Cria uma locação de moto (dispara o Saga de locação).")
            .Produces<LocacaoResponseDto>(201)
            .Produces(422);
    }
}
