namespace MyDream.Api.Models;

public class Condutor
{
    public int Id { get; set; }
    public string Nome { get; set; } = default!;
    public string Cnh { get; set; } = default!;
    public CategoriaCnh CategoriaCnh { get; set; }
}
