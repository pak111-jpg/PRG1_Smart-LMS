using System.Globalization;

namespace SchulApp.Models;

/// <summary>
/// Entität 2 (N-Seite). Frist wird als TEXT im Format "yyyy-MM-dd" gespeichert,
/// dadurch funktioniert die Sortierung direkt in der Datenbank.
/// Texte für die Anzeige liegen im DokumentDto, nicht hier.
/// </summary>
public class Dokument
{
    public int Id { get; set; }
    public string Titel { get; set; } = string.Empty;
    public string Typ { get; set; } = string.Empty;
    public string? Dateipfad { get; set; }
    public string? Frist { get; set; }
    public bool Abgegeben { get; set; }
    public int FachId { get; set; }

    /// <summary>Navigation Property zur 1-Seite. Nur gefüllt, wenn mit Include() geladen.</summary>
    public Fach? Fach { get; set; }

    // ---------- Fachliche Regeln (keine Tabellenspalten) ----------

    /// <summary>Die Frist als Datum - null, wenn keine (gültige) Frist gesetzt ist.</summary>
    public DateTime? FristDatum =>
        DateTime.TryParse(Frist, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d.Date
            : null;

    public bool HatFrist => FristDatum.HasValue;

    /// <summary>Frist liegt in der Vergangenheit und das Dokument ist noch offen.</summary>
    public bool IstUeberfaellig =>
        !Abgegeben && FristDatum.HasValue && FristDatum.Value < DateTime.Today;
}
