using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using SchulApp.Data;
using SchulApp.Models;
using SchulApp.Repositories;

namespace SchulApp;

/// <summary>
/// Code-Behind: ausschliesslich UI-Logik und Aufrufe der Repository-Methoden.
/// Kein MVVM, kein INotifyPropertyChanged – nach jeder Änderung werden die
/// betroffenen Listen neu aus der Datenbank geladen.
/// </summary>
public partial class MainWindow : Window
{
    private readonly FachRepository _fachRepository = new();
    private readonly DokumentRepository _dokumentRepository = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    // ==================== Start ====================

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            DbInitializer.Initialize();
            BeispieldatenAnlegen();   // nur beim allerersten Start, siehe unten

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
            : (LstFaecher.SelectedItem as Fach)?.Id ?? 0;

        List<Fach> faecher = _fachRepository.GetAll();

        LstFaecher.ItemsSource = faecher;
        TxtFachAnzahl.Text = faecher.Count == 1 ? "1 Fach" : $"{faecher.Count} Fächer";
        TxtKeineFaecher.Visibility = faecher.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        // Vorherige Auswahl wiederherstellen, sonst das erste Fach wählen.
        Fach? auswahl = faecher.FirstOrDefault(f => f.Id == merkeId) ?? faecher.FirstOrDefault();
        LstFaecher.SelectedItem = auswahl;

