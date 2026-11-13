using SchulApp.DataAccess;
using SchulApp.Dtos;
using SchulApp.Models;

namespace SchulApp.Services;

/// <summary>
/// Business-Logik rund um Fächer: Regeln, Ablaufsteuerung und das Mapping
/// Fach -> FachDto. Kennt vom Datenzugriff nur das Interface IFachRepository.
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
            .Select(ZuDto)
            .ToList();
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

        if (_repository.ExistiertName(name))
            throw new ValidierungsException($"Das Fach \"{name}\" gibt es bereits.");

        var fach = new Fach
        {
            Name = name,
            Lehrperson = lehrperson.Trim(),
            Erstellt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        _repository.Add(fach);
        return fach.Id;
    }

    /// <summary>Löscht ein Fach samt seinen Dokumenten.</summary>
    public void Loeschen(int fachId)
    {
        _repository.Delete(fachId);
    }

    // ---------- Mapping Entity -> DTO ----------

    /// <summary>
    /// Bildet ein Fach samt seinen Dokumenten auf ein FachDto ab. Die beiden
    /// Kennzahlen werden hier aus der Navigation Property berechnet.
    /// </summary>
    private static FachDto ZuDto(Fach fach)
    {
        int anzahlDokumente = fach.Dokumente.Count;
        int anzahlOffen = fach.Dokumente.Count(d => !d.Abgegeben);

        return new FachDto
        {
            Id = fach.Id,
            Name = fach.Name,
            Lehrperson = fach.Lehrperson,
            Initialen = BerechneInitialen(fach.Name),
            LehrpersonAnzeige = string.IsNullOrWhiteSpace(fach.Lehrperson) ? "Keine Lehrperson" : fach.Lehrperson,
            AnzahlDokumente = anzahlDokumente,
            AnzahlOffen = anzahlOffen,
            DokumenteAnzeige = anzahlDokumente == 1 ? "1 Dokument" : $"{anzahlDokumente} Dokumente",
            OffenAnzeige = anzahlOffen == 0 ? string.Empty : $" · {anzahlOffen} offen"
        };
    }

    /// <summary>Zwei Wörter: je der erste Buchstabe ("PRG I" -> "PI"). Ein Wort: die ersten zwei Buchstaben.</summary>
    private static string BerechneInitialen(string name)
    {
        var text = name.Trim();
        if (text.Length == 0) return "?";

        var teile = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (teile.Length >= 2)
            return $"{char.ToUpper(teile[0][0])}{char.ToUpper(teile[1][0])}";

        return text.Length >= 2 ? text.Substring(0, 2).ToUpperInvariant() : text.ToUpperInvariant();
    }
}
