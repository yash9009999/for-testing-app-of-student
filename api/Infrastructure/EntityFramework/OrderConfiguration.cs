using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Infrastructure.EntityFramework;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderTime)
            .HasColumnName("order_time")
            .IsRequired(false);

        builder.Property(o => o.OrderTotal)
            .HasColumnName("order_total")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(o => o.DeliveryCost)
            .HasColumnName("delivery_cost")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(o => o.EstDeliveryMinutes)
            .HasColumnName("est_delivery_minutes");

        builder.Property(o => o.Promotion)
            .HasColumnName("promotion")
            .HasMaxLength(64);

        builder.Property(o => o.MemorableName)
            .HasColumnName("memorable_name")
            .HasMaxLength(120);

        builder.Property(o => o.GuestAccessToken)
            .HasColumnName("guest_access_token")
            .HasMaxLength(64);

        builder.Property(o => o.PaidAt)
            .HasColumnName("paid_at");

        builder.Property(o => o.Version)
            .IsRowVersion();

        builder.HasMany(o => o.Treats)
            .WithOne(t => t.Order)
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
