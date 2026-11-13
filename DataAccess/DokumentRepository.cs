using Microsoft.EntityFrameworkCore;
using SchulApp.Models;

namespace SchulApp.DataAccess;

/// <summary>
/// Repository für die Tabelle Dokument inkl. Fristenübersicht, Suche und dem
/// Workflow "als abgegeben markieren". GetAll, GetById und Delete kommen
/// unverändert aus der Basisklasse Repository.
/// </summary>
public class DokumentRepository : Repository<Dokument>, IDokumentRepository
{
    // ---------- READ ----------

    public List<Dokument> GetByFach(int fachId)
    {
        using var context = new AppDbContext();
        return context.Dokumente
            .Where(d => d.FachId == fachId)
            .OrderBy(d => d.Abgegeben)
            .ThenBy(d => d.Frist == null || d.Frist == "")   // Dokumente ohne Frist ans Ende
            .ThenBy(d => d.Frist)
            .ThenBy(d => d.Titel)
            .ToList();
    }

    /// <summary>
    /// Fristenübersicht. Include() lädt das zugehörige Fach in derselben Abfrage mit
    /// (JOIN über die 1:N-Beziehung) - dadurch ist Dokument.Fach gefüllt.
    /// </summary>
    public List<Dokument> GetOffeneFristen()
    {
        using var context = new AppDbContext();
        return context.Dokumente
            .Include(d => d.Fach)
            .Where(d => !d.Abgegeben && d.Frist != null && d.Frist != "")
            .OrderBy(d => d.Frist)
            .ToList();
    }

    /// <summary>
    /// Gesamtübersicht über alle Fächer: optional nach Titel gesucht, nach Status
    /// gefiltert und sortiert. Die Abfrage wird Schritt für Schritt zusammengesetzt
    /// und erst mit ToList() als ein einziges SQL-Statement ausgeführt.
    /// </summary>
    public List<Dokument> Suche(string suchtext, DokumentFilter filter, DokumentSortierung sortierung)
    {
        // "yyyy-MM-dd" sortiert als Text gleich wie als Datum, deshalb reicht ein Textvergleich.
        var heute = DateTime.Today.ToString("yyyy-MM-dd");

        using var context = new AppDbContext();

        IQueryable<Dokument> abfrage = context.Dokumente.Include(d => d.Fach);

        if (!string.IsNullOrWhiteSpace(suchtext))
        {
            // LIKE ignoriert in SQLite die Gross-/Kleinschreibung nur für A-Z. Damit auch
            // Umlaute gefunden werden ("PRÜF" findet "Prüfung"), wird zusätzlich mit der
            // klein- und der grossgeschriebenen Variante des Suchtexts verglichen.
            var muster = $"%{suchtext.Trim()}%";
            var musterKlein = muster.ToLower();
            var musterGross = muster.ToUpper();
            abfrage = abfrage.Where(d => EF.Functions.Like(d.Titel, muster)
                                         || EF.Functions.Like(d.Titel, musterKlein)
                                         || EF.Functions.Like(d.Titel, musterGross));
        }

        abfrage = filter switch
        {
            DokumentFilter.Offen => abfrage.Where(d => !d.Abgegeben),
            DokumentFilter.Abgegeben => abfrage.Where(d => d.Abgegeben),
            DokumentFilter.Ueberfaellig => abfrage.Where(d => !d.Abgegeben
                                                              && d.Frist != null && d.Frist != ""
                                                              && string.Compare(d.Frist, heute) < 0),
            _ => abfrage
        };

        abfrage = sortierung switch
        {
            DokumentSortierung.Titel => abfrage.OrderBy(d => d.Titel),
            DokumentSortierung.Fach => abfrage.OrderBy(d => d.Fach!.Name).ThenBy(d => d.Titel),
            _ => abfrage
                .OrderBy(d => d.Frist == null || d.Frist == "")   // Dokumente ohne Frist ans Ende
                .ThenBy(d => d.Frist)
                .ThenBy(d => d.Titel)
        };

        return abfrage.ToList();
    }

    // ---------- CREATE / UPDATE ----------

    public override void Add(Dokument entity)
    {
        Bereinige(entity);
        base.Add(entity);
    }

    public override void Update(Dokument entity)
    {
        Bereinige(entity);
        base.Update(entity);
    }

    /// <summary>
    /// Kern des Workflows: setzt Abgegeben in der Datenbank.
    /// Ist Abgegeben = true, fällt das Dokument automatisch aus der Fristenabfrage heraus.
    /// </summary>
    public void SetAbgegeben(int id, bool abgegeben)
    {
        using var context = new AppDbContext();
        var dokument = context.Dokumente.Find(id);
        if (dokument == null) return;

        dokument.Abgegeben = abgegeben;
        context.SaveChanges();
    }

    // ---------- Hilfsmethode ----------

    /// <summary>
    /// Einheitliche Schreibweise vor dem Speichern: Texte ohne Rand-Leerzeichen,
    /// leerer Dateipfad und leere Frist werden als NULL gespeichert.
    /// </summary>
    private static void Bereinige(Dokument dokument)
    {
        dokument.Titel = dokument.Titel.Trim();
        dokument.Typ = dokument.Typ?.Trim() ?? string.Empty;
        dokument.Dateipfad = string.IsNullOrWhiteSpace(dokument.Dateipfad) ? null : dokument.Dateipfad;
        dokument.Frist = string.IsNullOrWhiteSpace(dokument.Frist) ? null : dokument.Frist;
    }
}
