using SchulApp.DataAccess;
using SchulApp.Models;

namespace SchulApp.Services;

/// <summary>
/// Ablauf beim Programmstart: Datenbank vorbereiten und beim allerersten Start
/// Beispieldaten anlegen. Die UI ruft nur Initialisiere() auf.
/// </summary>
public class StartService
{
    private const string FristFormat = "yyyy-MM-dd";

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
        BeispieldatenAnlegen();
    }

    /// <summary>
    /// Legt beim allerersten Start ein paar Beispieldaten an, damit die Oberfläche
    /// nicht leer startet. Diese Methode kann ersatzlos gelöscht werden.
    /// </summary>
    private void BeispieldatenAnlegen()
    {
        if (_fachRepository.GetAll().Count > 0) return;

        var heute = DateTime.Today;

        int prg = FachAnlegen("PRG I", "M. Keller");
        int ism = FachAnlegen("ISM", "S. Brunner");
        int lds = FachAnlegen("LDS II", "A. Marti");

        DokumentAnlegen("Übung 4 – Schleifen", "Auftrag", heute.AddDays(-2), prg);
        DokumentAnlegen("Projektdokumentation", "Projekt", heute.AddDays(4), prg);
        DokumentAnlegen("Zusammenfassung Kapitel 1–3", "Notizen", null, prg);
        DokumentAnlegen("Fallstudie Datenschutz", "Auftrag", heute.AddDays(1), ism);
        DokumentAnlegen("Vorbereitung Prüfung", "Prüfung", heute.AddDays(11), lds);
    }

    private int FachAnlegen(string name, string lehrperson)
    {
        var fach = new Fach { Name = name, Lehrperson = lehrperson };
        _fachRepository.Add(fach);
        return fach.Id;
    }

    private void DokumentAnlegen(string titel, string typ, DateTime? frist, int fachId)
    {
        _dokumentRepository.Add(new Dokument
        {
            Titel = titel,
            Typ = typ,
            Frist = frist?.ToString(FristFormat),
            FachId = fachId
        });
    }
}
