using SchulApp.Dtos;
using SchulApp.Models;

namespace SchulApp.Services;

/// <summary>
/// Bildet ein Dokument auf ein DokumentDto ab und erzeugt dabei alle Anzeigetexte.
/// Eigene Klasse, damit der DokumentService nur Regeln und Abläufe enthält
/// (Single Responsibility). Der Stichtag "heute" wird übergeben - so ist das
/// Mapping ohne Systemuhr testbar.
/// </summary>
public static class DokumentMapper
{
    public const string FristAnzeigeFormat = "dd.MM.yyyy";
    public const string ZeichenAbgegeben = "✓";
    public const string ZeichenOffen = "○";
    public const string TextAbgegeben = "Abgegeben";
    public const string TextOffen = "Offen";

    public static DokumentDto ZuDto(Dokument dokument, DateTime heute)
    {
        return new DokumentDto
        {
            Id = dokument.Id,
            Titel = dokument.Titel,
            Typ = dokument.Typ,
            FachName = dokument.Fach?.Name ?? string.Empty,
            DateiAnzeige = BerechneDateiAnzeige(dokument.Dateipfad),
            HatFrist = dokument.HatFrist,
            FristAnzeige = dokument.FristDatum?.ToString(FristAnzeigeFormat) ?? Anzeigetexte.KeinWert,
            RestText = BerechneRestText(dokument.FristDatum, heute),
            Abgegeben = dokument.Abgegeben,
            IstUeberfaellig = dokument.IstUeberfaelligAm(heute),
            StatusZeichen = dokument.Abgegeben ? ZeichenAbgegeben : ZeichenOffen,
            StatusText = dokument.Abgegeben ? TextAbgegeben : TextOffen
        };
    }

    /// <summary>
    /// Restzeit bis zur Frist im Klartext, z. B. "in 3 Tagen" oder "2 Tage überfällig".
    /// Ohne Frist ein leerer Text.
    /// </summary>
    public static string BerechneRestText(DateTime? frist, DateTime heute)
    {
        if (!frist.HasValue) return string.Empty;

        int tageBisFrist = (frist.Value.Date - heute.Date).Days;
        return tageBisFrist switch
        {
            < -1 => $"{-tageBisFrist} Tage überfällig",
            -1 => "1 Tag überfällig",
            0 => "heute fällig",
            1 => "morgen fällig",
            _ => $"in {tageBisFrist} Tagen"
        };
    }

    /// <summary>Dateiname ohne Pfad - oder der Platzhalter, wenn keine Datei verknüpft ist.</summary>
    private static string BerechneDateiAnzeige(string? dateipfad)
    {
        return string.IsNullOrWhiteSpace(dateipfad)
            ? Anzeigetexte.KeinWert
            : System.IO.Path.GetFileName(dateipfad);
    }
}
