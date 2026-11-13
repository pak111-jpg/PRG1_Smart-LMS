using SchulApp.DataAccess;
using SchulApp.Dtos;
using SchulApp.Models;

namespace SchulApp.Services;

/// <summary>
/// Auswertung des Abgabestands: berechnet pro Fach und über alle Fächer, wie
/// viele Dokumente abgegeben, offen und überfällig sind. Liest nur - und kennt
/// vom Datenzugriff nur die Interfaces IFachRepository und IDokumentRepository.
/// </summary>
public class AuswertungService
{
    public const string GesamtBezeichnung = "Alle Fächer";

    private const double HundertProzent = 100.0;

    private readonly IFachRepository _fachRepository;
    private readonly IDokumentRepository _dokumentRepository;

    public AuswertungService(IFachRepository fachRepository, IDokumentRepository dokumentRepository)
    {
        _fachRepository = fachRepository;
        _dokumentRepository = dokumentRepository;
    }

    /// <summary>
    /// Eine Zeile pro Fach, alphabetisch. Fächer ohne Dokumente erscheinen mit 0,
    /// damit die Auswertung alle Fächer zeigt.
    /// </summary>
    public List<FachAuswertungDto> GetAuswertungProFach()
    {
        return _fachRepository.GetAlleMitDokumenten()
            .Select(fach => Berechne(fach.Name, fach.Dokumente))
            .ToList();
    }

    /// <summary>Summe über alle Fächer - die Fusszeile der Auswertung.</summary>
    public FachAuswertungDto GetGesamtauswertung()
    {
        return Berechne(GesamtBezeichnung, _dokumentRepository.GetAll());
    }

    private static FachAuswertungDto Berechne(string bezeichnung, IEnumerable<Dokument> dokumente)
    {
        var liste = dokumente.ToList();
        DateTime heute = DateTime.Today;

        int gesamt = liste.Count;
        int abgegeben = liste.Count(dokument => dokument.Abgegeben);
        int prozentAbgegeben = gesamt == 0 ? 0 : (int)Math.Round(HundertProzent * abgegeben / gesamt);

        return new FachAuswertungDto
        {
            FachName = bezeichnung,
            Gesamt = gesamt,
            Abgegeben = abgegeben,
            Offen = gesamt - abgegeben,
            Ueberfaellig = liste.Count(dokument => dokument.IstUeberfaelligAm(heute)),
            ProzentAbgegeben = prozentAbgegeben,
            ProzentAnzeige = gesamt == 0 ? Anzeigetexte.KeinWert : $"{prozentAbgegeben} %"
        };
    }
}
