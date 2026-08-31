namespace MyDream.Api.Sagas;

/// Orquestrador de saga "leve": tudo roda em processo, sem message broker.
/// O que importa aqui é o padrão — passos independentes, cada um com sua compensação,
/// executados em sequência e desfeitos em ordem inversa (LIFO) se algo falhar no meio do caminho.
public class SagaOrchestrator<TContext>
{
    private readonly IReadOnlyList<ISagaStep<TContext>> _passos;
    private readonly ILogger _logger;

    public SagaOrchestrator(IEnumerable<ISagaStep<TContext>> passos, ILogger logger)
    {
        _passos = passos.ToList();
        _logger = logger;
    }

    public async Task ExecutarAsync(TContext contexto, CancellationToken ct = default)
    {
        var executados = new Stack<ISagaStep<TContext>>();

        foreach (var passo in _passos)
        {
            try
            {
                _logger.LogInformation("Saga: executando passo {Passo}", passo.Nome);
                await passo.ExecutarAsync(contexto, ct);
                executados.Push(passo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Saga: falha no passo {Passo}. Iniciando compensação.", passo.Nome);
                await CompensarAsync(executados, contexto, ct);
                throw;
            }
        }
    }

    private async Task CompensarAsync(Stack<ISagaStep<TContext>> executados, TContext contexto, CancellationToken ct)
    {
        while (executados.Count > 0)
        {
            var passo = executados.Pop();
            try
            {
                _logger.LogInformation("Saga: compensando passo {Passo}", passo.Nome);
                await passo.CompensarAsync(contexto, ct);
            }
            catch (Exception ex)
            {
                // Em produção: mandar pra uma fila de dead-letter / alertar — nunca deixar
                // uma falha de compensação passar em silêncio, pois deixa o sistema inconsistente.
                _logger.LogError(ex, "Saga: falha ao compensar passo {Passo}", passo.Nome);
            }
        }
    }
}
