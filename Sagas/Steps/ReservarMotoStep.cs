using Microsoft.EntityFrameworkCore;
using MyDream.Api.Data;
using MyDream.Api.Models;

namespace MyDream.Api.Sagas.Steps;

// Reserva otimista da moto: usa RowVersion (concorrência otimista) para impedir que duas
// locações concorrentes peguem a mesma moto — evita overbooking sob alta concorrência.
public class ReservarMotoStep(AppDbContext db) : ISagaStep<LocacaoSagaContext>
{
    public string Nome => "ReservarMoto";

    public async Task ExecutarAsync(LocacaoSagaContext contexto, CancellationToken ct)
    {
        var moto = await db.Motos.FirstOrDefaultAsync(m => m.Id == contexto.Request.MotoId, ct)
            ?? throw new SagaStepException($"Moto {contexto.Request.MotoId} não encontrada.");

        if (moto.Status != StatusMoto.Disponivel)
            throw new SagaStepException($"Moto {moto.Placa} não está disponível (status atual: {moto.Status}).");

        moto.Status = StatusMoto.Reservada;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new SagaStepException("Outra locação reservou esta moto no mesmo instante. Tente novamente.");
        }

        contexto.Moto = moto;
        contexto.MotoReservada = true;
    }

    public async Task CompensarAsync(LocacaoSagaContext contexto, CancellationToken ct)
    {
        if (!contexto.MotoReservada) return;

        contexto.Moto.Status = StatusMoto.Disponivel;
        await db.SaveChangesAsync(ct);
    }
}
