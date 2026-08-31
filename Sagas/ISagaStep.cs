namespace MyDream.Api.Sagas;

public interface ISagaStep<TContext>
{
    string Nome { get; }
    Task ExecutarAsync(TContext contexto, CancellationToken ct);
    Task CompensarAsync(TContext contexto, CancellationToken ct);
}
