using SchulApp.DataAccess;
using SchulApp.Models;

namespace SchulApp.Tests.Fakes;

/// <summary>
/// Speicherinterne Variante von IFachRepository für Unit Tests: eine Liste statt
/// einer Datenbank. Möglich ist das nur, weil die Services seit K3 gegen das
/// Interface arbeiten und nicht gegen die konkrete FachRepository-Klasse.
/// </summary>
public class FakeFachRepository : IFachRepository
{
    private readonly List<Fach> _faecher = new();
    private int _naechsteId = 1;

    public List<Fach> GetAll()
    {
        return _faecher.OrderBy(fach => fach.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>Im Fake hängen die Dokumente direkt am Fach-Objekt - kein Nachladen nötig.</summary>
    public List<Fach> GetAlleMitDokumenten()
    {
        return GetAll();
    }

    public Fach? GetById(int id)
    {
        return _faecher.FirstOrDefault(fach => fach.Id == id);
    }

    /// <summary>Wie die Datenbank (Collation NOCASE): Gross-/Kleinschreibung wird ignoriert.</summary>
    public bool ExistiertName(string name, int ausserId = 0)
    {
        return _faecher.Any(fach => fach.Id != ausserId
                                    && string.Equals(fach.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public void Add(Fach entity)
    {
        entity.Id = _naechsteId++;
        _faecher.Add(entity);
    }

    public void Update(Fach entity)
    {
        int position = _faecher.FindIndex(fach => fach.Id == entity.Id);
        if (position >= 0) _faecher[position] = entity;
    }

    public void Delete(int id)
    {
        _faecher.RemoveAll(fach => fach.Id == id);
    }
}
