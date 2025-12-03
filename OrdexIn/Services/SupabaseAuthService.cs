using OrdexIn.Models;
using OrdexIn.Services.Intefaces;
using Supabase.Gotrue;
using Client = Supabase.Client;

namespace OrdexIn.Services
{
    public class SupabaseAuthService : IAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SupabaseAuthService> _logger;
        private readonly IKardexDataService _kardexService;
        private readonly IConfiguration _configuration;
        private readonly Client _supabaseClient;
        
        private const int MaxRetry = 5;

        public SupabaseAuthService(Client supabaseClient, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor, IKardexDataService kardexService,
            ILogger<SupabaseAuthService> logger)
        {
            _supabaseClient = supabaseClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _kardexService = kardexService;
            _logger = logger;
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
                throw new Exception("Error logging in user " + ex);
            }

            return null;
        }

        public async Task<Session?> RegisterUserAsync(UserModel user, bool isAdmin = false)
        {
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
                throw new ArgumentException("Email and password are required", nameof(user));
            
            var currentAccess = _supabaseClient.Auth.CurrentSession?.AccessToken;
            var currentRefresh = _supabaseClient.Auth.CurrentSession?.RefreshToken;
            
            try
            {
                var session = await _supabaseClient.Auth.SignUp(user.Email, user.Password);
                
                if (session is {User.Id: not null})
                {
                    if (isAdmin)
                    {
                        if (Guid.TryParse(session.User.Id, out var guid))
                        {
                            try
                            {
                                var roleProfile = await _supabaseClient
                                    .From<ProfileModel>()
                                    .Where(u => u.UserId == guid)
                                    .Single();
                            
                                if (roleProfile == null)
                                {
                                    // Create profile if none exists
                                    var profileToInsert = new ProfileModel
                                    {
                                        UserId = guid,
                                        Email = user.Email,
                                        IsAdmin = true
                                    };
                                    await _supabaseClient
                                        .From<ProfileModel>()
                                        .Insert(profileToInsert);
                                }
                                else
                                {
                                    // Update existing profile to set admin
                                    roleProfile.IsAdmin = true;
                                    await roleProfile.Update<ProfileModel>();
                                }
                                
                                // await _kardexService.RegisterKardexEntry()
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to create/update profile for new user {UserId}", guid);
                            }
                            
                        }
                        else
                        {
                            _logger.LogWarning("Could not parse new user's id '{Id}' as GUID", session.User.Id);
                        }
                    }
                    return session;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error registering user " + ex);
            }
            finally
            {
                try
                {
                    // Restore previous session if any
                    if (!string.IsNullOrEmpty(currentAccess) && !string.IsNullOrEmpty(currentRefresh))
                    {
                        await _supabaseClient.Auth.SetSession(currentAccess, currentRefresh);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to restore Supabase session after creating user");
                }
                
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _supabaseClient.Auth.SignOut(Constants.SignOutScope.Local);
            }
            catch (Exception ex)
            {
                throw new Exception("Error logging out user " + ex);
            }
        }

        public async Task<bool> IsUserAdminAsync(Guid userId)
        {
            try
            { 
                var response = await _supabaseClient
                    .From<ProfileModel>()
                    .Select("*")
                    .Where(x => x.UserId == userId)
                    .Get()
                    .ContinueWith(t => t.Result.Models.FirstOrDefault());
            
                if (response == null)
                    _logger.LogWarning("User profile not found for userId: {userId}", userId);

                return response.IsAdmin;
            }
            catch (Exception e)
            {
                _logger.LogWarning("Error issuing user profile for userId {userid}", userId);
            }
            return false;
        }


        public async Task SendPasswordResetEmailAsync(UserModel user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("Email is required", nameof(user));

            try
            {
                // Try environment variable first
                var appUrl = Environment.GetEnvironmentVariable("APP_URL");

                // Fallback to configuration
                if (string.IsNullOrWhiteSpace(appUrl))
                {
                    appUrl = _configuration["AppUrl"];
                }

                // Final fallback: build from current request if available
                if (string.IsNullOrWhiteSpace(appUrl) && _httpContextAccessor?.HttpContext != null)
                {
                    var req = _httpContextAccessor.HttpContext.Request;
                    if (req.Host.HasValue)
                    {
                        appUrl = $"{req.Scheme}://{req.Host.Value}";
                    }
                }

                var redirectTo = string.IsNullOrWhiteSpace(appUrl)
                    ? null
                    : $"{appUrl.TrimEnd('/')}/reset-password";

                Console.WriteLine(redirectTo);

                if (string.IsNullOrEmpty(redirectTo))
                {
                    await _supabaseClient.Auth.ResetPasswordForEmail(user.Email);
                }
                else
                {
                    var options = new ResetPasswordForEmailOptions(user.Email)
                    {
                        RedirectTo = redirectTo
                    };
                    await _supabaseClient.Auth.ResetPasswordForEmail(options);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error sending password recovery email " + ex);
            }
        }

        public async Task<bool> ResetPasswordAsync(string newPassword, string? accessToken = null)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("New password is required", nameof(newPassword));

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token is required to reset password", nameof(accessToken));

            try
            {
                // Console.WriteLine("[SupabaseAuthService] Attempting to set session for password reset (masking token)...");
                // Console.WriteLine($"[SupabaseAuthService] accessToken length: {accessToken?.Length}");

                // Workaround: if refresh token is not available (common for recovery links),
                // pass the access token as the refresh token so the client will accept it.
                var refreshTokenToUse = accessToken;
                // Console.WriteLine("[SupabaseAuthService] Using access token as refresh token fallback for SetSession.");

                // Ensure the Supabase client has the session (so update call uses the token)
                await _supabaseClient.Auth.SetSession(accessToken, refreshTokenToUse);

                var updatePayload = new UserAttributes { Password = newPassword };
                var updateResult = await _supabaseClient.Auth.Update(updatePayload);

                var ok = updateResult != null;
                // Console.WriteLine($"[SupabaseAuthService] Update result non-null: {ok}");
                return ok;
            }
            catch (Exception ex)
            {
                throw new Exception("Error resetting password " + ex);
            }
        }

        public async Task<string> GetUserEmailAsync(Guid userId)
        {
            try
            {
                var user = await _supabaseClient
                    .From<ProfileModel>()
                    .Select("*")
                    .Where(x => x.UserId == userId)
                    .Single();
                
                return user?.Email ?? "Accion sin usuario";
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving user email " + ex);
            }
        }
        
        public async Task<List<ProfileModel>> GetAllUsersAsync()
        {
            var results = new List<ProfileModel>();
            const int chunkSize = 1000;
            var from = 0;

            while (true)
            {
                var response = await ExecuteWithRetry(async () =>
                    await _supabaseClient
                        .From<ProfileModel>()
                        .Select("*")
                        .Range(from, from + chunkSize - 1)
                        .Get()
                );

                var page = response?.Models;
                if (page == null || page.Count == 0)
                    break;

                results.AddRange(page);

                if (page.Count < chunkSize)
                    break; // last page

                from += chunkSize;
            }

            return results;
        }
        
        private static async Task<T> ExecuteWithRetry<T>(Func<Task<T>> operation)
        {
            var attempts = 0;
            var delayMs = 200;

            while (true)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    attempts++;
                    if (attempts >= MaxRetry)
                        throw new Exception($"[ERROR] Operation failed after {MaxRetry} attempts: {ex.Message}", ex);

                    await Task.Delay(delayMs);
                    delayMs = Math.Min(delayMs * 2, 2000);
                }
            }
        }
    }
}
