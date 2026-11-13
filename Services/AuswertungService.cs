using SchulApp.DataAccess;
using SchulApp.Models;

namespace SchulApp.Services;

/// <summary>
/// Zweite, eigenständige Serviceklasse (K1-Challenge): Auswertung des Abgabestands.
/// Berechnet pro Fach und über alle Fächer, wie viele Dokumente abgegeben, offen
/// und überfällig sind. Liest nur - und spricht die Datenbank ausschliesslich über
/// die DataAccess-Klassen an.
/// </summary>
public class AuswertungService
{
    private const string GesamtBezeichnung = "Alle Fächer";

    private readonly FachDataAccess _fachDataAccess = new();
    private readonly DokumentDataAccess _dokumentDataAccess = new();

    /// <summary>
    /// Eine Zeile pro Fach, alphabetisch. Fächer ohne Dokumente erscheinen mit 0,
    /// damit die Auswertung alle Fächer zeigt.
    /// </summary>
    public List<FachAuswertung> GetAuswertungProFach()
    {
        List<Fach> faecher = _fachDataAccess.GetAll();
        List<Dokument> dokumente = _dokumentDataAccess.GetAll();

        return faecher
            .Select(fach => Berechne(fach.Name, dokumente.Where(d => d.FachId == fach.Id)))
            .ToList();
    }

    /// <summary>Summe über alle Fächer - die Fusszeile der Auswertung.</summary>
    public FachAuswertung GetGesamtauswertung()
    {
        return Berechne(GesamtBezeichnung, _dokumentDataAccess.GetAll());
    }

    private static FachAuswertung Berechne(string bezeichnung, IEnumerable<Dokument> dokumente)
    {
        List<Dokument> liste = dokumente.ToList();

        return new FachAuswertung
        {
            FachName = bezeichnung,
            Gesamt = liste.Count,
            Abgegeben = liste.Count(d => d.Abgegeben),
            Ueberfaellig = liste.Count(d => d.IstUeberfaellig)
        };
    }
}
