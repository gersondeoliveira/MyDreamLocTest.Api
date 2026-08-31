namespace MyDream.Api.Sagas;

/// Erro de negócio esperado dentro de um passo da saga (ex.: moto indisponível,
/// pagamento recusado). Dispara a compensação, mas não é um "bug".
public class SagaStepException(string message) : Exception(message);
