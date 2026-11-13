namespace SchulApp.Models;

/// <summary>
/// Die Dokumenttypen, die in der Eingabemaske zur Auswahl stehen. Zentral
/// definiert, damit die Bezeichnungen nicht als lose Texte im Code verstreut sind.
/// </summary>
public static class DokumentTypen
{
    public const string Auftrag = "Auftrag";
    public const string Pruefung = "Prüfung";
    public const string Notizen = "Notizen";
    public const string Skript = "Skript";
    public const string Projekt = "Projekt";

    /// <summary>Gilt, wenn beim Ablegen kein Typ angegeben wird.</summary>
    public const string Standard = Auftrag;

    /// <summary>Alle Typen in der Reihenfolge der Auswahlliste.</summary>
    public static IReadOnlyList<string> Alle { get; } = new[] { Auftrag, Pruefung, Notizen, Skript, Projekt };
}
