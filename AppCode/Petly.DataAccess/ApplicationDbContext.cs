using Microsoft.EntityFrameworkCore;
using Petly.Models;

namespace Petly.DataAccess.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Pet> Pets { get; set; }
    public DbSet<AdoptionApplication> AdoptionApplications { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<UserProfile> Users { get; set; }
    public DbSet<Shelter> Shelters { get; set; }
    public DbSet<ShelterNeed> ShelterNeeds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserProfile>()
            .HasOne(up => up.Account)
            .WithOne(a => a.UserProfile)
            .HasForeignKey<UserProfile>(up => up.AccountId);

        modelBuilder.Entity<Account>()
            .Property(a => a.Role)
            .HasDefaultValue("user");

        modelBuilder.Entity<ShelterNeed>()
            .HasOne(sn => sn.Shelter)
            .WithMany()
            .HasForeignKey(sn => sn.ShelterId);
    }
}
