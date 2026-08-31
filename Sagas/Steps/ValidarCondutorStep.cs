using Microsoft.EntityFrameworkCore;
using MyDream.Api.Data;
using MyDream.Api.Models;

namespace MyDream.Api.Sagas.Steps;

// Regra de negócio da MyDream: só loca moto para condutor com CNH categoria A ou AB.
public class ValidarCondutorStep(AppDbContext db) : ISagaStep<LocacaoSagaContext>
{
    public string Nome => "ValidarCondutor";

    public async Task ExecutarAsync(LocacaoSagaContext contexto, CancellationToken ct)
    {
        if (!Enum.TryParse<CategoriaCnh>(contexto.Request.CategoriaCnh, ignoreCase: true, out var categoria)
            || categoria is not (CategoriaCnh.A or CategoriaCnh.AB))
        {
            throw new SagaStepException("Condutor precisa de CNH categoria A ou AB para alugar uma moto.");
        }

        var condutor = await db.Condutores.FirstOrDefaultAsync(c => c.Cnh == contexto.Request.Cnh, ct);
        if (condutor is null)
        {
            condutor = new Condutor
            {
                Nome = contexto.Request.NomeCondutor,
                Cnh = contexto.Request.Cnh,
                CategoriaCnh = categoria
            };
            db.Condutores.Add(condutor);
            await db.SaveChangesAsync(ct);
        }

        contexto.Condutor = condutor;
    }

    // Nada a desfazer: cadastrar um condutor não tem efeito colateral em outra parte do sistema.
    public Task CompensarAsync(LocacaoSagaContext contexto, CancellationToken ct) => Task.CompletedTask;
}
