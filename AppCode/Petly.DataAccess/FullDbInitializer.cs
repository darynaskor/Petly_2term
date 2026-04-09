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

        // SYSTEM ADMIN
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

        // SHELTER ADMIN
        var shelterAdminEmail = "shelter_admin@gmail.com";
        var shelterAdmin = await userManager.FindByEmailAsync(shelterAdminEmail);

        if (shelterAdmin == null)
        {
            shelterAdmin = new ApplicationUser
            {
                UserName = shelterAdminEmail,
                Email = shelterAdminEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(shelterAdmin, "Admin123!");
        }

        var shelterRoles = await userManager.GetRolesAsync(shelterAdmin);
        await userManager.RemoveFromRolesAsync(shelterAdmin, shelterRoles);
        await userManager.AddToRoleAsync(shelterAdmin, "shelter_admin");

        //USERS
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

        // DATA
        if (!context.Shelters.Any())
        {
            var shelter = new Shelter
            {
                AccountId = shelterAdmin.Id,
                ShelterName = "Happy Pets Shelter",
                Location = "Lviv",
                AdminName = "Shelter Admin"
            };

            context.Shelters.Add(shelter);
            await context.SaveChangesAsync();

            context.ShelterNeeds.Add(new ShelterNeed
            {
                ShelterId = shelter.AccountId,
                Description = "Потрібна їжа",
                PaymentDetails = "Карта: 1111 2222 3333 4444"
            });

            context.Pets.AddRange(
                new Pet
                {
                    ShelterId = shelter.AccountId,
                    PetName = "Макс",
                    Type = "Собака",
                    Breed = "Лабрадор",
                    Gender = "Чоловіча",
                    Age = 3,
                    Size = "Великий",
                    Vaccinated = true,
                    Sterilized = true,
                    Status = "Доступний",
                    PhotoUrl = "/images/pets/max.jpg"
                },
                new Pet
                {
                    ShelterId = shelter.AccountId,
                    PetName = "Луна",
                    Type = "Кіт",
                    Breed = "Сіамський",
                    Gender = "Жіноча",
                    Age = 2,
                    Size = "Малий",
                    Vaccinated = true,
                    Sterilized = false,
                    Status = "Доступний",
                    PhotoUrl = "/images/pets/luna.jpg"
                },
                new Pet
                {
                    ShelterId = shelter.AccountId,
                    PetName = "Рокі",
                    Type = "Собака",
                    Breed = "Бульдог",
                    Gender = "Чоловіча",
                    Age = 4,
                    Size = "Середній",
                    Vaccinated = true,
                    Sterilized = true,
                    Status = "Доступний",
                    PhotoUrl = "/images/pets/rocky.jpg"
                },
                new Pet
                {
                    ShelterId = shelter.AccountId,
                    PetName = "Белла",
                    Type = "Собака",
                    Breed = "Бульдог",
                    Gender = "Жіноча",
                    Age = 4,
                    Size = "Середній",
                    Vaccinated = true,
                    Sterilized = true,
                    Status = "Доступний",
                    PhotoUrl = "/images/pets/bella.jpg"
                },
                new Pet
                {
                    ShelterId = shelter.AccountId,
                    PetName = "Чарлі",
                    Type = "Собака",
                    Breed = "Бульдог",
                    Gender = "Чоловіча",
                    Age = 4,
                    Size = "Середній",
                    Vaccinated = true,
                    Sterilized = true,
                    Status = "Доступний",
                    PhotoUrl = "/images/pets/charlie.jpg"
                },
                new Pet
                {
                    ShelterId = shelter.AccountId,
                    PetName = "Дейзі",
                    Type = "Собака",
                    Breed = "Бульдог",
                    Gender = "Жіноча",
                    Age = 12,
                    Size = "Середній",
                    Vaccinated = true,
                    Sterilized = true,
                    Status = "Доступний",
                    PhotoUrl = "/images/pets/daisy.jpg"
                },
                new Pet
                {
                    ShelterId = shelter.AccountId,
                    PetName = "Люсі",
                    Type = "Кіт",
                    Breed = "Мішана",
                    Gender = "Жіноча",
                    Age = 4,
                    Size = "Малий",
                    Vaccinated = true,
                    Sterilized = true,
                    Status = "Доступний",
                    PhotoUrl = "/images/pets/lucy.jpg"
                },
                new Pet
                {
                    ShelterId = shelter.AccountId,
                    PetName = "Олівер",
                    Type = "Собака",
                    Breed = "Мішаний",
                    Gender = "Чоловіча",
                    Age = 8,
                    Size = "Великий",
                    Vaccinated = true,
                    Sterilized = true,
                    Status = "Доступний",
                    PhotoUrl = "/images/pets/oliver.jpg"
                },
                new Pet
                {
                    ShelterId = shelter.AccountId,
                    PetName = "Сімба",
                    Type = "Кіт",
                    Breed = "Мішаний",
                    Gender = "Чоловіча",
                    Age = 5,
                    Size = "Середній",
                    Vaccinated = true,
                    Sterilized = true,
                    Status = "Доступний",
                    PhotoUrl = "/images/pets/simba.jpg"
                },
                new Pet
                {
                    ShelterId = shelter.AccountId,
                    PetName = "Майло",
                    Type = "Кіт",
                    Breed = "Британський",
                    Gender = "Чоловіча",
                    Age = 1,
                    Size = "Малий",
                    Vaccinated = true,
                    Sterilized = false,
                    Status = "Доступний",
                    PhotoUrl = "/images/pets/milo.jpg"
                }
            );

            await context.SaveChangesAsync();

            var user = await userManager.FindByEmailAsync("haroqub@gmail.com");

            context.AdoptionApplications.Add(new AdoptionApplication
            {
                UserId = user.Id,
                PetId = context.Pets.First().PetId,
                Status = AdoptionStatuses.Pending,
                ApplicantName = user.Name ?? "Користувач",
                ApplicantSurname = user.Surname ?? "Petly",
                ApplicantAge = 24,
                ContactInfo = user.Email ?? "petly@example.com"
            });

            await context.SaveChangesAsync();
        }
    }
}
