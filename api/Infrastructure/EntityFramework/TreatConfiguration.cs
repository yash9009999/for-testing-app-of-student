using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Infrastructure.EntityFramework;

/// <summary>
/// SSD: cascade delete from <see cref="Models.Order"/> to <see cref="Models.Treat"/> is intentional for cart cleanup — be careful using
/// hard deletes in admin tools so you do not wipe historical rows unintentionally (prefer soft-delete if you need retention).
/// </summary>
public class TreatConfiguration : IEntityTypeConfiguration<Treat>
{
    public void Configure(EntityTypeBuilder<Treat> builder)
    {
        builder.ToTable("treats");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.HasOne(t => t.Order)
            .WithMany(o => o.Treats)
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Products)
            .WithMany()
            .UsingEntity(
                "TreatProducts",
                l => l.HasOne(typeof(Product)).WithMany().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne(typeof(Treat)).WithMany().HasForeignKey("TreatId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasKey("TreatId", "ProductId")
            );
    }
}
