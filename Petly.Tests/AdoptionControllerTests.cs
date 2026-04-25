using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Petly.Business.Services;
using Petly.Controllers;
using Petly.DataAccess.Data;
using Petly.Models;
using Xunit;

namespace Petly.Tests;

public class AdoptionControllerTests
{
    [Fact]
    public async Task ViewApplicationsWithoutLogin()
    {
        using var provider = BuildServiceProvider();
        var controller = CreateController(provider);

        var result = await controller.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);
    }

    [Fact]
    public async Task ViewUserApplications()
    {
        using var provider = BuildServiceProvider();
        var db = provider.GetRequiredService<ApplicationDbContext>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        var user = await CreateUserAsync(userManager, roleManager, "user@test.com", "pass123", "user");
        var otherUser = await CreateUserAsync(userManager, roleManager, "other@test.com", "pass123", "user");
        db.Pets.Add(new Pet { PetId = 1, ShelterId = 10, PetName = "Barsik" });

        db.AdoptionApplications.AddRange(
            new AdoptionApplication { AdoptId = 1, UserId = user.Id, PetId = 1 },
            new AdoptionApplication { AdoptId = 2, UserId = otherUser.Id, PetId = 1 }
        );

        await db.SaveChangesAsync();

        var controller = CreateController(provider, "user", user.Id);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<AdoptionApplication>>(view.Model);

        Assert.Single(model);
    }

    [Fact]
    public async Task AdoptPet()
    {
        using var provider = BuildServiceProvider();
        var db = provider.GetRequiredService<ApplicationDbContext>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        var user = await CreateUserAsync(userManager, roleManager, "test@test.com", "pass123", "user");
        db.Pets.Add(new Pet { PetId = 2, ShelterId = 20, PetName = "Simba" });

        await db.SaveChangesAsync();

        var controller = CreateController(provider, "user", user.Id);

        var result = await controller.Adopt(new AdoptionRequestViewModel
        {
            PetId = 2,
            ApplicantName = "Іра",
            ApplicantSurname = "Коваль",
            ApplicantAge = 22,
            ContactInfo = "123"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddSingleton<IOptions<IdentityOptions>>(Options.Create(new IdentityOptions()));
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
        services.AddSingleton<ILookupNormalizer, UpperInvariantLookupNormalizer>();
        services.AddSingleton<IdentityErrorDescriber>();
        services.AddScoped<IUserStore<ApplicationUser>, UserStore<ApplicationUser, IdentityRole<int>, ApplicationDbContext, int>>();
        services.AddScoped<IRoleStore<IdentityRole<int>>, RoleStore<IdentityRole<int>, ApplicationDbContext, int>>();
        services.AddSingleton<ILogger<UserManager<ApplicationUser>>>(NullLogger<UserManager<ApplicationUser>>.Instance);
        services.AddSingleton<ILogger<RoleManager<IdentityRole<int>>>>(NullLogger<RoleManager<IdentityRole<int>>>.Instance);
        services.AddScoped<UserManager<ApplicationUser>>();
        services.AddScoped<RoleManager<IdentityRole<int>>>();
        services.AddScoped<AdoptionService>();
        services.AddScoped<PetService>();
        services.AddTransient<AdoptionController>();

        return services.BuildServiceProvider();
    }

    private static AdoptionController CreateController(ServiceProvider provider, string? role = null, int? userId = null)
    {
        var httpContext = new DefaultHttpContext
        {
            User = CreatePrincipal(userId, role),
            Session = new TestSession()
        };

        var controller = provider.GetRequiredService<AdoptionController>();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());

        return controller;
    }

    private static ClaimsPrincipal CreatePrincipal(int? userId, string? role)
    {
        if (!userId.HasValue)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.Value.ToString())
        };

        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    private static async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        string email,
        string password,
        string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            IdentityResult roleResult = await roleManager.CreateAsync(new IdentityRole<int>(role));
            Assert.True(roleResult.Succeeded, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            RegistrationDate = DateTime.UtcNow,
            Status = "Активний"
        };

        IdentityResult createResult = await userManager.CreateAsync(user, password);
        Assert.True(createResult.Succeeded, string.Join(", ", createResult.Errors.Select(e => e.Description)));

        IdentityResult addToRoleResult = await userManager.AddToRoleAsync(user, role);
        Assert.True(addToRoleResult.Succeeded, string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));

        return user;
    }

    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public IEnumerable<string> Keys => _store.Keys;
        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) => _store.TryGetValue(key, out value);
    }

    private class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
