using Microsoft.EntityFrameworkCore;
using Petly.DataAccess.Data;
using Petly.Business.Services;
using Microsoft.AspNetCore.Http;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// 1. Підключення MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 2. Реєстрація сервісів
builder.Services.AddScoped<PetService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<NeedService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Додавання контролерів з Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); 
app.UseRouting();
app.UseSession();

// Проста сесійна авторизація: без логіну/реєстрації доступу до сайту немає
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var isAuthenticated = context.Session.GetInt32("AccountId").HasValue;
    var allowed = new[]
    {
        "/Account/Login",
        "/Account/Register",
        "/Account/Logout",
        "/favicon.ico"
    };

    bool isStatic = path.StartsWithSegments("/css") || path.StartsWithSegments("/js")
        || path.StartsWithSegments("/lib") || path.StartsWithSegments("/images");

    if (!isAuthenticated && !isStatic && !allowed.Any(p => path.StartsWithSegments(p)))
    {
        context.Response.Redirect("/Account/Login");
        return;
    }

    await next();
});
app.UseAuthorization();

// Маршрути
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
