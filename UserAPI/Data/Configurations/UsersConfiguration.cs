

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserAPI.Models.Core;
using System.Reflection.Emit;


namespace UserAPI.Data.Configurations;



public class UsersConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(p => p.Id);
        // Например, можно задать ограничения уникальности:
        builder
            .HasIndex(u => u.Login)
            .IsUnique();
    }
}
