using MyDream.Api.DTOs;
using MyDream.Api.Models;

namespace MyDream.Api.Sagas;

public class LocacaoSagaContext
{
    public required CriarLocacaoRequest Request { get; init; }
    public Moto Moto { get; set; } = default!;
    public Condutor Condutor { get; set; } = default!;
    public Locacao Locacao { get; set; } = default!;
    public bool MotoReservada { get; set; }
    public bool PagamentoAprovado { get; set; }
}
