using System.Security.Claims;

namespace ChangeManagement.Api.Services;

public interface IActorResolver
{
    Guid ResolveActorUserId();
}

public class ActorResolver : IActorResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ActorResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid ResolveActorUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var rawValue = user?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(rawValue, out var userId) && userId != Guid.Empty)
        {
            return userId;
        }

        throw new UnauthorizedAccessException("Authenticated actor is required.");
    }
}
