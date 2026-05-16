using api.Models;

namespace api.Infrastructure.EntityFramework;

public static class SeedData
{
    /// <summary>
    /// SSD: idempotent seed — adds any missing catalog rows by name instead of bailing when the table is only partially populated.
    /// </summary>
    public static void Initialize(ApplicationDbContext context)
    {
        var catalog = BuildCatalog();
        var existingNames = context.Products.Select(p => p.Name).ToHashSet(StringComparer.Ordinal);
        var missing = catalog.Where(p => !existingNames.Contains(p.Name)).ToList();
        if (missing.Count == 0)
            return;

        context.Products.AddRange(missing);
        context.SaveChanges();

        // SSD: repair legacy comma-separated ingredient rows where commas appeared inside a single ingredient (JSON storage avoids this).
        var templatesByName = BuildCatalog().ToDictionary(p => p.Name, StringComparer.Ordinal);
        foreach (var entity in context.Products.Where(p => !p.IsDeleted).ToList())
        {
            if (templatesByName.TryGetValue(entity.Name, out var template))
                entity.Ingredients = new List<string>(template.Ingredients);
        }

        context.SaveChanges();
    }

    private static List<Product> BuildCatalog() =>
    [
        // Cones
        new Product("Waffle Cone", 2.00m, "A classic crispy waffle cone, perfect for any scoop.",
            new List<string> { "Wheat flour", "sugar", "vegetable oil", "eggs", "salt" },
            ProductType.CONE),
        new Product("Sugar Cone", 1.50m, "A sweet, crunchy cone with a light flavor.",
            new List<string> { "Wheat flour", "sugar", "corn syrup", "vegetable oil", "salt" },
            ProductType.CONE),
        new Product("Cup", 1.00m, "A simple cup for those who prefer no cone.",
            new List<string> { "Paper", "food-safe coating (beeswax, eggs, coconut oil)" },
            ProductType.CONE),

        // Flavors
        new Product("Vanilla", 1.00m, "Smooth and creamy classic vanilla ice cream.",
            new List<string> { "Milk", "cream", "sugar", "vanilla extract" },
            ProductType.FLAVOR),
        new Product("Chocolate", 1.00m, "Rich and indulgent chocolate ice cream.",
            new List<string> { "Milk", "cream", "sugar", "cocoa powder" },
            ProductType.FLAVOR),
        new Product("Strawberry", 1.00m, "Sweet and fruity strawberry ice cream.",
            new List<string> { "Milk", "cream", "sugar", "strawberries" },
            ProductType.FLAVOR),
        new Product("Mint Chocolate Chip", 1.20m, "Refreshing mint ice cream with chocolate chips.",
            new List<string> { "Milk", "cream", "sugar", "mint extract", "chocolate chips" },
            ProductType.FLAVOR),
        new Product("Salted Caramel", 1.25m, "Creamy caramel with a hint of sea salt.",
            new List<string> { "Milk", "cream", "sugar", "caramel", "sea salt" },
            ProductType.FLAVOR),
        new Product("Cookies & Cream", 1.25m, "Classic cookies folded into creamy ice cream.",
            new List<string> { "Milk", "cream", "sugar", "chocolate sandwich cookies" },
            ProductType.FLAVOR),
        new Product("Pistachio", 1.30m, "Nutty and smooth pistachio ice cream.",
            new List<string> { "Milk", "cream", "sugar", "pistachios (contains nuts)" },
            ProductType.FLAVOR),
        new Product("Lemon Sorbet", 1.10m, "Light and zesty dairy-free lemon sorbet.",
            new List<string> { "Water", "sugar", "lemon juice", "lemon zest" },
            ProductType.FLAVOR),

        // Toppings
        new Product("Sprinkles", 0.50m, "Colorful candy sprinkles for a fun finish.",
            new List<string> { "Sugar", "corn starch", "food coloring" },
            ProductType.TOPPING),
        new Product("Chocolate Chips", 0.75m, "Crunchy chocolate chips for extra delight.",
            new List<string> { "Sugar", "cocoa butter", "cocoa mass", "milk powder" },
            ProductType.TOPPING),
        new Product("Caramel Sauce", 0.80m, "Smooth and sweet caramel sauce drizzle.",
            new List<string> { "Sugar", "cream", "butter" },
            ProductType.TOPPING),
        new Product("Hot Fudge", 0.85m, "Warm chocolate fudge sauce.",
            new List<string> { "Sugar", "cocoa", "cream", "butter" },
            ProductType.TOPPING),
        new Product("Whipped Cream", 0.60m, "Light and fluffy whipped cream.",
            new List<string> { "Cream", "sugar" },
            ProductType.TOPPING),
        new Product("Chopped Nuts", 0.70m, "A crunchy mix of chopped nuts.",
            new List<string> { "Nuts", "cinnamon" },
            ProductType.TOPPING),
        new Product("Fresh Strawberries", 0.90m, "Sliced fresh strawberries.",
            new List<string> { "Strawberries", "sugar" },
            ProductType.TOPPING)
    ];
}
