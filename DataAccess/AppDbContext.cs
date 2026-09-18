using Microsoft.EntityFrameworkCore;

namespace SchulApp.DataAccess;

/// <summary>
/// Zugang zur Datenbank über Entity Framework Core. Ein Context ist für kurze,
/// abgeschlossene Operationen gedacht: pro DataAccess-Methode mit using erzeugen,
/// nie als dauerhaftes Feld halten.
/// </summary>
public class AppDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(DbInitializer.ConnectionString);
    }
}
