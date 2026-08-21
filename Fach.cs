namespace SchulApp.Models;

/// <summary>
/// Entität 1 der 1:N-Beziehung. Reines Datenobjekt (kein ViewModel,
/// kein INotifyPropertyChanged) – die UI wird nach jeder Änderung neu geladen.
/// </summary>
public class Fach
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Lehrperson { get; set; } = string.Empty;
    public string Erstellt { get; set; } = string.Empty;

    /// <summary>Anzahl zugehöriger Dokumente – wird per JOIN mitgeladen (nur Anzeige).</summary>
    public int AnzahlDokumente { get; set; }

    /// <summary>Initialen für den runden Avatar in der Fächerliste, z. B. "PR".</summary>
    public string Initialen
    {
        get
        {
            var t = Name.Trim();
            if (t.Length == 0) return "?";
            var teile = t.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (teile.Length >= 2)
                return $"{char.ToUpper(teile[0][0])}{char.ToUpper(teile[1][0])}";
            return t.Length >= 2 ? t.Substring(0, 2).ToUpperInvariant() : t.ToUpperInvariant();
        }
    }

    public string LehrpersonAnzeige =>
        string.IsNullOrWhiteSpace(Lehrperson) ? "Keine Lehrperson" : Lehrperson;

    public string DokumenteAnzeige =>
        AnzahlDokumente == 1 ? "1 Dokument" : $"{AnzahlDokumente} Dokumente";

    public override string ToString() => Name;
}
