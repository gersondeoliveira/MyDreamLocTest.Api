using MyDream.Api.Data;
using MyDream.Api.Models;

namespace MyDream.Api.Sagas.Steps;

public class ConfirmarLocacaoStep(AppDbContext db) : ISagaStep<LocacaoSagaContext>
{
    public string Nome => "ConfirmarLocacao";

    public async Task ExecutarAsync(LocacaoSagaContext contexto, CancellationToken ct)
    {
        contexto.Locacao.Status = StatusLocacao.Confirmada;
        contexto.Moto.Status = StatusMoto.Locada;
        await db.SaveChangesAsync(ct);
    }

    public async Task CompensarAsync(LocacaoSagaContext contexto, CancellationToken ct)
    {
        contexto.Locacao.Status = StatusLocacao.Cancelada;
        // A moto volta para "Disponivel" é responsabilidade do ReservarMotoStep.CompensarAsync,
        // que roda depois deste na ordem de compensação (LIFO) — evitamos duplicar a regra aqui.
        await db.SaveChangesAsync(ct);
    }
}
