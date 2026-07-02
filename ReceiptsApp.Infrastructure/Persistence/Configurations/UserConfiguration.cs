using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReceiptsApp.Domain.Entities;

namespace ReceiptsApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Keeps EF Core mapping concerns (column types, indexes, max lengths) out
/// of the DbContext and out of the Domain entity itself.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.PasswordHash).IsRequired();
    }
}
