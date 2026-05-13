using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ModularMonolith.BuildingBlocks.Application.Abstractions;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Services;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId => Guid.TryParse(Principal?.FindFirstValue("sub"), out var id) ? id : null;
    public Guid? TenantId => Guid.TryParse(Principal?.FindFirstValue("tenant_id"), out var id) ? id : null;
    public IReadOnlyList<string> Roles => Principal?.FindAll("role").Select(c => c.Value).ToList() ?? [];
    public IReadOnlyList<string> Permissions => Principal?.FindAll("permission").Select(c => c.Value).ToList() ?? [];
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}
