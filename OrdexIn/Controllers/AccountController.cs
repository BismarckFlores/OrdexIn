using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdexIn.Models;
using OrdexIn.Services;
using Supabase.Gotrue;

namespace OrdexIn.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IAuthService _supabaseAuthService;
        private readonly IAppSignInService _appSignInService;
        public AccountController(IAuthService supabaseAuthService, IAppSignInService appSignInService)
        {
            _supabaseAuthService = supabaseAuthService;
            _appSignInService = appSignInService;
        }
        
        public IActionResult Index()
        {
            // Redirige directamente al path de login configurado en Cookie options
            return Redirect("/login");
        }

        // GET: /login - muestra la página de login (necesario para que la cookie middleware pueda redirigir con GET)
        [HttpGet]
        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /login - procesa el envío del formulario de login
        [HttpPost]
        [Route("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessLogin(UserModel user)
        {
            var session = await _supabaseAuthService.LoginUserAsync(user);
        
            if (session?.User != null && !string.IsNullOrEmpty(session.AccessToken))
            {
                await _appSignInService.SignInAsync(session);
                return RedirectToAction("Index", "Home");
            }
        
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, "Credenciales inválidas. Por favor, inténtelo de nuevo.");
            return View("Login", user);
        }

        // GET: /register
        [HttpGet]
        [Route("register")]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /register
        [HttpPost]
        [Route("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessRegister(UserModel user)
        {
            if (!ModelState.IsValid)
                return View("Login", user);
        
            if (!IsPaswordStrong(user.Password))
            {
                ModelState.AddModelError(string.Empty, "La contraseña no cumple con los requisitos de seguridad.");
                return View("Register", user);
            }
        
            var session = await _supabaseAuthService.RegisterUserAsync(user);
        
            if (session?.User != null && !string.IsNullOrEmpty(session.AccessToken))
            {
                await _appSignInService.SignInAsync(session);
                return RedirectToAction("Index", "Home");
            }
        
            ModelState.AddModelError(string.Empty, "Error al registrar el usuario. Por favor, inténtelo de nuevo.");
            return View("Register", user);
        }

        // GET: /forgot-password
        [HttpGet]
        [Route("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /forgot-password
        [HttpPost]
        [Route("forgot-password")]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPasswordPost()
        {
            return View();
        }

        // GET: /reset-password
        [HttpGet]
        [Route("reset-password")]
        public IActionResult ResetPassword()
        {
            return View();
        }

        // POST: /reset-password
        [HttpPost]
        [Route("reset-password")]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPasswordPost()
        {
            return View();
        }

        // POST: /logout
        [HttpPost]
        [Route("logout")]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            return Redirect("/login");
        }
        
        private bool IsPaswordStrong(string password)
        {
            return password.Length >= 8
                && password.Any(char.IsUpper)
                && password.Any(char.IsLower)
                && password.Any(char.IsDigit)
                && password.Any(ch => !char.IsLetterOrDigit(ch));
        }
    }
}
