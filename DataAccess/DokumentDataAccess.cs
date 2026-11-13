using Microsoft.EntityFrameworkCore;
using SchulApp.Models;

namespace SchulApp.DataAccess;

/// <summary>
/// Sämtliche Datenbankzugriffe auf die Tabelle Dokument inkl. Fristenübersicht
/// und dem Workflow "als abgegeben markieren" - über AppDbContext und LINQ,
/// ohne SQL-Strings. Pro Methode ein eigener, kurzlebiger Context.
/// </summary>
public class DokumentDataAccess
{
    // ---------- READ ----------

    /// <summary>Alle Dokumente eines Fachs: offene zuerst, dann nach Frist, dann nach Titel.</summary>
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

    /// <summary>Alle Dokumente aller Fächer - Grundlage für die Abgabe-Auswertung.</summary>
    public List<Dokument> GetAll()
    {
        using var context = new AppDbContext();
        return context.Dokumente.ToList();
    }

    /// <summary>
    /// Gesamtübersicht über alle Fächer: optional nach Titel gesucht, nach Status
    /// gefiltert und sortiert. Die Abfrage wird Schritt für Schritt zusammengesetzt
    /// und erst mit ToList() als ein einziges SQL-Statement ausgeführt.
    /// </summary>
    public List<Dokument> Suche(string suchtext, DokumentFilter filter, DokumentSortierung sortierung)
    {
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

    public Dokument? GetById(int id)
    {
        using var context = new AppDbContext();
        return context.Dokumente.Find(id);
    }

    /// <summary>
    /// Fristenübersicht: alle offenen Dokumente mit gesetzter Frist, aufsteigend nach
    /// Frist sortiert. Include() lädt das zugehörige Fach in derselben Abfrage mit
    /// (JOIN über die 1:N-Beziehung) - dadurch ist Dokument.FachName gefüllt.
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

    /// <summary>Anzahl der überfälligen offenen Dokumente (Frist &lt; heute).</summary>
    public int ZaehleUeberfaellig()
    {
        // "yyyy-MM-dd" sortiert als Text gleich wie als Datum, deshalb reicht ein Textvergleich.
        var heute = DateTime.Today.ToString("yyyy-MM-dd");

        using var context = new AppDbContext();
        return context.Dokumente
            .Count(d => !d.Abgegeben
                        && d.Frist != null && d.Frist != ""
                        && string.Compare(d.Frist, heute) < 0);
    }

    // ---------- CREATE ----------

    public int Add(Dokument dokument)
    {
        Bereinige(dokument);

        using var context = new AppDbContext();
        context.Dokumente.Add(dokument);
        context.SaveChanges();   // EF Core trägt die neue Id direkt in dokument.Id ein

        return dokument.Id;
    }

    // ---------- UPDATE ----------

    public void Update(Dokument dokument)
    {
        Bereinige(dokument);

        using var context = new AppDbContext();
        var vorhanden = context.Dokumente.Find(dokument.Id);
        if (vorhanden == null) return;

        vorhanden.Titel = dokument.Titel;
        vorhanden.Typ = dokument.Typ;
        vorhanden.Dateipfad = dokument.Dateipfad;
        vorhanden.Frist = dokument.Frist;
        vorhanden.Abgegeben = dokument.Abgegeben;
        vorhanden.FachId = dokument.FachId;
        context.SaveChanges();
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

    // ---------- DELETE ----------

    public void Delete(int id)
    {
        using var context = new AppDbContext();
        var dokument = context.Dokumente.Find(id);
        if (dokument == null) return;

        context.Dokumente.Remove(dokument);
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
