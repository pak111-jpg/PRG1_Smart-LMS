using SchulApp.DataAccess;
using SchulApp.Dtos;
using SchulApp.Models;

namespace SchulApp.Services;

/// <summary>
/// Business-Logik rund um Fächer: Regeln und Ablaufsteuerung.
/// Kennt vom Datenzugriff nur das Interface IFachRepository; das Mapping auf
/// DTOs übernimmt der FachMapper.
/// </summary>
public class FachService
{
    private readonly IFachRepository _repository;

    public FachService(IFachRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Alle Fächer alphabetisch, als DTOs für die Anzeige.</summary>
    public List<FachDto> GetAlle()
    {
        return _repository.GetAlleMitDokumenten()
            .Select(FachMapper.ZuDto)
            .ToList();
    }

    /// <summary>
    /// Legt ein neues Fach an.
    /// Regeln: Der Name darf nicht leer sein und muss eindeutig sein.
    /// </summary>
    /// <returns>Die Id des neuen Fachs.</returns>
    /// <exception cref="ValidierungsException">Wenn eine der Regeln verletzt ist.</exception>
    public int Hinzufuegen(string name, string lehrperson)
    {
        name = name.Trim();

        if (name.Length == 0)
            throw new ValidierungsException("Gib zuerst einen Fachnamen ein.");

        if (_repository.ExistiertName(name))
            throw new ValidierungsException($"Das Fach \"{name}\" gibt es bereits.");

        var fach = new Fach
        {
            Name = name,
            Lehrperson = lehrperson.Trim(),
            Erstellt = DateTime.Now.ToString(Fach.ErstelltFormat)
        };

        _repository.Add(fach);
        return fach.Id;
    }

    /// <summary>Löscht ein Fach samt seinen Dokumenten.</summary>
    public void Loeschen(int fachId)
    {
        _repository.Delete(fachId);
    }
}
