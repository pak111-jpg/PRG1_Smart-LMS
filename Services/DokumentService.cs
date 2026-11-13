using SchulApp.DataAccess;
using SchulApp.Dtos;
using SchulApp.Models;

namespace SchulApp.Services;

/// <summary>
/// Business-Logik rund um Dokumente: Ablage, Fristenübersicht, der Kernprozess
/// "als abgegeben markieren" und das Mapping Dokument -> DokumentDto.
/// Kennt vom Datenzugriff nur das Interface IDokumentRepository.
/// </summary>
public class DokumentService
{
    private const string StandardTyp = "Auftrag";

    /// <summary>Speicherformat der Frist - dadurch sortiert die Datenbank korrekt.</summary>
    private const string FristFormat = "yyyy-MM-dd";

    private const string FristAnzeigeFormat = "dd.MM.yyyy";
    private const string KeinWert = "–";

    private readonly IDokumentRepository _repository;

    public DokumentService(IDokumentRepository repository)
    {
        _repository = repository;
    }

    // ---------- Lesen ----------

    /// <summary>Alle Dokumente eines Fachs: offene zuerst, dann nach Frist, dann nach Titel.</summary>
    public List<DokumentDto> GetByFach(int fachId)
    {
        return _repository.GetByFach(fachId)
            .Select(ZuDto)
            .ToList();
    }

    /// <summary>Fristenübersicht: offene Dokumente mit Frist, aufsteigend nach Frist.</summary>
    public List<DokumentDto> GetOffeneFristen()
    {
        return _repository.GetOffeneFristen()
            .Select(ZuDto)
            .ToList();
    }

    /// <summary>
    /// Gesamtübersicht aller Dokumente über alle Fächer - gesucht, gefiltert und
    /// sortiert. Ein leerer Suchtext bedeutet "nicht nach Titel einschränken".
    /// </summary>
    public List<DokumentDto> Suche(string? suchtext, DokumentFilter filter, DokumentSortierung sortierung)
    {
        return _repository.Suche(suchtext?.Trim() ?? string.Empty, filter, sortierung)
            .Select(ZuDto)
            .ToList();
    }

    public int ZaehleOffene(List<DokumentDto> dokumente)
    {
        return dokumente.Count(d => !d.Abgegeben);
    }

    public int ZaehleUeberfaellige(List<DokumentDto> dokumente)
    {
        return dokumente.Count(d => d.IstUeberfaellig);
    }

    // ---------- Ablage ----------

    /// <summary>
    /// Legt ein neues, offenes Dokument in einem Fach ab und liefert es zurück.
    /// Regeln: Der Titel darf nicht leer sein; ohne Typ gilt "Auftrag".
    /// </summary>
    public DokumentDto Hinzufuegen(string titel, string? typ, string? dateipfad, DateTime? frist, int fachId)
    {
        titel = titel.Trim();

        if (titel.Length == 0)
            throw new ValidierungsException("Gib einen Titel für das Dokument ein.");

        var dokument = new Dokument
        {
            Titel = titel,
            Typ = string.IsNullOrWhiteSpace(typ) ? StandardTyp : typ.Trim(),
            Dateipfad = dateipfad?.Trim(),
            Frist = frist?.ToString(FristFormat),
            Abgegeben = false,
            FachId = fachId
        };

        _repository.Add(dokument);
        return ZuDto(dokument);
    }

    public void Loeschen(int dokumentId)
    {
        _repository.Delete(dokumentId);
    }

    // ---------- Kernprozess: Abgabe ----------

    /// <summary>
    /// Schaltet den Abgabestatus eines Dokuments um und liefert den neuen Status zurück.
    /// </summary>
    public bool AbgabeUmschalten(DokumentDto dokument)
    {
        bool neuerStatus = !dokument.Abgegeben;
        _repository.SetAbgegeben(dokument.Id, neuerStatus);
        return neuerStatus;
    }

    /// <summary>Markiert ein Dokument als abgegeben - es fällt damit aus der Fristenübersicht.</summary>
    public void MarkiereAlsAbgegeben(int dokumentId)
    {
        _repository.SetAbgegeben(dokumentId, true);
    }

    // ---------- Mapping Entity -> DTO ----------

    private static DokumentDto ZuDto(Dokument dokument)
    {
        return new DokumentDto
        {
            Id = dokument.Id,
            Titel = dokument.Titel,
            Typ = dokument.Typ,
            FachName = dokument.Fach?.Name ?? string.Empty,
            DateiAnzeige = string.IsNullOrWhiteSpace(dokument.Dateipfad)
                ? KeinWert
                : System.IO.Path.GetFileName(dokument.Dateipfad),
            HatFrist = dokument.HatFrist,
            FristAnzeige = dokument.FristDatum?.ToString(FristAnzeigeFormat) ?? KeinWert,
            RestText = BerechneRestText(dokument.FristDatum),
            Abgegeben = dokument.Abgegeben,
            IstUeberfaellig = dokument.IstUeberfaellig,
            StatusZeichen = dokument.Abgegeben ? "✓" : "○",
            StatusText = dokument.Abgegeben ? "Abgegeben" : "Offen"
        };
    }

    /// <summary>Restzeit im Klartext, z. B. "in 3 Tagen" oder "2 Tage überfällig". Ohne Frist leer.</summary>
    private static string BerechneRestText(DateTime? frist)
    {
        if (!frist.HasValue) return string.Empty;

        int tage = (frist.Value - DateTime.Today).Days;
        return tage switch
        {
            < -1 => $"{-tage} Tage überfällig",
            -1 => "1 Tag überfällig",
            0 => "heute fällig",
            1 => "morgen fällig",
            _ => $"in {tage} Tagen"
        };
    }
}
