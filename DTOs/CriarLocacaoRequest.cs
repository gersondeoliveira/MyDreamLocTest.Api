namespace MyDream.Api.DTOs;

public record CriarLocacaoRequest(
    int MotoId,
    string NomeCondutor,
    string Cnh,
    string CategoriaCnh, // "A" ou "AB"
    int PlanoDias,
    bool SimularFalhaPagamento = false);
