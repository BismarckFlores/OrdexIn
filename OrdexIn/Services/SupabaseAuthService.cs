using OrdexIn.Models;
using Supabase.Gotrue;
using Client = Supabase.Client;

namespace OrdexIn.Services
{
    public class SupabaseAuthService : IAuthService
    {
        private readonly Client _supabaseClient;

        public SupabaseAuthService(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<Session?> LoginUserAsync(UserModel user)
        {
            try
            {
                var session = await _supabaseClient.Auth.SignIn(user.Email, user.Password);

                if (session?.User != null)
                {
                    return session;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error logging in user", ex);
            }
            return null;
        }

        public async Task<Session?> RegisterUserAsync(UserModel user, bool isAdmin = false)
        {
            var options = new SignUpOptions();
            if (isAdmin)
            {
                options.Data = new Dictionary<string, object>
                {
                    { "is_admin", isAdmin }
                };
            }

            try
            {
                var session = await _supabaseClient.Auth.SignUp(user.Email, user.Password, options);
                if (session?.User != null)
                {
                    return session;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error registering user", ex);
            }
            return null;
        }
        
        public Task LogoutAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsUserAdminAsync(Guid userId)
        {
            var response = await _supabaseClient
                .From<UserRoleModel>()
                .Where(x => x.UserId == userId)
                .Limit(1)
                .Get();

            var userRole = response.Models.FirstOrDefault();
            return userRole != null && userRole.IsAdmin;
        }


        public Task SendPasswordResetEmailAsync(UserModel user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ResetPasswordAsync(string newPassword, string? accessToken = null)
        {
            throw new NotImplementedException();
        }
    }
}
