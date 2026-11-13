namespace SchulApp.Dtos;

/// <summary>
/// Was die UI von einem Dokument braucht - inklusive Fachname, damit die UI nicht
/// selbst über die Navigation Property greifen muss. Nur Daten, keine Logik:
/// gefüllt wird das DTO im DokumentService.
/// Bewusst nicht enthalten: FachId (Fremdschlüssel, braucht die UI nicht), der
/// volle Dateipfad (angezeigt wird nur der Dateiname) und das Fach-Objekt selbst.
/// </summary>
public class DokumentDto
{
    public int Id { get; set; }
    public string Titel { get; set; } = string.Empty;
    public string Typ { get; set; } = string.Empty;

    /// <summary>Name des Fachs - nur gefüllt, wenn das Fach mitgeladen wurde.</summary>
    public string FachName { get; set; } = string.Empty;

    /// <summary>Dateiname ohne Pfad oder "–".</summary>
    public string DateiAnzeige { get; set; } = string.Empty;

    public bool HatFrist { get; set; }

    /// <summary>Frist als "dd.MM.yyyy" oder "–".</summary>
    public string FristAnzeige { get; set; } = string.Empty;

    /// <summary>Restzeit im Klartext, z. B. "in 3 Tagen" oder "2 Tage überfällig".</summary>
    public string RestText { get; set; } = string.Empty;

    public bool Abgegeben { get; set; }
    public bool IstUeberfaellig { get; set; }

    /// <summary>"✓" für abgegeben, "○" für offen.</summary>
    public string StatusZeichen { get; set; } = string.Empty;

    /// <summary>"Abgegeben" oder "Offen".</summary>
    public string StatusText { get; set; } = string.Empty;
}
