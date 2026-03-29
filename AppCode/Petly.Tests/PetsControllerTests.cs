using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
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
        var controller = new PetsController(service);

        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        if (!string.IsNullOrEmpty(role)) httpContext.Session.SetString("Role", role);
        if (accountId.HasValue) httpContext.Session.SetInt32("AccountId", accountId.Value);

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());

        return controller;
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

    private class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _data = new();
        public IEnumerable<string> Keys => _data.Keys;
        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;
        public void Clear() => _data.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _data.Remove(key);
        public void Set(string key, byte[] value) => _data[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _data.TryGetValue(key, out value);
    }

    private class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
