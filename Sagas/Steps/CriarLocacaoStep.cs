using MyDream.Api.Data;
using MyDream.Api.Models;

namespace MyDream.Api.Sagas.Steps;

public class CriarLocacaoStep(AppDbContext db) : ISagaStep<LocacaoSagaContext>
{
    public string Nome => "CriarLocacao";

    public async Task ExecutarAsync(LocacaoSagaContext contexto, CancellationToken ct)
    {
        var locacao = new Locacao
        {
            MotoId = contexto.Moto.Id,
            CondutorId = contexto.Condutor.Id,
            DataInicio = DateOnly.FromDateTime(DateTime.UtcNow),
            PlanoDias = contexto.Request.PlanoDias,
            ValorTotal = contexto.Moto.ValorDiaria * contexto.Request.PlanoDias,
            Status = StatusLocacao.PendentePagamento
        };

        db.Locacoes.Add(locacao);
        await db.SaveChangesAsync(ct);

        contexto.Locacao = locacao;
    }

    public async Task CompensarAsync(LocacaoSagaContext contexto, CancellationToken ct)
    {
        if (contexto.Locacao is null) return;

        db.Locacoes.Remove(contexto.Locacao);
        await db.SaveChangesAsync(ct);
    }
}
