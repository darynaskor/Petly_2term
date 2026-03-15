using Microsoft.EntityFrameworkCore;
using Petly.DataAccess.Data;
using Petly.Business.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Підключення MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 2. Реєстрація сервісів
builder.Services.AddScoped<PetService>();

// 3. Додавання контролерів з Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // для wwwroot

app.UseRouting();
app.UseAuthorization();

// Маршрути
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();