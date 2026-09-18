namespace SchulApp.Services;

/// <summary>
/// Wird von der Service-Schicht geworfen, wenn eine Eingabe eine Regel verletzt
/// (z. B. leerer Name, doppeltes Fach). Die UI zeigt die Meldung als Hinweis an -
/// im Unterschied zu technischen Fehlern, die als Fehler gemeldet werden.
/// </summary>
public class ValidierungsException : Exception
{
    public ValidierungsException(string message) : base(message)
    {
    }
}
