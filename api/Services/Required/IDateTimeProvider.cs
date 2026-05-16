namespace api.Services.Required;

public interface IDateTimeProvider
{
    /// <summary>
    /// SSD: unambiguous UTC clock — <see cref="DateTimeOffset"/> carries offset explicitly so order timestamps cannot silently pick up Local <see cref="DateTime.Kind"/>.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
