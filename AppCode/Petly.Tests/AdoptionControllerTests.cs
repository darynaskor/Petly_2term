using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
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
        await using var db = CreateDbContext();
        var controller = CreateController(db);

        var result = await controller.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);
        Assert.Equal("Account", redirect.ControllerName);
    }

    [Fact]
    public async Task ViewUserApplications()
    {
        await using var db = CreateDbContext();

        db.Users.Add(new ApplicationUser { Id = 7, UserName = "user7@test.com", Email = "user7@test.com" });

        db.Pets.Add(new Pet { PetId = 1, ShelterId = 10, PetName = "Barsik", Status = "Доступний" });

        db.AdoptionApplications.AddRange(
            new AdoptionApplication { AdoptId = 11, UserId = 7, PetId = 1, Status = AdoptionStatuses.Pending },
            new AdoptionApplication { AdoptId = 12, UserId = 8, PetId = 1, Status = AdoptionStatuses.Pending });

        await db.SaveChangesAsync();

        var controller = CreateController(db, "user", 7);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<AdoptionApplication>>(view.Model);

        var application = Assert.Single(model);
        Assert.Equal(7, application.UserId);
    }

    [Fact]
    public async Task OpenAdoptPage()
    {
        await using var db = CreateDbContext();

        db.Users.Add(new ApplicationUser
        {
            Id = 9,
            Name = "Оля",
            Surname = "Коваль",
            UserName = "user9@test.com",
            Email = "user9@test.com"
        });

        db.Pets.Add(new Pet
        {
            PetId = 5,
            ShelterId = 30,
            PetName = "Simba",
            Status = "Доступний"
        });

        await db.SaveChangesAsync();

        var controller = CreateController(db, "user", 9);

        var result = await controller.Create(5);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AdoptionRequestViewModel>(view.Model);

        Assert.Equal("Simba", model.PetName);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task AdoptPet()
    {
        await using var db = CreateDbContext();

        db.Users.Add(new ApplicationUser
        {
            Id = 13,
            UserName = "user13@test.com",
            Email = "user13@test.com"
        });

        db.Pets.Add(new Pet
        {
            PetId = 6,
            ShelterId = 41,
            PetName = "Daisy",
            Status = "Доступний"
        });

        await db.SaveChangesAsync();

        var controller = CreateController(db, "user", 13);

        var result = await controller.Adopt(new AdoptionRequestViewModel
        {
            PetId = 6,
            ApplicantName = "Анна",
            ApplicantSurname = "Іваненко",
            ApplicantAge = 24,
            ContactInfo = "+380123456789"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var application = await db.AdoptionApplications.SingleAsync();
        Assert.Equal(13, application.UserId);
        Assert.Equal(AdoptionStatuses.Pending, application.Status);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static AdoptionController CreateController(ApplicationDbContext db, string? role = null, int? accountId = null)
    {
        var adoptionService = new AdoptionService(db);
        var petService = new PetService(db);

        var user = accountId.HasValue
            ? new ApplicationUser { Id = accountId.Value }
            : null;

        var userManager = GetUserManager(user, role);

        var httpContext = new DefaultHttpContext
        {
            User = CreatePrincipal(user, role),
            Session = new TestSession()
        };

        httpContext.Session.SetString("Role", role ?? "user");

        var controller = new AdoptionController(adoptionService, petService, userManager);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());

        return controller;
    }

    private static UserManager<ApplicationUser> GetUserManager(ApplicationUser? user, string? role)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();

        var manager = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!
        );

        manager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        return manager.Object;
    }

    private static ClaimsPrincipal CreatePrincipal(ApplicationUser? user, string? role)
    {
        if (user == null)
            return new ClaimsPrincipal(new ClaimsIdentity());

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        if (!string.IsNullOrEmpty(role))
            claims.Add(new Claim(ClaimTypes.Role, role));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
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
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
    }

    private class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}