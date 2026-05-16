using api.Services.Required;

namespace api.Infrastructure.SystemDateTimeProvider;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
