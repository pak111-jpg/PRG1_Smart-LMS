using SchulApp.DataAccess;
using SchulApp.Dtos;
using SchulApp.Models;

namespace SchulApp.Services;

/// <summary>
/// Business-Logik rund um Dokumente: Ablage, Fristenübersicht und der Kernprozess
/// "als abgegeben markieren". Kennt vom Datenzugriff nur das Interface
/// IDokumentRepository; das Mapping auf DTOs übernimmt der DokumentMapper.
/// </summary>
public class DokumentService
{
    private readonly IDokumentRepository _repository;

    public DokumentService(IDokumentRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Die Typen, die beim Ablegen zur Auswahl stehen.</summary>
    public IReadOnlyList<string> GetDokumentTypen()
    {
        return DokumentTypen.Alle;
    }

    /// <summary>Alle Dokumente eines Fachs: offene zuerst, dann nach Frist, dann nach Titel.</summary>
    public List<DokumentDto> GetByFach(int fachId)
    {
        return ZuDtos(_repository.GetByFach(fachId));
    }

    /// <summary>Fristenübersicht: offene Dokumente mit Frist, aufsteigend nach Frist.</summary>
    public List<DokumentDto> GetOffeneFristen()
    {
        return ZuDtos(_repository.GetOffeneFristen());
    }

    /// <summary>
    /// Gesamtübersicht aller Dokumente über alle Fächer - gesucht, gefiltert und
    /// sortiert. Ein leerer Suchtext bedeutet "nicht nach Titel einschränken".
    /// </summary>
    public List<DokumentDto> Suche(string? suchtext, DokumentFilter filter, DokumentSortierung sortierung)
    {
        return ZuDtos(_repository.Suche(suchtext?.Trim() ?? string.Empty, filter, sortierung));
    }

    public int ZaehleOffene(List<DokumentDto> dokumente)
    {
        return dokumente.Count(dokument => !dokument.Abgegeben);
    }

    public int ZaehleUeberfaellige(List<DokumentDto> dokumente)
    {
        return dokumente.Count(dokument => dokument.IstUeberfaellig);
    }

    /// <summary>
    /// Legt ein neues, offenes Dokument in einem Fach ab.
    /// Regeln: Der Titel darf nicht leer sein; ohne Typ gilt der Standardtyp.
    /// </summary>
    /// <returns>Das gespeicherte Dokument.</returns>
    /// <exception cref="ValidierungsException">Wenn der Titel leer ist.</exception>
    public DokumentDto Hinzufuegen(string titel, string? typ, string? dateipfad, DateTime? frist, int fachId)
    {
        titel = titel.Trim();

        if (titel.Length == 0)
            throw new ValidierungsException("Gib einen Titel für das Dokument ein.");

        var dokument = new Dokument
        {
            Titel = titel,
            Typ = string.IsNullOrWhiteSpace(typ) ? DokumentTypen.Standard : typ.Trim(),
            Dateipfad = dateipfad?.Trim(),
            Frist = frist?.ToString(Dokument.FristFormat),
            Abgegeben = false,
            FachId = fachId
        };

        _repository.Add(dokument);
        return DokumentMapper.ZuDto(dokument, DateTime.Today);
    }

    public void Loeschen(int dokumentId)
    {
        _repository.Delete(dokumentId);
    }

    /// <summary>Kernprozess: schaltet den Abgabestatus eines Dokuments um.</summary>
    /// <returns>Der neue Status: true = abgegeben.</returns>
    public bool AbgabeUmschalten(DokumentDto dokument)
    {
        bool istNeuAbgegeben = !dokument.Abgegeben;
        _repository.SetAbgegeben(dokument.Id, istNeuAbgegeben);
        return istNeuAbgegeben;
    }

    /// <summary>Markiert ein Dokument als abgegeben - es fällt damit aus der Fristenübersicht.</summary>
    public void MarkiereAlsAbgegeben(int dokumentId)
    {
        _repository.SetAbgegeben(dokumentId, true);
    }

    private static List<DokumentDto> ZuDtos(List<Dokument> dokumente)
    {
        DateTime heute = DateTime.Today;

        return dokumente
            .Select(dokument => DokumentMapper.ZuDto(dokument, heute))
            .ToList();
    }
}
