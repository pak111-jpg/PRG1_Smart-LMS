using SchulApp.DataAccess;
using SchulApp.Models;

namespace SchulApp.Tests.Fakes;

/// <summary>
/// Speicherinterne Variante von IDokumentRepository für Unit Tests: eine Liste
/// statt einer Datenbank. Filter und Sortierung sind so einfach wie möglich
/// nachgebaut - getestet werden die Services, nicht dieser Fake.
/// </summary>
public class FakeDokumentRepository : IDokumentRepository
{
    private readonly List<Dokument> _dokumente = new();
    private int _naechsteId = 1;

    public List<Dokument> GetAll()
    {
        return _dokumente.ToList();
    }

    public Dokument? GetById(int id)
    {
        return _dokumente.FirstOrDefault(dokument => dokument.Id == id);
    }

    public List<Dokument> GetByFach(int fachId)
    {
        return _dokumente
            .Where(dokument => dokument.FachId == fachId)
            .OrderBy(dokument => dokument.Abgegeben)
            .ThenBy(dokument => dokument.Frist == null)
            .ThenBy(dokument => dokument.Frist)
            .ThenBy(dokument => dokument.Titel)
            .ToList();
    }

    public List<Dokument> GetOffeneFristen()
    {
        return _dokumente
            .Where(dokument => !dokument.Abgegeben && dokument.HatFrist)
            .OrderBy(dokument => dokument.Frist)
            .ToList();
    }

    public List<Dokument> Suche(string suchtext, DokumentFilter filter, DokumentSortierung sortierung)
    {
        IEnumerable<Dokument> treffer = _dokumente
            .Where(dokument => dokument.Titel.Contains(suchtext, StringComparison.OrdinalIgnoreCase));

        treffer = filter switch
        {
            DokumentFilter.Offen => treffer.Where(dokument => !dokument.Abgegeben),
            DokumentFilter.Abgegeben => treffer.Where(dokument => dokument.Abgegeben),
            DokumentFilter.Ueberfaellig => treffer.Where(dokument => dokument.IstUeberfaelligAm(DateTime.Today)),
            _ => treffer
        };

        return treffer.OrderBy(dokument => dokument.Titel).ToList();
    }

    public void Add(Dokument entity)
    {
        entity.Id = _naechsteId++;
        _dokumente.Add(entity);
    }

    public void Update(Dokument entity)
    {
        int position = _dokumente.FindIndex(dokument => dokument.Id == entity.Id);
        if (position >= 0) _dokumente[position] = entity;
    }

    public void Delete(int id)
    {
        _dokumente.RemoveAll(dokument => dokument.Id == id);
    }

    public void SetAbgegeben(int id, bool abgegeben)
    {
        var dokument = GetById(id);
        if (dokument == null) return;

        dokument.Abgegeben = abgegeben;
    }
}
