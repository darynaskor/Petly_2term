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

public class PetsControllerTests
{
    [Fact]
    public async Task GetAllPets_AsAdmin()
    {
        await using var db = CreateDbContext();
        await AddPetsAsync(db, new Pet { PetId = 1, ShelterId = 10, PetName = "Barsik" },
                               new Pet { PetId = 2, ShelterId = 20, PetName = "Luna" });

        var controller = CreateController(db, "system_admin");

        var model = await GetViewModel<List<Pet>>(controller.Index());

        Assert.Equal(2, model.Count);
    }

    [Fact]
    public async Task GetPets_ByShelterAdmin()
    {
        await using var db = CreateDbContext();
        await AddPetsAsync(db, new Pet { PetId = 1, ShelterId = 5, PetName = "A" },
                               new Pet { PetId = 2, ShelterId = 6, PetName = "B" });

        var controller = CreateController(db, "shelter_admin", 5);

        var model = await GetViewModel<List<Pet>>(controller.Index());

        Assert.Single(model);
        Assert.Equal(5, model[0].ShelterId);
    }

    [Fact]
    public async Task GetPetDetails()
    {
        await using var db = CreateDbContext();
        await AddPetsAsync(db, new Pet { PetId = 1, PetName = "Rex" });

        var controller = CreateController(db);

        var model = await GetViewModel<Pet>(controller.Details(1));

        Assert.Equal("Rex", model.PetName);
    }

    [Fact]
    public async Task CreatePet()
    {
        await using var db = CreateDbContext();
        var controller = CreateController(db, "shelter_admin", 7);

        var pet = new Pet { PetName = "NewPet" };

        var result = await controller.Create(pet);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var saved = await db.Pets.SingleAsync();
        Assert.Equal("NewPet", saved.PetName);
        Assert.Equal(7, saved.ShelterId);
    }

    [Fact]
    public async Task CreatePet_WithoutAccess()
    {
        await using var db = CreateDbContext();
        var controller = CreateController(db, "user");

        var result = await controller.Create(new Pet());

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task EditPet()
    {
        await using var db = CreateDbContext();
        await AddPetsAsync(db, new Pet { PetId = 1, ShelterId = 5, PetName = "Old" });

        var controller = CreateController(db, "shelter_admin", 5);
        var updated = new Pet { PetId = 1, PetName = "New" };

        var result = await controller.Edit(updated);
        Assert.IsType<RedirectToActionResult>(result);

        var pet = await db.Pets.FindAsync(1);
        Assert.Equal("New", pet!.PetName);
    }

    [Fact]
    public async Task EditPet_Forbidden()
    {
        await using var db = CreateDbContext();
        await AddPetsAsync(db, new Pet { PetId = 1, ShelterId = 10 });

        var controller = CreateController(db, "shelter_admin", 5);

        var result = await controller.Edit(new Pet { PetId = 1 });
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeletePet()
    {
        await using var db = CreateDbContext();
        await AddPetsAsync(db, new Pet { PetId = 1, ShelterId = 5 });

        var controller = CreateController(db, "shelter_admin", 5);

        var result = await controller.DeleteConfirmed(1);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.False(await db.Pets.AnyAsync());
    }

    [Fact]
    public async Task DeletePet_Forbidden()
    {
        await using var db = CreateDbContext();
        await AddPetsAsync(db, new Pet { PetId = 1, ShelterId = 10 });

        var controller = CreateController(db, "shelter_admin", 5);

        var result = await controller.DeleteConfirmed(1);
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task NotFoundTests()
    {
        await using var db = CreateDbContext();
        var controller = CreateController(db, "shelter_admin", 5);

        Assert.IsType<NotFoundResult>(await controller.Details(999));
        Assert.IsType<NotFoundResult>(await controller.Edit(new Pet { PetId = 999 }));
        Assert.IsType<NotFoundResult>(await controller.DeleteConfirmed(999));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static PetsController CreateController(ApplicationDbContext db, string? role = null, int? accountId = null)
    {
        var service = new PetService(db);
        var normalizedAccountId = role != null && !accountId.HasValue ? 1 : accountId;
        var currentUser = normalizedAccountId.HasValue
            ? new ApplicationUser
            {
                Id = normalizedAccountId.Value,
                UserName = $"user{normalizedAccountId.Value}@petly.test",
                Email = $"user{normalizedAccountId.Value}@petly.test"
            }
            : null;
        var userManager = CreateUserManager(currentUser, role);
        var controller = new PetsController(service, userManager);

        var httpContext = new DefaultHttpContext { User = CreatePrincipal(currentUser, role) };

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());

        return controller;
    }

    private static UserManager<ApplicationUser> CreateUserManager(ApplicationUser? currentUser, string? role)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var manager = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        manager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(currentUser);
        manager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(role == null
                ? new List<string>()
                : new List<string> { role });

        return manager.Object;
    }

    private static System.Security.Claims.ClaimsPrincipal CreatePrincipal(ApplicationUser? currentUser, string? role)
    {
        if (currentUser == null)
        {
            return new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
        }

        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, currentUser.Id.ToString()),
            new(System.Security.Claims.ClaimTypes.Name, currentUser.UserName ?? currentUser.Email ?? currentUser.Id.ToString())
        };

        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
        }

        return new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(claims, "TestAuth"));
    }

    private static async Task AddPetsAsync(ApplicationDbContext db, params Pet[] pets)
    {
        db.Pets.AddRange(pets);
        await db.SaveChangesAsync();
    }

    private static async Task<T> GetViewModel<T>(Task<IActionResult> action)
    {
        var result = await action;
        var view = Assert.IsType<ViewResult>(result);
        return Assert.IsType<T>(view.Model);
    }

    private class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
