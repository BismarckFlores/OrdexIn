using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdexIn.Models;
using OrdexIn.Services.Intefaces;

namespace OrdexIn.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAuthService _supabaseAuthService;
        private readonly IAppSignInService _appSignInService;
        public AccountController(IAuthService supabaseAuthService, IAppSignInService appSignInService,
            ILogger<AccountController> logger)
        {
            _supabaseAuthService = supabaseAuthService;
            _appSignInService = appSignInService;
            _logger = logger;
        }
        
        public IActionResult Index() => Redirect("/login");

        [Route("expired")]
        public IActionResult Expired() => View();

        // GET: /login - muestra la página de login (necesario para que la cookie middleware pueda redirigir con GET)
        [HttpGet]
        [Route("login")]
        public IActionResult Login() => View();

        // POST: /login - procesa el envío del formulario de login
        [HttpPost]
        [Route("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessLogin(UserModel user)
        {
            try
            {
                var session = await _supabaseAuthService.LoginUserAsync(user);
                
                if (session == null) _logger.LogCritical("[ProcessLogin] SupabaseAuthService.LoginUserAsync returned null session.");

                if (session?.User != null && !string.IsNullOrEmpty(session.AccessToken))
                {
                    try
                    {
                        await _appSignInService.SignInAsync(session);
                        return RedirectToAction("Index", "Home");
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError(string.Empty, "Error al inciar session en la aplicación. " + e.Message);
                        throw;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Hay Credenciales inválidas. Por favor, inténtelo de nuevo. {ex}");
            }
            return View("Login", user);
        }

        // GET: /forgot-password
        [HttpGet]
        [Route("forgot-password")]
        public IActionResult ForgotPassword() => View();

        // POST: /forgot-password
        [HttpPost]
        [Route("forgot-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "El correo es requerido.");
                return View("ForgotPassword");
            }
            try
            {
                await _supabaseAuthService.SendPasswordResetEmailAsync(new UserModel { Email = email });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error sending password reset email, try again in 10 seconds." + ex);
                return View("ForgotPassword", new UserModel { Email = email });
            }
            
            return RedirectToAction("Login");
        }

        // GET: /reset-password
        [HttpGet]
        [Route("reset-password")]
        public IActionResult ResetPassword() => View();

        // POST: /reset-password
        [HttpPost]
        [Route("reset-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessResetPassword(string newPassword, string confirmPassword, string access_token)
        {
            // Console.WriteLine($"[ResetPassword] Received access_token: {(access_token == null ? "null" : $"len={access_token.Length}")}");

            if (string.IsNullOrWhiteSpace(access_token))
            {
                ModelState.AddModelError(string.Empty, "Access token is missing. Request a new password reset email.");
                return View("ResetPassword");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError(string.Empty, "Both password fields are required.");
                return View("ResetPassword");
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View("ResetPassword");
            }

            if (!IsPaswordStrong(newPassword))
            {
                ModelState.AddModelError(string.Empty, "The password does not meet strength requirements.");
                return View("ResetPassword");
            }

            try
            {
                var success = await _supabaseAuthService.ResetPasswordAsync(newPassword, access_token);
                if (success)
                {
                    TempData["InfoMessage"] = "Contraseña actualizada correctamente. Por favor, inicie sesión.";
                    return RedirectToAction("ResetPassword");
                }

                // Console.WriteLine("[ResetPassword] ResetPasswordAsync returned false - password not updated.");
                ModelState.AddModelError(string.Empty, "Failed to reset password. The reset link may be expired or invalid. Check server logs for details.");
                return View("ResetPassword");
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"[ResetPassword] Exception: {ex}");
                ModelState.AddModelError(string.Empty, "An error occurred while resetting the password.");
                return View("ResetPassword");
            }
        }

        // POST: /logout
        [HttpPost]
        [Route("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _supabaseAuthService.LogoutAsync();
                await _appSignInService.SignOutAsync();
            }
            catch
            {
                // Log error if needed
            }
            
            return Redirect("/login");
        }
        
        private static bool IsPaswordStrong(string password)
        {
            return password.Length >= 8
                && password.Any(char.IsUpper)
                && password.Any(char.IsLower)
                && password.Any(char.IsDigit)
                && password.Any(ch => !char.IsLetterOrDigit(ch));
        }
    }
}
