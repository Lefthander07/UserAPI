using Microsoft.EntityFrameworkCore;
using UserAPI.Models.Core;

namespace UserAPI.Data;

public class UsersDbContext : DbContext
{
    public const string SchemaName = "users";
    public UsersDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    public DbSet<User> Users { get; set; }
}