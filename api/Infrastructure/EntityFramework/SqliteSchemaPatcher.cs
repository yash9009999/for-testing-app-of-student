using Microsoft.EntityFrameworkCore;

namespace api.Infrastructure.EntityFramework;

/// <summary>SSD: dev SQLite databases created before schema changes need lightweight column patches (EnsureCreated does not alter existing files).</summary>
public static class SqliteSchemaPatcher
{
    public static void EnsureOrderGuestAccessTokenColumn(ApplicationDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        connection.Open();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA table_info(orders);";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(1);
                if (string.Equals(name, "guest_access_token", StringComparison.OrdinalIgnoreCase))
                    return;
            }
        }
        finally
        {
            connection.Close();
        }

        context.Database.ExecuteSqlRaw("ALTER TABLE orders ADD COLUMN guest_access_token TEXT;");
    }
}
