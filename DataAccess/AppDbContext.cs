using Microsoft.EntityFrameworkCore;
using SchulApp.Models;

namespace SchulApp.DataAccess;

/// <summary>
/// Zugang zur Datenbank über Entity Framework Core. Ein Context ist für kurze,
/// abgeschlossene Operationen gedacht: pro DataAccess-Methode mit using erzeugen,
/// nie als dauerhaftes Feld halten.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Fach> Faecher { get; set; } = null!;
    public DbSet<Dokument> Dokumente { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(DbInitializer.ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fach>(fach =>
        {
            // Tabellenname wie im bisherigen Schema (Einzahl), nicht der DbSet-Name.
            fach.ToTable("Fach");

            // NOCASE: Sortierung und Namensvergleich ignorieren Gross-/Kleinschreibung,
            // so wie vorher "COLLATE NOCASE" in den SQL-Abfragen.
            fach.Property(f => f.Name).UseCollation("NOCASE");
        });

        modelBuilder.Entity<Dokument>(dokument =>
        {
            dokument.ToTable("Dokument");
            dokument.Property(d => d.Titel).UseCollation("NOCASE");

            // 1:N-Beziehung: Ein Fach hat viele Dokumente. Wird das Fach gelöscht,
            // löscht die Datenbank seine Dokumente mit (ON DELETE CASCADE).
            dokument.HasOne(d => d.Fach)
                .WithMany(f => f.Dokumente)
                .HasForeignKey(d => d.FachId)
                .OnDelete(DeleteBehavior.Cascade);

            // Die Fristenübersicht sortiert und filtert nach Frist.
            dokument.HasIndex(d => d.Frist);
        });
    }
}
