namespace SchulApp.Models;

/// <summary>
/// Entität 1 der 1:N-Beziehung. Enthält nur, was in der Tabelle Fach steht,
/// plus die Navigation Property. Alles für die Anzeige liegt im FachDto.
/// </summary>
public class Fach
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Lehrperson { get; set; } = string.Empty;
    public string Erstellt { get; set; } = string.Empty;

    /// <summary>Navigation Property der 1:N-Beziehung. Nur gefüllt, wenn mit Include() geladen.</summary>
    public ICollection<Dokument> Dokumente { get; set; } = new List<Dokument>();
}
