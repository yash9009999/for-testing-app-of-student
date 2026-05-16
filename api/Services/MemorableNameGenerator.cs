using System.Security.Cryptography;

namespace api.Services;

public static class MemorableNameGenerator
{
    private static readonly string[] Adjectives =
    [
        "Happy", "Sunny", "Lucky", "Sweet", "Cool", "Tasty", "Fresh", "Golden",
        "Jolly", "Merry", "Bright", "Cheerful", "Delightful", "Lovely", "Pleasant",
        "Sparkling", "Fizzy", "Creamy", "Smooth", "Rich", "Divine", "Heavenly"
    ];

    private static readonly string[] Nouns =
    [
        "Penguin", "Dolphin", "Panda", "Koala", "Tiger", "Eagle", "Falcon", "Phoenix",
        "Dragon", "Unicorn", "Rainbow", "Sunset", "Ocean", "Mountain", "River", "Cloud",
        "Star", "Moon", "Galaxy", "Comet", "Breeze", "Thunder", "Lightning", "Aurora"
    ];

    /// <summary>
    /// SSD: <see cref="Random.Shared"/> is thread-safe from .NET 6+; we append cryptographic entropy so concurrent checkouts cannot trivially collide on the same adjective+noun+number.
    /// </summary>
    public static string Generate()
    {
        var adjective = Adjectives[Random.Shared.Next(Adjectives.Length)];
        var noun = Nouns[Random.Shared.Next(Nouns.Length)];
        var number = Random.Shared.Next(1000, 9999);
        var entropy = Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
        return $"{adjective}{noun}{number}{entropy}";
    }
}
