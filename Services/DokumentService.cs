using SchulApp.DataAccess;
using SchulApp.Models;

namespace SchulApp.Services;

/// <summary>
/// Business-Logik rund um Dokumente: Ablage, Fristenübersicht und der
/// Kernprozess "als abgegeben markieren".
/// Spricht die Datenbank ausschliesslich über DokumentDataAccess an - kein SQL hier.
/// </summary>
public class DokumentService
{
    private const string StandardTyp = "Auftrag";

    /// <summary>Speicherformat der Frist - dadurch sortiert die Datenbank korrekt.</summary>
    private const string FristFormat = "yyyy-MM-dd";

    private readonly DokumentDataAccess _dataAccess = new();

    // ---------- Lesen ----------

    /// <summary>Alle Dokumente eines Fachs: offene zuerst, dann nach Frist, dann nach Titel.</summary>
    public List<Dokument> GetByFach(int fachId)
    {
        return _dataAccess.GetByFach(fachId);
    }

    /// <summary>Fristenübersicht: offene Dokumente mit Frist, aufsteigend nach Frist.</summary>
    public List<Dokument> GetOffeneFristen()
    {
        return _dataAccess.GetOffeneFristen();
    }

    public int ZaehleOffene(List<Dokument> dokumente)
    {
        return dokumente.Count(d => !d.Abgegeben);
    }

    public int ZaehleUeberfaellige(List<Dokument> dokumente)
    {
        return dokumente.Count(d => d.IstUeberfaellig);
    }

    // ---------- Ablage ----------

    /// <summary>
    /// Legt ein neues, offenes Dokument in einem Fach ab und liefert es zurück.
    /// Regeln: Der Titel darf nicht leer sein; ohne Typ gilt "Auftrag".
    /// </summary>
    public Dokument Hinzufuegen(string titel, string? typ, string? dateipfad, DateTime? frist, int fachId)
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

        _dataAccess.Add(dokument);
        return dokument;
    }

    public void Loeschen(int dokumentId)
    {
        _dataAccess.Delete(dokumentId);
    }

    // ---------- Kernprozess: Abgabe ----------

    /// <summary>
    /// Schaltet den Abgabestatus eines Dokuments um und liefert den neuen Status zurück.
    /// </summary>
    public bool AbgabeUmschalten(Dokument dokument)
    {
        bool neuerStatus = !dokument.Abgegeben;
        _dataAccess.SetAbgegeben(dokument.Id, neuerStatus);
        return neuerStatus;
    }

    /// <summary>Markiert ein Dokument als abgegeben - es fällt damit aus der Fristenübersicht.</summary>
    public void MarkiereAlsAbgegeben(int dokumentId)
    {
        _dataAccess.SetAbgegeben(dokumentId, true);
    }
}
