using Microsoft.EntityFrameworkCore;

namespace SchulApp.DataAccess;

/// <summary>
/// Bringt die SQLite-Datenbank beim Start auf den aktuellen Stand. Das Schema
/// stammt nicht mehr aus einem CREATE-TABLE-Skript, sondern aus den EF-Core-
/// Migrations im Ordner Migrations.
/// </summary>
public static class DbInitializer
{
    /// <summary>Legt die Datenbank an bzw. führt alle noch fehlenden Migrations aus.</summary>
    public static void Initialize()
    {
        using var context = new AppDbContext();
        context.Database.Migrate();
    }
}
