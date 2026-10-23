using Microsoft.EntityFrameworkCore;

namespace SchulApp.DataAccess;

/// <summary>
/// Bringt die SQLite-Datenbank beim Start auf den aktuellen Stand. Das Schema
/// stammt nicht mehr aus einem CREATE-TABLE-Skript, sondern aus den EF-Core-
/// Migrations im Ordner Migrations.
/// </summary>
public static class DbInitializer
{
    // Neuer Dateiname ab K2: Die alte app.db wurde per SQL-Skript angelegt und kennt
    // keine Migrations - Migrate() würde dort an "table Fach already exists" scheitern.
    // Die alte Datei bleibt unangetastet liegen.
    public const string ConnectionString = "Data Source=schulapp.db";

    /// <summary>Legt die Datenbank an bzw. führt alle noch fehlenden Migrations aus.</summary>
    public static void Initialize()
    {
        using var context = new AppDbContext();
        context.Database.Migrate();
    }
}
