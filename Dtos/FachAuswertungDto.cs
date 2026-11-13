namespace SchulApp.Models;

/// <summary>
/// Eine Zeile der Abgabe-Auswertung: Kennzahlen zu den Dokumenten eines Fachs
/// (oder aller Fächer zusammen). Reines Datenobjekt, wird vom AuswertungService
/// berechnet und nicht in der Datenbank gespeichert.
/// </summary>
public class FachAuswertung
{
    public string FachName { get; set; } = string.Empty;
    public int Gesamt { get; set; }
    public int Abgegeben { get; set; }
    public int Ueberfaellig { get; set; }

    public int Offen => Gesamt - Abgegeben;

    /// <summary>Anteil abgegebener Dokumente in Prozent (0 bis 100), ohne Dokumente 0.</summary>
    public int ProzentAbgegeben =>
        Gesamt == 0 ? 0 : (int)Math.Round(100.0 * Abgegeben / Gesamt);

    public string ProzentAnzeige => Gesamt == 0 ? "–" : $"{ProzentAbgegeben} %";
}
