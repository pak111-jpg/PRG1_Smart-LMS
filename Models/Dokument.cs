using System.Globalization;

namespace SchulApp.Models;

/// <summary>
/// Entität 2 (N-Seite). Texte für die Anzeige liegen im DokumentDto, nicht hier.
/// </summary>
public class Dokument
{
    /// <summary>
    /// Speicherformat der Frist. Als Text in diesem Format sortiert die Datenbank
    /// gleich wie nach Datum.
    /// </summary>
    public const string FristFormat = "yyyy-MM-dd";

    public int Id { get; set; }
    public string Titel { get; set; } = string.Empty;
    public string Typ { get; set; } = string.Empty;
    public string? Dateipfad { get; set; }
    public string? Frist { get; set; }
    public bool Abgegeben { get; set; }
    public int FachId { get; set; }

    /// <summary>Navigation Property zur 1-Seite. Nur gefüllt, wenn mit Include() geladen.</summary>
    public Fach? Fach { get; set; }

    /// <summary>Die Frist als Datum - null, wenn keine (gültige) Frist gesetzt ist.</summary>
    public DateTime? FristDatum =>
        DateTime.TryParse(Frist, CultureInfo.InvariantCulture, DateTimeStyles.None, out var datum)
            ? datum.Date
            : null;

    public bool HatFrist => FristDatum.HasValue;

    /// <summary>
    /// Überfällig heisst: noch offen und die Frist liegt vor dem Stichtag.
    /// Der Stichtag wird übergeben, damit die Regel ohne Systemuhr testbar ist.
    /// </summary>
    public bool IstUeberfaelligAm(DateTime stichtag)
    {
        return !Abgegeben && FristDatum.HasValue && FristDatum.Value < stichtag.Date;
    }
}
