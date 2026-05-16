namespace api.Models;

public class Product
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public List<string> Ingredients { get; set; } = new();

    public ProductType ProductType { get; set; }

    public long? Version { get; set; }

    public bool IsDeleted { get; set; }

    protected Product()
    {
        // For deserialization
    }

    public Product(string name, decimal price, string? description, List<string> ingredients, ProductType productType)
    {
        Name = name;
        Price = price;
        Description = description;
        Ingredients = ingredients ?? new List<string>();
        ProductType = productType;
    }
}
