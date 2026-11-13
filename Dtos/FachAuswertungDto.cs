namespace SchulApp.Dtos;

/// <summary>
/// Eine Zeile der Abgabe-Auswertung: Kennzahlen zu den Dokumenten eines Fachs
/// (oder aller Fächer zusammen). Nur Daten - berechnet wird im AuswertungService.
/// </summary>
public class FachAuswertungDto
{
    public string FachName { get; set; } = string.Empty;
    public int Gesamt { get; set; }
    public int Abgegeben { get; set; }
    public int Offen { get; set; }
    public int Ueberfaellig { get; set; }

    /// <summary>Anteil abgegebener Dokumente in Prozent (0 bis 100), ohne Dokumente 0.</summary>
    public int ProzentAbgegeben { get; set; }

    /// <summary>Z. B. "40 %" - oder "–", wenn es keine Dokumente gibt.</summary>
    public string ProzentAnzeige { get; set; } = string.Empty;
}
