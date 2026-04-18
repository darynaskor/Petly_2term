using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Petly.DataAccess.Data;
using Petly.Models;

namespace Petly.DataAccess;

public static class FullDbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        string[] roles = { "user", "shelter_admin", "system_admin" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
        }

        var adminEmail = "admin@petly.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, "Admin123!");
        }

        var adminRoles = await userManager.GetRolesAsync(admin);
        await userManager.RemoveFromRolesAsync(admin, adminRoles);
        await userManager.AddToRoleAsync(admin, "system_admin");

        string[] shelterAdmins =
        {
            "happypaws@petly.com",
            "domivka@petly.com",
            "bestfriends@petly.com"
        };

        foreach (var email in shelterAdmins)
        {
            var shelterAdmin = await userManager.FindByEmailAsync(email);

            if (shelterAdmin == null)
            {
                shelterAdmin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(shelterAdmin, "Admin123!");
            }

            var rolesUser = await userManager.GetRolesAsync(shelterAdmin);
            await userManager.RemoveFromRolesAsync(shelterAdmin, rolesUser);
            await userManager.AddToRoleAsync(shelterAdmin, "shelter_admin");
        }

        string[] users =
        {
            "skorobogatuh.dara@gmail.com",
            "karpiakmarta23@gmail.com",
            "pzakutynskyi@gmail.com",
            "haroqub@gmail.com",
            "magockadiana@gmail.com"
        };

        foreach (var email in users)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user, "User123!");
            }

            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            await userManager.AddToRoleAsync(user, "user");
        }

        var admin1 = await userManager.FindByEmailAsync("happypaws@petly.com");
        var admin2 = await userManager.FindByEmailAsync("domivka@petly.com");
        var admin3 = await userManager.FindByEmailAsync("bestfriends@petly.com");

        context.Pets.RemoveRange(context.Pets);
        context.Shelters.RemoveRange(context.Shelters);
        await context.SaveChangesAsync();

        var shelter1 = new Shelter
        {
            AccountId = admin1.Id,
            ShelterName = "Happy Paws",
            Location = "Lviv",
            AdminName = "Admin"
        };

        var shelter2 = new Shelter
        {
            AccountId = admin2.Id,
            ShelterName = "Домівка",
            Location = "Kyiv",
            AdminName = "Admin"
        };

        var shelter3 = new Shelter
        {
            AccountId = admin3.Id,
            ShelterName = "Best Friends",
            Location = "Lviv",
            AdminName = "Admin"
        };

        context.Shelters.AddRange(shelter1, shelter2, shelter3);
        await context.SaveChangesAsync();

        context.Pets.AddRange(

            new Pet { ShelterId = shelter1.AccountId, PetName = "Макс", Type = "Собака", Breed = "Лабрадор", Gender = "Чоловіча", Age = 3, Size = "Великий", Vaccinated = true, Sterilized = true, Status = "Доступний", PhotoUrl = "/images/pets/max.jpg" },
            new Pet { ShelterId = shelter1.AccountId, PetName = "Луна", Type = "Кіт", Breed = "Мішана", Gender = "Жіноча", Age = 2, Size = "Малий", Vaccinated = true, Sterilized = false, Status = "Доступний", PhotoUrl = "/images/pets/luna.jpg" },
            new Pet { ShelterId = shelter1.AccountId, PetName = "Рокі", Type = "Собака", Breed = "Вівчарка", Gender = "Чоловіча", Age = 4, Size = "Великий", Vaccinated = true, Sterilized = true, Status = "Доступний", PhotoUrl = "/images/pets/rocky.jpg" },

            new Pet { ShelterId = shelter2.AccountId, PetName = "Белла", Type = "Собака", Breed = "Родезійський ріджбек", Gender = "Жіноча", Age = 4, Size = "Середній", Vaccinated = true, Sterilized = true, Status = "Доступний", PhotoUrl = "/images/pets/bella.jpg" },
            new Pet { ShelterId = shelter2.AccountId, PetName = "Чарлі", Type = "Собака", Breed = "Такса", Gender = "Чоловіча", Age = 4, Size = "Малий", Vaccinated = true, Sterilized = true, Status = "Доступний", PhotoUrl = "/images/pets/charlie.jpg" },
            new Pet { ShelterId = shelter2.AccountId, PetName = "Дейзі", Type = "Собака", Breed = "Золотистий ретривер", Gender = "Жіноча", Age = 12, Size = "Середній", Vaccinated = true, Sterilized = true, Status = "Доступний", PhotoUrl = "/images/pets/daisy.jpg" },

            new Pet { ShelterId = shelter3.AccountId, PetName = "Люсі", Type = "Кіт", Breed = "Мішана", Gender = "Жіноча", Age = 4, Size = "Малий", Vaccinated = true, Sterilized = true, Status = "Доступний", PhotoUrl = "/images/pets/lucy.jpg" },
            new Pet { ShelterId = shelter3.AccountId, PetName = "Олівер", Type = "Кіт", Breed = "Мішаний", Gender = "Чоловіча", Age = 8, Size = "Великий", Vaccinated = true, Sterilized = true, Status = "Доступний", PhotoUrl = "/images/pets/oliver.jpg" },
            new Pet { ShelterId = shelter3.AccountId, PetName = "Сімба", Type = "Кіт", Breed = "Мішаний", Gender = "Чоловіча", Age = 5, Size = "Середній", Vaccinated = true, Sterilized = true, Status = "Доступний", PhotoUrl = "/images/pets/simba.jpg" },
            new Pet { ShelterId = shelter3.AccountId, PetName = "Майло", Type = "Кіт", Breed = "Мішаний", Gender = "Чоловіча", Age = 1, Size = "Малий", Vaccinated = true, Sterilized = false, Status = "Доступний", PhotoUrl = "/images/pets/milo.jpg" }
        );

        await context.SaveChangesAsync();
    }
}