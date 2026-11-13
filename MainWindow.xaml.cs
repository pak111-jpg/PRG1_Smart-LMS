using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SchulApp.Dtos;
using SchulApp.Services;

namespace SchulApp;

/// <summary>
/// Code-Behind: ausschliesslich UI-Logik und Aufrufe der Service-Schicht.
/// Arbeitet nur mit DTOs - die Entity-Klassen kennt das Fenster nicht.
/// Kein MVVM, kein INotifyPropertyChanged – nach jeder Änderung werden die
/// betroffenen Listen neu aus der Datenbank geladen.
/// </summary>
public partial class MainWindow : Window
{
    private readonly StartService _startService;
    private readonly FachService _fachService;
    private readonly DokumentService _dokumentService;
    private readonly AuswertungService _auswertungService;

    public MainWindow(StartService startService, FachService fachService,
        DokumentService dokumentService, AuswertungService auswertungService)
    {
        InitializeComponent();

        _startService = startService;
        _fachService = fachService;
        _dokumentService = dokumentService;
        _auswertungService = auswertungService;
    }

    // ==================== Start ====================

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _startService.Initialisiere();   // Datenbank + Beispieldaten beim allerersten Start

            TxtHeute.Text = DateTime.Today.ToString("dddd, dd. MMMM yyyy");

