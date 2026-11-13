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
    public List<Dokument> GetByFach(int fachId)
    {
        using var context = new AppDbContext();

        var offeneZuerst = context.Dokumente
            .Where(dokument => dokument.FachId == fachId)
            .OrderBy(dokument => dokument.Abgegeben);

        return SortiereNachFrist(offeneZuerst).ToList();
    }

    /// <summary>
    /// Fristenübersicht. Include() lädt das zugehörige Fach in derselben Abfrage mit
    /// (JOIN über die 1:N-Beziehung) - dadurch ist Dokument.Fach gefüllt.
    /// </summary>
    public List<Dokument> GetOffeneFristen()
    {
        using var context = new AppDbContext();
        return context.Dokumente
            .Include(dokument => dokument.Fach)
            .Where(dokument => !dokument.Abgegeben && dokument.Frist != null && dokument.Frist != "")
            .OrderBy(dokument => dokument.Frist)
            .ToList();
    }

    /// <summary>
    /// Gesamtübersicht über alle Fächer. Die Abfrage wird Schritt für Schritt
    /// zusammengesetzt und erst mit ToList() als ein einziges SQL-Statement ausgeführt.
    /// </summary>
    public List<Dokument> Suche(string suchtext, DokumentFilter filter, DokumentSortierung sortierung)
    {
        using var context = new AppDbContext();

        IQueryable<Dokument> abfrage = context.Dokumente.Include(dokument => dokument.Fach);
        abfrage = FiltereNachTitel(abfrage, suchtext);
        abfrage = FiltereNachStatus(abfrage, filter);
        abfrage = Sortiere(abfrage, sortierung);

        return abfrage.ToList();
    }

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

    private static IQueryable<Dokument> FiltereNachTitel(IQueryable<Dokument> abfrage, string suchtext)
    {
        if (string.IsNullOrWhiteSpace(suchtext)) return abfrage;

        // LIKE ignoriert in SQLite die Gross-/Kleinschreibung nur für A-Z. Damit auch
        // Umlaute gefunden werden ("PRÜF" findet "Prüfung"), wird zusätzlich mit der
        // klein- und der grossgeschriebenen Variante des Suchtexts verglichen.
        var muster = $"%{suchtext.Trim()}%";
        var musterKlein = muster.ToLower();
        var musterGross = muster.ToUpper();

        return abfrage.Where(dokument => EF.Functions.Like(dokument.Titel, muster)
                                         || EF.Functions.Like(dokument.Titel, musterKlein)
                                         || EF.Functions.Like(dokument.Titel, musterGross));
    }

    private static IQueryable<Dokument> FiltereNachStatus(IQueryable<Dokument> abfrage, DokumentFilter filter)
    {
        return filter switch
        {
            DokumentFilter.Offen => abfrage.Where(dokument => !dokument.Abgegeben),
            DokumentFilter.Abgegeben => abfrage.Where(dokument => dokument.Abgegeben),
            DokumentFilter.Ueberfaellig => FiltereUeberfaellige(abfrage),
            _ => abfrage
        };
    }

    private static IQueryable<Dokument> FiltereUeberfaellige(IQueryable<Dokument> abfrage)
    {
        // Das Speicherformat der Frist sortiert als Text gleich wie als Datum,
        // deshalb reicht in der Datenbank ein Textvergleich.
        var heute = DateTime.Today.ToString(Dokument.FristFormat);

        return abfrage.Where(dokument => !dokument.Abgegeben
                                         && dokument.Frist != null && dokument.Frist != ""
                                         && string.Compare(dokument.Frist, heute) < 0);
    }

    private static IQueryable<Dokument> Sortiere(IQueryable<Dokument> abfrage, DokumentSortierung sortierung)
    {
        return sortierung switch
        {
            DokumentSortierung.Titel => abfrage.OrderBy(dokument => dokument.Titel),
            DokumentSortierung.Fach => abfrage
                .OrderBy(dokument => dokument.Fach!.Name)
                .ThenBy(dokument => dokument.Titel),
            _ => SortiereNachFrist(abfrage.OrderBy(dokument => dokument.Frist == null || dokument.Frist == ""))
        };
    }

    /// <summary>
    /// Hängt an eine bestehende Sortierung an: Dokumente ohne Frist ans Ende, sonst
    /// früheste Frist zuerst, bei gleicher Frist nach Titel.
    /// </summary>
    private static IOrderedQueryable<Dokument> SortiereNachFrist(IOrderedQueryable<Dokument> vorsortiert)
    {
        return vorsortiert
            .ThenBy(dokument => dokument.Frist == null || dokument.Frist == "")
            .ThenBy(dokument => dokument.Frist)
            .ThenBy(dokument => dokument.Titel);
    }

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
