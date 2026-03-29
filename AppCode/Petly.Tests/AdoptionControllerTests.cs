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
        db.Pets.AddRange(
            new Pet
            {
                PetId = 1,
                ShelterId = 10,
                PetName = "Barsik",
                Status = "Available"
            },
            new Pet
            {
                PetId = 2,
                ShelterId = 20,
                PetName = "Luna",
                Status = "Available"
            });
        db.AdoptionApplications.AddRange(
            new AdoptionApplication
            {
                AdoptId = 11,
                UserId = 7,
                PetId = 1,
                Status = "Очікує",
                SubmissionDate = new DateTime(2026, 3, 25)
            },
            new AdoptionApplication
            {
                AdoptId = 12,
                UserId = 8,
                PetId = 2,
                Status = "Схвалено",
                SubmissionDate = new DateTime(2026, 3, 24)
            });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "user", 7);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Adoption", view.ViewName);
        var model = Assert.IsType<List<AdoptionApplication>>(view.Model);
        Assert.False((bool)controller.ViewBag.IsAdmin);
        var application = Assert.Single(model);
        Assert.Equal(11, application.AdoptId);
        Assert.Equal(7, application.UserId);
        Assert.Equal("Barsik", application.Pet!.PetName);
    }

    [Fact]
    public async Task ViewShelterApplications()
    {
        await using var db = CreateDbContext();
        db.Pets.AddRange(
            new Pet
            {
                PetId = 3,
                ShelterId = 15,
                PetName = "Rex",
                Status = "Available"
            },
            new Pet
            {
                PetId = 4,
                ShelterId = 16,
                PetName = "Murka",
                Status = "Available"
            });
        db.AdoptionApplications.AddRange(
            new AdoptionApplication
            {
                AdoptId = 21,
                UserId = 2,
                PetId = 3,
                Status = "Очікує",
                SubmissionDate = new DateTime(2026, 3, 26)
            },
            new AdoptionApplication
            {
                AdoptId = 22,
                UserId = 3,
                PetId = 4,
                Status = "Очікує",
                SubmissionDate = new DateTime(2026, 3, 25)
            });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "shelter_admin", 15);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Adoption", view.ViewName);
        var model = Assert.IsType<List<AdoptionApplication>>(view.Model);
        Assert.True((bool)controller.ViewBag.IsAdmin);
        var application = Assert.Single(model);
        Assert.Equal(21, application.AdoptId);
        Assert.Equal("Rex", application.Pet!.PetName);
    }

    [Fact]
    public async Task OpenAdoptPage()
    {
        await using var db = CreateDbContext();
        db.Pets.Add(new Pet
        {
            PetId = 5,
            ShelterId = 30,
            PetName = "Simba",
            Type = "Кіт",
            Status = "Available"
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "user", 9);

        var result = await controller.Create(5);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Adopt", view.ViewName);
        var model = Assert.IsType<Pet>(view.Model);
        Assert.Equal(5, model.PetId);
        Assert.Equal("Simba", model.PetName);
    }

    [Fact]
    public async Task OpenAdoptPagePetNotFound()
    {
        await using var db = CreateDbContext();
        var controller = CreateController(db, "user", 9);

        var result = await controller.Create(404);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AdoptPet()
    {
        await using var db = CreateDbContext();
        db.Pets.Add(new Pet
        {
            PetId = 6,
            ShelterId = 41,
            PetName = "Daisy",
            Status = "Available"
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "user", 13);

        var result = await controller.Adopt(6);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Вашу заявку успішно подано!", controller.TempData["Success"]);

        var application = await db.AdoptionApplications.SingleAsync();
        Assert.Equal(6, application.PetId);
        Assert.Equal(13, application.UserId);
        Assert.Equal("Очікує", application.Status);
    }

    [Fact]
    public async Task ApproveApplication()
    {
        await using var db = CreateDbContext();
        db.AdoptionApplications.Add(new AdoptionApplication
        {
            AdoptId = 31,
            UserId = 4,
            PetId = 8,
            Status = "Очікує",
            SubmissionDate = new DateTime(2026, 3, 25)
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "shelter_admin", 50);

        var result = await controller.Approve(31);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Заявку схвалено!", controller.TempData["Success"]);

        var application = await db.AdoptionApplications.SingleAsync(x => x.AdoptId == 31);
        Assert.Equal("Схвалено", application.Status);
    }

    [Fact]
    public async Task RejectApplicationWithoutAccess()
    {
        await using var db = CreateDbContext();
        db.AdoptionApplications.Add(new AdoptionApplication
        {
            AdoptId = 32,
            UserId = 5,
            PetId = 9,
            Status = "Очікує",
            SubmissionDate = new DateTime(2026, 3, 25)
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "user", 5);

        var result = await controller.Reject(32);

        Assert.IsType<ForbidResult>(result);
        var application = await db.AdoptionApplications.SingleAsync(x => x.AdoptId == 32);
        Assert.Equal("Очікує", application.Status);
    }

    [Fact]
    public async Task DeleteOwnApplication()
    {
        await using var db = CreateDbContext();
        db.AdoptionApplications.AddRange(
            new AdoptionApplication
            {
                AdoptId = 41,
                UserId = 14,
                PetId = 12,
                Status = "Очікує",
                SubmissionDate = new DateTime(2026, 3, 25)
            },
            new AdoptionApplication
            {
                AdoptId = 42,
                UserId = 99,
                PetId = 13,
                Status = "Очікує",
                SubmissionDate = new DateTime(2026, 3, 24)
            });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "user", 14);

        var result = await controller.Delete(41);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Вашу заявку успішно скасовано.", controller.TempData["Success"]);
        Assert.False(await db.AdoptionApplications.AnyAsync(x => x.AdoptId == 41));
        Assert.True(await db.AdoptionApplications.AnyAsync(x => x.AdoptId == 42));
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
        var controller = new AdoptionController(adoptionService, petService);
        var httpContext = new DefaultHttpContext
        {
            Session = new TestSession()
        };

        if (!string.IsNullOrWhiteSpace(role))
        {
            httpContext.Session.SetString("Role", role);
        }

        if (accountId.HasValue)
        {
            httpContext.Session.SetInt32("AccountId", accountId.Value);
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());

        return controller;
    }

    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public IEnumerable<string> Keys => _store.Keys;

        public string Id { get; } = Guid.NewGuid().ToString();

        public bool IsAvailable => true;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[]? value) => _store.TryGetValue(key, out value);
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