            LadeFaecher();
            LadeFristen();
            Status("Bereit.");
        }
        catch (Exception ex)
        {
            Fehler("Die Datenbank konnte nicht geöffnet werden.", ex);
        }
    }

    // ==================== Fächer ====================

    private void LadeFaecher(int auswaehlenId = 0)
    {
        var merkeId = auswaehlenId > 0
            ? auswaehlenId
            : (LstFaecher.SelectedItem as FachDto)?.Id ?? 0;

        List<FachDto> faecher = _fachService.GetAlle();

        LstFaecher.ItemsSource = faecher;
        TxtFachAnzahl.Text = faecher.Count == 1 ? "1 Fach" : $"{faecher.Count} Fächer";
        TxtKeineFaecher.Visibility = faecher.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        // Vorherige Auswahl wiederherstellen, sonst das erste Fach wählen.
        FachDto? auswahl = faecher.FirstOrDefault(f => f.Id == merkeId) ?? faecher.FirstOrDefault();
        LstFaecher.SelectedItem = auswahl;

        if (auswahl == null)
            LadeDokumente(null);
    }

    private void LstFaecher_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LadeDokumente(LstFaecher.SelectedItem as FachDto);
    }

    private void BtnFachHinzufuegen_Click(object sender, RoutedEventArgs e)
    {
        var name = TxtNeuesFach.Text.Trim();

        try
        {
            int neueId = _fachService.Hinzufuegen(name, TxtNeueLehrperson.Text);

            TxtNeuesFach.Clear();
            TxtNeueLehrperson.Clear();
            TxtNeuesFach.Focus();

            LadeFaecher(neueId);
            Status($"Fach \"{name}\" angelegt.");
        }
        catch (ValidierungsException ex)
        {
            Hinweis(ex.Message);
            TxtNeuesFach.SelectAll();
            TxtNeuesFach.Focus();
        }
        catch (Exception ex)
        {
            Fehler("Das Fach konnte nicht gespeichert werden.", ex);
        }
    }

    private void BtnFachLoeschen_Click(object sender, RoutedEventArgs e)
    {
        if (LstFaecher.SelectedItem is not FachDto fach)
        {
            Hinweis("Wähle links ein Fach aus.");
            return;
        }

        var frage = fach.AnzahlDokumente > 0
            ? $"\"{fach.Name}\" und {fach.DokumenteAnzeige} löschen?"
            : $"\"{fach.Name}\" löschen?";

        if (MessageBox.Show(this, frage, "Fach löschen",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            _fachService.Loeschen(fach.Id);
            LadeFaecher();
            LadeFristen();
            Status($"Fach \"{fach.Name}\" gelöscht.");
        }
        catch (Exception ex)
        {
            Fehler("Das Fach konnte nicht gelöscht werden.", ex);
        }
    }

    private void TxtNeuesFach_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            BtnFachHinzufuegen_Click(sender, e);
    }

    // ==================== Dokumente ====================

    private void LadeDokumente(FachDto? fach)
    {
        if (fach == null)
        {
            GrdDokumente.ItemsSource = null;
            TxtFachUntertitel.Text = "Kein Fach ausgewählt";
            TxtDokumentAnzahl.Text = string.Empty;
            TxtKeineDokumente.Visibility = Visibility.Collapsed;
            PrgAbgabe.Visibility = Visibility.Collapsed;
            DokEingabe.KannAblegen = false;
            return;
        }

        List<DokumentDto> dokumente = _dokumentService.GetByFach(fach.Id);

        GrdDokumente.ItemsSource = dokumente;
        DokEingabe.KannAblegen = true;

        int offen = _dokumentService.ZaehleOffene(dokumente);
        TxtFachUntertitel.Text = string.IsNullOrWhiteSpace(fach.Lehrperson)
            ? fach.Name
            : $"{fach.Name} · {fach.Lehrperson}";
        TxtDokumentAnzahl.Text = $"{dokumente.Count} Dokumente · {offen} offen";
        TxtKeineDokumente.Visibility = dokumente.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        // Fortschrittsbalken: Anteil der abgegebenen Dokumente in diesem Fach.
        int abgegeben = dokumente.Count - offen;
        PrgAbgabe.Maximum = Math.Max(dokumente.Count, 1);
        PrgAbgabe.Value = abgegeben;
        PrgAbgabe.ToolTip = $"{abgegeben} von {dokumente.Count} abgegeben";
        PrgAbgabe.Visibility = dokumente.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>
    /// Reagiert auf das Ablegen-Ereignis des UserControls DokumentEingabe:
    /// Eingaben aus dem Control lesen und über den Service speichern.
    /// </summary>
    private void DokEingabe_Ablegen(object? sender, EventArgs e)
    {
        if (LstFaecher.SelectedItem is not FachDto fach)
        {
            Hinweis("Wähle zuerst ein Fach aus.");
            return;
        }

        var titel = DokEingabe.Titel;

        try
        {
            DokumentDto dokument = _dokumentService.Hinzufuegen(
                titel,
                DokEingabe.Typ,
                DokEingabe.Dateipfad,
                DokEingabe.Frist,
                fach.Id);

            DokEingabe.Leeren();
            DokEingabe.FokusAufTitel();

            LadeFaecher(fach.Id);   // aktualisiert auch den Dokumentenzähler im Fach
            LadeFristen();
            Status(dokument.HatFrist
                ? $"\"{titel}\" abgelegt, Frist {dokument.FristAnzeige}."
                : $"\"{titel}\" abgelegt.");
        }
        catch (ValidierungsException ex)
        {
            Hinweis(ex.Message);
            DokEingabe.FokusAufTitel();
        }
        catch (Exception ex)
        {
            Fehler("Das Dokument konnte nicht gespeichert werden.", ex);
        }
    }

    /// <summary>
    /// Der geforderte Prozess: Abgegeben in der Datenbank umschalten und die
    /// Ansicht neu laden, damit Haken, Zeilenfarbe und Fristenliste stimmen.
    /// </summary>
    private void BtnAbgabeUmschalten_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not DokumentDto dokument) return;

        try
        {
            bool neuerStatus = _dokumentService.AbgabeUmschalten(dokument);

            LadeFaecher();   // aktualisiert Dokumentenliste und den Offen-Zähler im Fach
            LadeFristen();

            Status(neuerStatus
                ? $"\"{dokument.Titel}\" ist abgegeben."
                : $"\"{dokument.Titel}\" ist wieder offen.");
        }
        catch (Exception ex)
        {
            Fehler("Der Status konnte nicht geändert werden.", ex);
        }
    }

    private void BtnDokumentLoeschen_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not DokumentDto dokument) return;

        if (MessageBox.Show(this, $"\"{dokument.Titel}\" löschen?", "Dokument löschen",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            _dokumentService.Loeschen(dokument.Id);
            LadeFaecher((LstFaecher.SelectedItem as FachDto)?.Id ?? 0);
            LadeFristen();
            Status($"\"{dokument.Titel}\" gelöscht.");
        }
        catch (Exception ex)
        {
            Fehler("Das Dokument konnte nicht gelöscht werden.", ex);
        }
    }

    // ==================== Fristenübersicht ====================

    private void LadeFristen()
    {
        List<DokumentDto> fristen = _dokumentService.GetOffeneFristen();

        LstFristen.ItemsSource = fristen;
        TxtFristenAnzahl.Text = fristen.Count == 1 ? "1 offen" : $"{fristen.Count} offen";
        TxtKeineFristen.Visibility = fristen.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        int ueberfaellig = _dokumentService.ZaehleUeberfaellige(fristen);
        TxtUeberfaellig.Text = ueberfaellig == 1 ? "1 überfällig" : $"{ueberfaellig} überfällig";
        BdrUeberfaellig.Visibility = ueberfaellig > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>Abgeben direkt aus der Fristenliste – gleicher Workflow wie in der Tabelle.</summary>
    private void BtnFristAbgeben_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not DokumentDto dokument) return;

        try
        {
            _dokumentService.MarkiereAlsAbgegeben(dokument.Id);
            LadeFaecher();   // aktualisiert Dokumentenliste und den Offen-Zähler im Fach
            LadeFristen();
            Status($"\"{dokument.Titel}\" ist abgegeben.");
        }
        catch (Exception ex)
        {
            Fehler("Der Status konnte nicht geändert werden.", ex);
        }
    }

    // ==================== Menü ====================

    private void MnuAktualisieren_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            LadeFaecher();
            LadeFristen();
            Status("Ansicht aktualisiert.");
        }
        catch (Exception ex)
        {
            Fehler("Die Daten konnten nicht geladen werden.", ex);
        }
    }

    private void MnuBeenden_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MnuNeuesFach_Click(object sender, RoutedEventArgs e)
    {
        TxtNeuesFach.Focus();
    }

    private void MnuNeuesDokument_Click(object sender, RoutedEventArgs e)
    {
        if (LstFaecher.SelectedItem is not FachDto)
        {
            Hinweis("Wähle zuerst ein Fach aus.");
            return;
        }

        DokEingabe.FokusAufTitel();
    }

    /// <summary>Öffnet die Gesamtübersicht aller Dokumente mit Suche, Filter und Sortierung.</summary>
    private void MnuAlleDokumente_Click(object sender, RoutedEventArgs e)
    {
        new AlleDokumenteWindow(_dokumentService) { Owner = this }.ShowDialog();
    }

    /// <summary>Öffnet die Abgabe-Auswertung (AuswertungService) als Dialog.</summary>
    private void MnuAuswertung_Click(object sender, RoutedEventArgs e)
    {
        new AuswertungWindow(_auswertungService) { Owner = this }.ShowDialog();
    }

    private void MnuUeber_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(this,
            "SchulApp – Dokumente und Fristen\n\nFächer verwalten, Dokumente ablegen und Abgabefristen im Blick behalten.",
            "Über SchulApp", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ==================== Hilfsmethoden ====================

    private void Status(string text) =>
        TxtStatus.Text = $"{DateTime.Now:HH:mm}  ·  {text}";

    private void Hinweis(string text)
    {
        Status(text);
        MessageBox.Show(this, text, "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Fehler(string text, Exception ex)
    {
        Status(text);
        MessageBox.Show(this, $"{text}\n\n{ex.Message}", "Fehler",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
