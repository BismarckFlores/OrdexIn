using OrdexIn.Models;
using Supabase.Gotrue;

namespace OrdexIn.Services
{
    public interface IAuthService
    {
        Task<Session?> LoginUserAsync(UserModel user);
        Task<Session?> RegisterUserAsync(UserModel user, bool isAdmin = false);
        Task SendPasswordResetEmailAsync(UserModel user);
        Task<bool> ResetPasswordAsync(string newPassword, string? accessToken = null);
        Task<bool> IsUserAdminAsync(Guid userId);
        Task LogoutAsync();
        
        Task<List<UserModel>> GetAllUsersAsync();
    }
}
