using SchulApp.DataAccess;
using SchulApp.Models;

namespace SchulApp.Services;

/// <summary>
/// Business-Logik rund um Fächer: Regeln und Ablaufsteuerung.
/// Spricht die Datenbank ausschliesslich über FachDataAccess an – kein SQL hier.
/// </summary>
public class FachService
{
    private readonly FachDataAccess _dataAccess = new();

    /// <summary>Alle Fächer inkl. Anzahl der zugehörigen Dokumente, alphabetisch.</summary>
    public List<Fach> GetAlle()
    {
        return _dataAccess.GetAll();
    }

    /// <summary>
    /// Legt ein neues Fach an und liefert dessen Id zurück.
    /// Regeln: Der Name darf nicht leer sein und muss eindeutig sein.
    /// </summary>
    public int Hinzufuegen(string name, string lehrperson)
    {
        name = name.Trim();

        if (name.Length == 0)
            throw new ValidierungsException("Gib zuerst einen Fachnamen ein.");

        if (_dataAccess.ExistiertName(name))
            throw new ValidierungsException($"Das Fach \"{name}\" gibt es bereits.");

        var fach = new Fach
        {
            Name = name,
            Lehrperson = lehrperson.Trim(),
            Erstellt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        return _dataAccess.Add(fach);
    }

    /// <summary>Löscht ein Fach samt seinen Dokumenten.</summary>
    public void Loeschen(int fachId)
    {
        _dataAccess.Delete(fachId);
    }
}
