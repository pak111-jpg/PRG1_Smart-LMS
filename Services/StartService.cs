using SchulApp.DataAccess;
using SchulApp.Models;

namespace SchulApp.Services;

/// <summary>
/// Ablauf beim Programmstart: Datenbank vorbereiten und beim allerersten Start
/// Beispieldaten anlegen. Die UI ruft nur Initialisiere() auf und kennt weder
/// DbInitializer noch die DataAccess-Klassen.
/// </summary>
public class StartService
{
    private const string FristFormat = "yyyy-MM-dd";

    private readonly FachDataAccess _fachDataAccess = new();
    private readonly DokumentDataAccess _dokumentDataAccess = new();

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
        if (_fachDataAccess.GetAll().Count > 0) return;

        var heute = DateTime.Today;

        int prg = _fachDataAccess.Add(new Fach { Name = "PRG I", Lehrperson = "M. Keller" });
        int ism = _fachDataAccess.Add(new Fach { Name = "ISM", Lehrperson = "S. Brunner" });
        int lds = _fachDataAccess.Add(new Fach { Name = "LDS II", Lehrperson = "A. Marti" });

        _dokumentDataAccess.Add(new Dokument
        {
            Titel = "Übung 4 – Schleifen",
            Typ = "Auftrag",
            Frist = heute.AddDays(-2).ToString(FristFormat),
            FachId = prg
        });
        _dokumentDataAccess.Add(new Dokument
        {
            Titel = "Projektdokumentation",
            Typ = "Projekt",
            Frist = heute.AddDays(4).ToString(FristFormat),
            FachId = prg
        });
        _dokumentDataAccess.Add(new Dokument
        {
            Titel = "Zusammenfassung Kapitel 1–3",
            Typ = "Notizen",
            FachId = prg
        });
        _dokumentDataAccess.Add(new Dokument
        {
            Titel = "Fallstudie Datenschutz",
            Typ = "Auftrag",
            Frist = heute.AddDays(1).ToString(FristFormat),
            FachId = ism
        });
        _dokumentDataAccess.Add(new Dokument
        {
            Titel = "Vorbereitung Prüfung",
            Typ = "Prüfung",
            Frist = heute.AddDays(11).ToString(FristFormat),
            FachId = lds
        });
    }
}
