using SchulApp.Models;

namespace SchulApp.DataAccess;

/// <summary>
/// Sämtliche Datenbankzugriffe auf die Tabelle Fach - über AppDbContext und LINQ,
/// ohne SQL-Strings. Pro Methode ein eigener, kurzlebiger Context.
/// </summary>
public class FachDataAccess
{
    // ---------- READ ----------

    /// <summary>Alle Fächer inkl. Anzahl der zugehörigen Dokumente, alphabetisch.</summary>
    public List<Fach> GetAll()
    {
        using var context = new AppDbContext();

        // Die Anzahl wird in derselben Abfrage mitgezählt - kein Nachladen pro Fach (N+1).
        var zeilen = context.Faecher
            .OrderBy(f => f.Name)
            .Select(f => new { Fach = f, Anzahl = f.Dokumente.Count })
            .ToList();

        foreach (var zeile in zeilen)
            zeile.Fach.AnzahlDokumente = zeile.Anzahl;

        return zeilen.Select(zeile => zeile.Fach).ToList();
    }

    public Fach? GetById(int id)
    {
        using var context = new AppDbContext();
        return context.Faecher.Find(id);
    }

    /// <summary>Prüft, ob ein Fachname bereits vergeben ist (optional eigene Id ausschliessen).</summary>
    public bool ExistiertName(string name, int ausserId = 0)
    {
        name = name.Trim();

        using var context = new AppDbContext();
        return context.Faecher.Any(f => f.Name == name && f.Id != ausserId);
    }

    // ---------- CREATE ----------

    /// <summary>Fügt ein Fach ein und liefert die neue Id zurück.</summary>
    public int Add(Fach fach)
    {
        fach.Name = fach.Name.Trim();
        fach.Lehrperson = fach.Lehrperson?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fach.Erstellt))
            fach.Erstellt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        using var context = new AppDbContext();
        context.Faecher.Add(fach);
        context.SaveChanges();   // EF Core trägt die neue Id direkt in fach.Id ein

        return fach.Id;
    }

    // ---------- UPDATE ----------

    public void Update(Fach fach)
    {
        using var context = new AppDbContext();
        var vorhanden = context.Faecher.Find(fach.Id);
        if (vorhanden == null) return;

        vorhanden.Name = fach.Name.Trim();
        vorhanden.Lehrperson = fach.Lehrperson?.Trim() ?? string.Empty;
        context.SaveChanges();
    }

    // ---------- DELETE ----------

    /// <summary>
    /// Löscht ein Fach. Seine Dokumente löscht die Datenbank über die
    /// Beziehung gleich mit (ON DELETE CASCADE, siehe AppDbContext).
    /// </summary>
    public void Delete(int id)
    {
        using var context = new AppDbContext();
        var fach = context.Faecher.Find(id);
        if (fach == null) return;

        context.Faecher.Remove(fach);
        context.SaveChanges();
    }
}
