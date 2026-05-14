using ModularMonolith.BuildingBlocks.Application.Abstractions;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Middleware;

public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public string CorrelationId =>
        _httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.ItemKey] as string
        ?? string.Empty;
}
