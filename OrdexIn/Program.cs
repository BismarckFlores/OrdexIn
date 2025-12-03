using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using OrdexIn.Services;
using Supabase;
using System.Security.Claims;
using OrdexIn.Hubs;
using OrdexIn.Services.Intefaces;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllersWithViews();

// SignalR
builder.Services.AddSignalR();

// Antiforgery header name (optional - matches JS header)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

// Authentication cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // session lifetime
        options.SlidingExpiration = true;                  // extend on activity
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

// Session (server-side idle timeout)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Global authorization requirement
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Other services
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<Client>(serviceProvider =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext;

    var config = builder.Configuration;
    var supabaseUrl = config["Supabase:Url"];
    var supabaseKey = config["Supabase:Key"];

    if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(supabaseKey))
        throw new InvalidOperationException("Supabase configuration is missing. Set 'Supabase:Url' and 'Supabase:Key'.");

    var supabaseClient = new Client(supabaseUrl, supabaseKey, new SupabaseOptions { /* ... */ });

    var accessToken = httpContext?.User.FindFirstValue("AccessToken");
    if (!string.IsNullOrEmpty(accessToken))
    {
        supabaseClient.Auth.SetSession(accessToken, string.Empty);
    }

    return supabaseClient;
});

// Custom services DI registration
builder.Services.AddScoped<IAppSignInService, AppSignInService>();
builder.Services.AddScoped<IAuthService, SupabaseAuthService>();
builder.Services.AddScoped<IProductService, ProductDAO>();
builder.Services.AddScoped<IPointOfSaleService, PointSaleDAO>();
builder.Services.AddScoped<IKardexDataService, KardexDAO>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // enable session middleware before auth (if needed by app)
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllers();
app.MapHub<InventoryHub>("/inventoryHub");

app.Run();