using System.Text.Json;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace api.Infrastructure.EntityFramework;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    private static readonly JsonSerializerOptions IngredientJsonOptions = new();

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Price)
            .HasColumnName("price")
            .HasPrecision(19, 4)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.ProductType)
            .HasColumnName("product_type")
            .HasMaxLength(50)
            .HasConversion<string>();

        var ingredientsConverter = new ValueConverter<List<string>, string>(
            v => SerializeIngredients(v),
            v => DeserializeIngredients(v));

        builder.Property(p => p.Ingredients)
            .HasConversion(ingredientsConverter)
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (a, b) => a!.SequenceEqual(b!),
                v => v.Aggregate(0, (h, x) => HashCode.Combine(h, x.GetHashCode())),
                v => v.ToList()));

        builder.Property(p => p.Version)
            .IsRowVersion();
    }

    private static string SerializeIngredients(List<string> v) =>
        JsonSerializer.Serialize(v, IngredientJsonOptions);

    /// <summary>SSD: JSON array avoids CSV delimiter collisions; CSV rows still deserialize for older databases.</summary>
    private static List<string> DeserializeIngredients(string stored)
    {
        if (string.IsNullOrWhiteSpace(stored))
            return new List<string>();

        var s = stored.Trim();
        if (s.StartsWith('['))
        {
            try
            {
                return JsonSerializer.Deserialize<List<string>>(s, IngredientJsonOptions) ?? new List<string>();
            }
            catch (JsonException)
            {
                // Fall through to legacy CSV
            }
        }

        return stored.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}
