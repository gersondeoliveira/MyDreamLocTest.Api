using MyDream.Api.Data;

namespace MyDream.Api.Sagas.Steps;

// Simula uma chamada a um gateway de pagamento externo (ex.: Stripe, PagSeguro).
// Em um cenário real seria uma chamada HTTP com timeout/retry (ex.: Polly).
public class ProcessarPagamentoStep(AppDbContext db) : ISagaStep<LocacaoSagaContext>
{
    public string Nome => "ProcessarPagamento";

    public async Task ExecutarAsync(LocacaoSagaContext contexto, CancellationToken ct)
    {
        await Task.Delay(150, ct); // simula a latência de rede até o gateway

        if (contexto.Request.SimularFalhaPagamento)
            throw new SagaStepException("Pagamento recusado pelo gateway (simulado).");

        contexto.PagamentoAprovado = true;
    }

    // Estorno simulado: em um gateway real, aqui entraria a chamada de "refund".
    public Task CompensarAsync(LocacaoSagaContext contexto, CancellationToken ct)
    {
        contexto.PagamentoAprovado = false;
        return Task.CompletedTask;
    }
}
