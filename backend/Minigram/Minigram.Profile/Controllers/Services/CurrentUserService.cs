namespace Minigram.Profile.Controllers.Services
{
    using System.Security.Claims;

    public class CurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private ClaimsPrincipal User =>
            _httpContextAccessor.HttpContext?.User ?? throw new InvalidOperationException();

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        public Guid? UserGuid =>
            Guid.TryParse(UserId, out var result) ? result : null;

        public string? Email =>
            User.FindFirstValue(ClaimTypes.Email);

        public bool IsAuthenticated =>
            User.Identity?.IsAuthenticated ?? false;
    }
}