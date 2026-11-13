namespace SchulApp.Services;

/// <summary>
/// Textbausteine, die an mehreren Stellen der App gleich lauten müssen.
/// </summary>
public static class Anzeigetexte
{
    /// <summary>Platzhalter für "kein Wert", z. B. bei einem Dokument ohne Frist.</summary>
    public const string KeinWert = "–";

    /// <summary>Anzahl mit korrekter Einzahl/Mehrzahl: "1 Dokument", "3 Dokumente".</summary>
    public static string AnzahlDokumente(int anzahl)
    {
        return anzahl == 1 ? "1 Dokument" : $"{anzahl} Dokumente";
    }
}
