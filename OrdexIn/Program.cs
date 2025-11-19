var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Supabase integration
builder.Services.AddScoped<Supabase.Client>(_ =>
    new Supabase.Client(
        builder.Configuration["Supabase:Url"],
        builder.Configuration["Supabase:Key"],
        new Supabase.SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true
        }
    )
);

// Registrar servicios de auditoría, inventario y reportes (implementaciones en memoria para prototipado)
builder.Services.AddSingleton<OrdexIn.Services.IInventarioService, OrdexIn.Services.InMemoryInventarioService>();
builder.Services.AddSingleton<OrdexIn.Services.IReporteService, OrdexIn.Services.InMemoryReporteService>();
builder.Services.AddSingleton<OrdexIn.Services.IAuditService, OrdexIn.Services.InMemoryAuditService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
