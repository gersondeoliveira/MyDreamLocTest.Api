using System.ComponentModel.DataAnnotations;

namespace MyDream.Api.Models;

public class Moto
{
    public int Id { get; set; }
    public string Placa { get; set; } = default!;
    public string Modelo { get; set; } = default!;
    public int AnoFabricacao { get; set; }
    public decimal ValorDiaria { get; set; }
    public StatusMoto Status { get; set; } = StatusMoto.Disponivel;

    // Rowversion: garante concorrência otimista na reserva da moto (evita duas locações
    // "ganharem" a mesma moto ao mesmo tempo sob alta concorrência).
    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;
}
