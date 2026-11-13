using SchulApp.DataAccess;
using SchulApp.Models;

namespace SchulApp.Services;

/// <summary>
/// Ablauf beim Programmstart: Datenbank vorbereiten und beim allerersten Start
/// Beispieldaten anlegen. Die UI ruft nur Initialisiere() auf.
/// </summary>
public class StartService
{
    private readonly IFachRepository _fachRepository;
    private readonly IDokumentRepository _dokumentRepository;

    public StartService(IFachRepository fachRepository, IDokumentRepository dokumentRepository)
    {
        _fachRepository = fachRepository;
        _dokumentRepository = dokumentRepository;
    }

    public void Initialisiere()
    {
        DbInitializer.Initialize();
        LegeBeispieldatenAn();
    }

    /// <summary>
    /// Legt Beispieldaten an, solange es noch kein Fach gibt - damit die Oberfläche
    /// beim allerersten Start nicht leer ist.
    /// </summary>
    public void LegeBeispieldatenAn()
    {
        if (_fachRepository.GetAll().Count > 0) return;

        DateTime heute = DateTime.Today;

        int programmierenId = LegeFachAn("PRG I", "M. Keller");
        int sicherheitId = LegeFachAn("ISM", "S. Brunner");
        int datenstrukturenId = LegeFachAn("LDS II", "A. Marti");

        LegeDokumentAn("Übung 4 – Schleifen", DokumentTypen.Auftrag, heute.AddDays(-2), programmierenId);
        LegeDokumentAn("Projektdokumentation", DokumentTypen.Projekt, heute.AddDays(4), programmierenId);
        LegeDokumentAn("Zusammenfassung Kapitel 1–3", DokumentTypen.Notizen, null, programmierenId);
        LegeDokumentAn("Fallstudie Datenschutz", DokumentTypen.Auftrag, heute.AddDays(1), sicherheitId);
        LegeDokumentAn("Vorbereitung Prüfung", DokumentTypen.Pruefung, heute.AddDays(11), datenstrukturenId);
    }

    private int LegeFachAn(string name, string lehrperson)
    {
        var fach = new Fach { Name = name, Lehrperson = lehrperson };
        _fachRepository.Add(fach);
        return fach.Id;
    }

    private void LegeDokumentAn(string titel, string typ, DateTime? frist, int fachId)
    {
        _dokumentRepository.Add(new Dokument
        {
            Titel = titel,
            Typ = typ,
            Frist = frist?.ToString(Dokument.FristFormat),
            FachId = fachId
        });
    }
}
