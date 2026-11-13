namespace SchulApp.Dtos;

/// <summary>
/// Was die UI von einem Fach braucht: Stammdaten, fertige Anzeigetexte und zwei
/// Kennzahlen, die beim Mapping aus den Dokumenten des Fachs berechnet werden.
/// Nur Daten, keine Logik - gefüllt wird das DTO im FachService.
/// Bewusst nicht enthalten: Erstellt (wird nirgends angezeigt) und die Liste der
/// Dokumente (die UI lädt Dokumente separat über den DokumentService).
/// </summary>
public class FachDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Lehrperson { get; set; } = string.Empty;

    /// <summary>Initialen für den runden Avatar in der Fächerliste, z. B. "PR".</summary>
    public string Initialen { get; set; } = string.Empty;

    /// <summary>Lehrperson oder "Keine Lehrperson".</summary>
    public string LehrpersonAnzeige { get; set; } = string.Empty;

    public int AnzahlDokumente { get; set; }
    public int AnzahlOffen { get; set; }

    /// <summary>Z. B. "1 Dokument" oder "3 Dokumente".</summary>
    public string DokumenteAnzeige { get; set; } = string.Empty;

    /// <summary>Zusatz für die Fächerliste, z. B. " · 2 offen" - leer, wenn nichts offen ist.</summary>
    public string OffenAnzeige { get; set; } = string.Empty;
}
