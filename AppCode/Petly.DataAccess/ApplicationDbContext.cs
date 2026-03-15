using Microsoft.EntityFrameworkCore;
using Petly.Models;

namespace Petly.DataAccess.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Pet> Pets { get; set; }
    // Можна додати DbSet<Account>, DbSet<Shelter> і т.д.
}