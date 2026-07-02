using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReceiptsApp.Domain.Entities;

namespace ReceiptsApp.Infrastructure.Receipts.Persistence.Configurations;

public class MarketConfiguration : IEntityTypeConfiguration<Market>
{
    public void Configure(EntityTypeBuilder<Market> builder)
    {
        builder.ToTable("Markets");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Country).HasMaxLength(100).IsRequired();

        builder.HasMany(m => m.Receipts)
            .WithOne(r => r.Market)
            .HasForeignKey(r => r.MarketId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("Receipts");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(r => r.Currency).HasMaxLength(3).IsRequired();

        builder.HasIndex(r => new { r.UserId, r.PurchasedAtUtc });

        builder.HasMany(r => r.Items)
            .WithOne()
            .HasForeignKey(i => i.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReceiptItemConfiguration : IEntityTypeConfiguration<ReceiptItem>
{
    public void Configure(EntityTypeBuilder<ReceiptItem> builder)
    {
        builder.ToTable("ReceiptItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Name).HasMaxLength(300).IsRequired();
        builder.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
    }
}
