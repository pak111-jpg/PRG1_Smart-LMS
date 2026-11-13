using Microsoft.EntityFrameworkCore;
using SchulApp.Models;

namespace SchulApp.DataAccess;

/// <summary>
/// Repository für die Tabelle Fach. GetById und Delete kommen unverändert aus
/// der Basisklasse Repository; hier steht nur, was für Fächer speziell ist.
/// </summary>
public class FachRepository : Repository<Fach>, IFachRepository
{
    // ---------- READ ----------

    /// <summary>Alle Fächer, alphabetisch.</summary>
    public override List<Fach> GetAll()
    {
        using var context = new AppDbContext();
        return context.Faecher
            .OrderBy(f => f.Name)
            .ToList();
    }

    /// <summary>
    /// Alle Fächer alphabetisch inkl. ihrer Dokumente. Include() lädt die Dokumente
    /// in derselben Abfrage mit - kein Nachladen pro Fach (N+1).
    /// </summary>
    public List<Fach> GetAlleMitDokumenten()
    {
        using var context = new AppDbContext();
        return context.Faecher
            .Include(f => f.Dokumente)
            .OrderBy(f => f.Name)
            .ToList();
    }

    public bool ExistiertName(string name, int ausserId = 0)
    {
        name = name.Trim();

        using var context = new AppDbContext();
        return context.Faecher.Any(f => f.Name == name && f.Id != ausserId);
    }

    // ---------- CREATE / UPDATE ----------

    public override void Add(Fach entity)
    {
        Bereinige(entity);
        if (string.IsNullOrWhiteSpace(entity.Erstellt))
            entity.Erstellt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        base.Add(entity);
    }

    public override void Update(Fach entity)
    {
        Bereinige(entity);
        base.Update(entity);
    }

    // Delete: Die Dokumente eines Fachs löscht die Datenbank über die Beziehung
    // gleich mit (ON DELETE CASCADE, siehe AppDbContext).

    // ---------- Hilfsmethode ----------

    /// <summary>Einheitliche Schreibweise vor dem Speichern: Texte ohne Rand-Leerzeichen.</summary>
    private static void Bereinige(Fach fach)
    {
        fach.Name = fach.Name.Trim();
        fach.Lehrperson = fach.Lehrperson?.Trim() ?? string.Empty;
    }
}
