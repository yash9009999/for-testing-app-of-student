using Microsoft.EntityFrameworkCore;

namespace api.Infrastructure.EntityFramework;

/// <summary>SSD: dev SQLite databases created before schema changes need lightweight column patches (EnsureCreated does not alter existing files).</summary>
public static class SqliteSchemaPatcher
{
    private static readonly (string Column, string SqlType)[] OrderColumns =
    [
        ("guest_access_token", "TEXT"),
        ("paid_at", "TEXT")
    ];

    public static void Apply(ApplicationDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        connection.Open();
        try
        {
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA table_info(orders);";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    existing.Add(reader.GetString(1));
            }

            foreach (var (column, sqlType) in OrderColumns)
            {
                if (existing.Contains(column))
                    continue;

                context.Database.ExecuteSqlRaw($"ALTER TABLE orders ADD COLUMN {column} {sqlType};"); // column names are compile-time constants
            }
        }
        finally
        {
            connection.Close();
        }
    }
}
