using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyDream.Api.Data;
using MyDream.Api.DTOs;

namespace MyDream.Api.Endpoints; 

public static class MotoEndpoints
{
    // Compiled query: o EF Core gera o plano de tradução da consulta uma única vez (na
    // inicialização) em vez de recompilá-lo a cada chamada — reduz CPU sob alta carga.
    private static readonly Func<AppDbContext, int, int, IAsyncEnumerable<MotoDisponivelDto>> ConsultaMotosDisponiveis =
        EF.CompileAsyncQuery((AppDbContext db, int cursor, int pageSize) =>
            db.Motos
                .AsNoTracking()
                .Where(m => m.Status == Models.StatusMoto.Disponivel && m.Id > cursor)
                .OrderBy(m => m.Id)
                .Take(pageSize)
                .Select(m => new MotoDisponivelDto(m.Id, m.Placa, m.Modelo, m.AnoFabricacao, m.ValorDiaria)));

    public static void MapMotoEndpoints(this WebApplication app)
    {
        // GET /api/motos/disponiveis — endpoint pensado para alta escala/performance.
        //
        // Decisões aplicadas aqui:
        //  1. Paginação por keyset (cursor = último Id da página anterior), não OFFSET/LIMIT:
        //     OFFSET grande é O(n) no SQL Server (ele ainda percorre e descarta linhas); keyset
        //     usa o índice (Status, Id) e é O(log n), independente de estar na página 1 ou 10.000.
        //  2. AsNoTracking(): dispensa o change tracker do EF — não vamos alterar essas entidades
        //     nesta requisição, então evitamos a alocação e o custo de rastreamento.
        //  3. Projeção direta para DTO: o SQL gerado só traz as colunas usadas (sem overfetching).
        //  4. Compiled query: evita recompilar a expressão LINQ -> SQL a cada chamada.
        //  5. Cache em memória via GetOrCreateAsync (atômico por chave -> protege contra
        //     "cache stampede"), com TTL curto: uma lista de "disponíveis agora" não precisa
        //     ser 100% real-time.
        //  6. ETag simples: cliente pode mandar If-None-Match e economizar banda com 304.
        //
        // Fora do escopo deste demo, mas seria o próximo passo em produção: output cache
        // distribuído (Redis) atrás de um load balancer, e uma read replica do SQL Server
        // para tirar esta leitura do banco primário.
        app.MapGet("/api/motos/disponiveis", async (
                int cursor,
                int pageSize,
                AppDbContext db,
                IMemoryCache cache,
                HttpContext http,
                CancellationToken ct) =>
            {
                pageSize = pageSize is > 0 and <= 100 ? pageSize : 20;
                cursor = cursor < 0 ? 0 : cursor;

                var cacheKey = $"motos-disponiveis:{cursor}:{pageSize}";

                var itens = await cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromSeconds(5);
                    var lista = new List<MotoDisponivelDto>();
                    await foreach (var moto in ConsultaMotosDisponiveis(db, cursor, pageSize).WithCancellation(ct))
                        lista.Add(moto);
                    return lista;
                });

                var etag = $"\"{itens!.Count}-{cursor}-{pageSize}\"";
                if (http.Request.Headers.IfNoneMatch == etag)
                    return Results.StatusCode(StatusCodes.Status304NotModified);

                http.Response.Headers.ETag = etag;

                var proximoCursor = itens.Count == pageSize ? itens[^1].Id : (int?)null;
                var mensagem = itens.Count == 0 ? "Nenhuma moto disponível no momento." : null;

                return Results.Ok(new { itens, proximoCursor, mensagem });
            })
            .WithName("ListarMotosDisponiveis")
            .WithSummary("Lista motos disponíveis para locação (paginação por cursor).")
            .Produces(200)
            .Produces(304);
    }
}
