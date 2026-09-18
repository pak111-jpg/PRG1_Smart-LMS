using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace SchulApp.Models;

/// <summary>
/// Entität 2 (N-Seite). Frist wird als TEXT im Format "yyyy-MM-dd" gespeichert,
/// dadurch funktioniert die Sortierung direkt per SQL (ORDER BY Frist ASC).
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

    /// <summary>Name des Fachs – nur bei der Fristenübersicht per JOIN gefüllt.</summary>
    [NotMapped]
    public string FachName { get; set; } = string.Empty;

    // ---------- Nur-Lesen-Eigenschaften für die Anzeige ----------

    public DateTime? FristDatum =>
        DateTime.TryParse(Frist, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d.Date
            : null;

    public string FristAnzeige =>
        FristDatum.HasValue ? FristDatum.Value.ToString("dd.MM.yyyy") : "–";

    public bool HatFrist => FristDatum.HasValue;

    /// <summary>Frist liegt in der Vergangenheit und das Dokument ist noch offen.</summary>
    public bool IstUeberfaellig =>
        !Abgegeben && FristDatum.HasValue && FristDatum.Value < DateTime.Today;

    public string StatusZeichen => Abgegeben ? "✓" : "○";

    public string StatusText => Abgegeben ? "Abgegeben" : "Offen";

    public string DateiAnzeige =>
        string.IsNullOrWhiteSpace(Dateipfad) ? "–" : System.IO.Path.GetFileName(Dateipfad);

    /// <summary>Restzeit im Klartext, z. B. "in 3 Tagen" oder "2 Tage überfällig".</summary>
    public string RestText
    {
        get
        {
            if (!FristDatum.HasValue) return string.Empty;
            var tage = (FristDatum.Value - DateTime.Today).Days;
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

    public override string ToString() => Titel;
}