        if (auswahl == null)
            LadeDokumente(null);
    }

    private void LstFaecher_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LadeDokumente(LstFaecher.SelectedItem as Fach);
    }

    private void BtnFachHinzufuegen_Click(object sender, RoutedEventArgs e)
    {
        var name = TxtNeuesFach.Text.Trim();

        if (name.Length == 0)
        {
            Hinweis("Gib zuerst einen Fachnamen ein.");
            TxtNeuesFach.Focus();
            return;
        }

        if (_fachRepository.ExistiertName(name))
        {
            Hinweis($"Das Fach \"{name}\" gibt es bereits.");
            TxtNeuesFach.SelectAll();
            TxtNeuesFach.Focus();
            return;
        }

        try
        {
            var fach = new Fach
            {
                Name = name,
                Lehrperson = TxtNeueLehrperson.Text.Trim(),
                Erstellt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            int neueId = _fachRepository.Add(fach);

            TxtNeuesFach.Clear();
            TxtNeueLehrperson.Clear();
            TxtNeuesFach.Focus();

            LadeFaecher(neueId);
            Status($"Fach \"{name}\" angelegt.");
        }
        catch (Exception ex)
        {
            Fehler("Das Fach konnte nicht gespeichert werden.", ex);
        }
    }

    private void BtnFachLoeschen_Click(object sender, RoutedEventArgs e)
    {
        if (LstFaecher.SelectedItem is not Fach fach)
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
            _fachRepository.Delete(fach.Id);
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

    private void LadeDokumente(Fach? fach)
    {
        if (fach == null)
        {
            GrdDokumente.ItemsSource = null;
            TxtFachUntertitel.Text = "Kein Fach ausgewählt";
            TxtDokumentAnzahl.Text = string.Empty;
            TxtKeineDokumente.Visibility = Visibility.Collapsed;
            BtnDokumentHinzufuegen.IsEnabled = false;
            return;
        }

        List<Dokument> dokumente = _dokumentRepository.GetByFach(fach.Id);

        GrdDokumente.ItemsSource = dokumente;
        BtnDokumentHinzufuegen.IsEnabled = true;

        var offen = dokumente.Count(d => !d.Abgegeben);
        TxtFachUntertitel.Text = string.IsNullOrWhiteSpace(fach.Lehrperson)
            ? fach.Name
            : $"{fach.Name} · {fach.Lehrperson}";
        TxtDokumentAnzahl.Text = $"{dokumente.Count} Dokumente · {offen} offen";
        TxtKeineDokumente.Visibility = dokumente.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void BtnDokumentHinzufuegen_Click(object sender, RoutedEventArgs e)
    {
        if (LstFaecher.SelectedItem is not Fach fach)
        {
            Hinweis("Wähle zuerst ein Fach aus.");
            return;
        }

        var titel = TxtDokTitel.Text.Trim();
        if (titel.Length == 0)
        {
            Hinweis("Gib einen Titel für das Dokument ein.");
            TxtDokTitel.Focus();
            return;
        }

        try
        {
            var dokument = new Dokument
            {
                Titel = titel,
                Typ = (CmbTyp.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Auftrag",
                Dateipfad = TxtDateipfad.Text.Trim(),
                // Frist als "yyyy-MM-dd" speichern – dadurch sortiert SQL korrekt.
                Frist = DpFrist.SelectedDate?.ToString("yyyy-MM-dd"),
                Abgegeben = false,
                FachId = fach.Id
            };

            _dokumentRepository.Add(dokument);

            TxtDokTitel.Clear();
            TxtDateipfad.Clear();
            DpFrist.SelectedDate = null;
            TxtDokTitel.Focus();

            LadeFaecher(fach.Id);   // aktualisiert auch den Dokumentenzähler im Fach
            LadeFristen();
            Status(dokument.HatFrist
                ? $"\"{titel}\" abgelegt, Frist {dokument.FristAnzeige}."
                : $"\"{titel}\" abgelegt.");
        }
        catch (Exception ex)
        {
            Fehler("Das Dokument konnte nicht gespeichert werden.", ex);
        }
    }

    private void TxtDokTitel_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            BtnDokumentHinzufuegen_Click(sender, e);
    }

    private void BtnDateiWaehlen_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Datei verknüpfen",
            Filter = "Alle Dateien (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) == true)
        {
            TxtDateipfad.Text = dialog.FileName;
            if (TxtDokTitel.Text.Trim().Length == 0)
                TxtDokTitel.Text = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
        }
    }

    /// <summary>
    /// Der geforderte Prozess: Abgegeben in der Datenbank umschalten und die
    /// Ansicht neu laden, damit Haken, Zeilenfarbe und Fristenliste stimmen.
    /// </summary>
    private void BtnAbgabeUmschalten_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not Dokument dokument) return;

        try
        {
            bool neuerStatus = !dokument.Abgegeben;
            _dokumentRepository.SetAbgegeben(dokument.Id, neuerStatus);

            LadeDokumente(LstFaecher.SelectedItem as Fach);
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
        if ((sender as FrameworkElement)?.DataContext is not Dokument dokument) return;

        if (MessageBox.Show(this, $"\"{dokument.Titel}\" löschen?", "Dokument löschen",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            _dokumentRepository.Delete(dokument.Id);
            LadeFaecher((LstFaecher.SelectedItem as Fach)?.Id ?? 0);
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
        List<Dokument> fristen = _dokumentRepository.GetOffeneFristen();

        LstFristen.ItemsSource = fristen;
        TxtFristenAnzahl.Text = fristen.Count == 1 ? "1 offen" : $"{fristen.Count} offen";
        TxtKeineFristen.Visibility = fristen.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        int ueberfaellig = fristen.Count(f => f.IstUeberfaellig);
        TxtUeberfaellig.Text = ueberfaellig == 1 ? "1 überfällig" : $"{ueberfaellig} überfällig";
        BdrUeberfaellig.Visibility = ueberfaellig > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>Abgeben direkt aus der Fristenliste – gleicher Workflow wie in der Tabelle.</summary>
    private void BtnFristAbgeben_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not Dokument dokument) return;

        try
        {
            _dokumentRepository.SetAbgegeben(dokument.Id, true);
            LadeDokumente(LstFaecher.SelectedItem as Fach);
            LadeFristen();
            Status($"\"{dokument.Titel}\" ist abgegeben.");
        }
        catch (Exception ex)
        {
            Fehler("Der Status konnte nicht geändert werden.", ex);
        }
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

    /// <summary>
    /// Legt beim allerersten Start ein paar Beispieldaten an, damit die Oberfläche
    /// nicht leer startet. Diese Methode kann ersatzlos gelöscht werden.
    /// </summary>
    private void BeispieldatenAnlegen()
    {
        if (_fachRepository.GetAll().Count > 0) return;

        var heute = DateTime.Today;

        int prg = _fachRepository.Add(new Fach { Name = "PRG I", Lehrperson = "M. Keller" });
        int ism = _fachRepository.Add(new Fach { Name = "ISM", Lehrperson = "S. Brunner" });
        int lds = _fachRepository.Add(new Fach { Name = "LDS II", Lehrperson = "A. Marti" });

        _dokumentRepository.Add(new Dokument
        {
            Titel = "Übung 4 – Schleifen",
            Typ = "Auftrag",
            Frist = heute.AddDays(-2).ToString("yyyy-MM-dd"),
            FachId = prg
        });
        _dokumentRepository.Add(new Dokument
        {
            Titel = "Projektdokumentation",
            Typ = "Projekt",
            Frist = heute.AddDays(4).ToString("yyyy-MM-dd"),
            FachId = prg
        });
        _dokumentRepository.Add(new Dokument
        {
            Titel = "Zusammenfassung Kapitel 1–3",
            Typ = "Notizen",
            FachId = prg
        });
        _dokumentRepository.Add(new Dokument
        {
            Titel = "Fallstudie Datenschutz",
            Typ = "Auftrag",
            Frist = heute.AddDays(1).ToString("yyyy-MM-dd"),
            FachId = ism
        });
        _dokumentRepository.Add(new Dokument
        {
            Titel = "Vorbereitung Prüfung",
            Typ = "Prüfung",
            Frist = heute.AddDays(11).ToString("yyyy-MM-dd"),
            FachId = lds
        });
    }
}
