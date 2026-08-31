namespace MyDream.Api.Models;

public class Locacao
{
    public int Id { get; set; }

    public int MotoId { get; set; }
    public Moto? Moto { get; set; }

    public int CondutorId { get; set; }
    public Condutor? Condutor { get; set; }

    public DateOnly DataInicio { get; set; }
    public int PlanoDias { get; set; }
    public decimal ValorTotal { get; set; }
    public StatusLocacao Status { get; set; } = StatusLocacao.PendentePagamento;
    public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;
}
