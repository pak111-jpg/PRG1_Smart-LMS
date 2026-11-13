using System.Windows;
using SchulApp.DataAccess;
using SchulApp.Services;

namespace SchulApp;

/// <summary>
/// Startpunkt der App und der einzige Ort, der alle Schichten kennt: Hier werden
/// die konkreten Repository-Klassen erzeugt und den Services über den Konstruktor
/// als Interface übergeben (Dependency Injection von Hand). Die Fenster bekommen
/// nur fertige Services.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IFachRepository fachRepository = new FachRepository();
        IDokumentRepository dokumentRepository = new DokumentRepository();

        var hauptfenster = new MainWindow(
            new StartService(fachRepository, dokumentRepository),
            new FachService(fachRepository),
            new DokumentService(dokumentRepository),
            new AuswertungService(fachRepository, dokumentRepository));

        hauptfenster.Show();
    }
}
