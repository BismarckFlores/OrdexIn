using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OrdexIn.Services.Intefaces;
using Supabase.Gotrue;

namespace OrdexIn.Services
{
    public class AppSignInService : IAppSignInService
    {
        private readonly ILogger<AppSignInService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthService _authService;

        public AppSignInService(IHttpContextAccessor httpContextAccessor, IAuthService authService,
            ILogger<AppSignInService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _authService = authService;
            _logger = logger;
        }

        public async Task SignInAsync(Session session)
        {
            
            var http = _httpContextAccessor.HttpContext;
            if (http == null || session?.User == null || string.IsNullOrEmpty(session.AccessToken))
                return;

            if (!Guid.TryParse(session.User.Id, out var userId))
                throw new Exception("Invalid user id or session.");
            
            bool isAdmin;
            try
            {
                isAdmin = await _authService.IsUserAdminAsync(userId);
            }
            catch (Exception e)
            {
                throw new Exception("Error during user role confirmation " + e);
            }
            
            _logger.LogInformation("Signing in user {UserId} with role {Role}", userId, isAdmin ? "admin" : "user");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Email, session.User.Email ?? string.Empty),
                new(ClaimTypes.Role, isAdmin ? "admin" : "user"),
                new("AccessToken", session.AccessToken)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            http.User = principal;
        }

        public async Task SignOutAsync()
        {
            var http = _httpContextAccessor.HttpContext;
            if (http == null) return;

            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}