using System.Windows;
using System.Windows.Controls;
using SchulApp.Dtos;
using SchulApp.Models;
using SchulApp.Services;

namespace SchulApp;

/// <summary>
/// Gesamtübersicht aller Dokumente in einem DataGrid (K2-Challenge).
/// Nur UI-Logik: Suche, Filter und Sortierung werden an den DokumentService
/// weitergereicht - kein DbContext im Code-Behind.
/// </summary>
public partial class AlleDokumenteWindow : Window
{
    private readonly DokumentService _dokumentService;

    public AlleDokumenteWindow(DokumentService dokumentService)
    {
        InitializeComponent();
        _dokumentService = dokumentService;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        LadeDokumente();
        TxtSuche.Focus();
    }

    private void TxtSuche_TextChanged(object sender, TextChangedEventArgs e)
    {
        LadeDokumente();
    }

    private void Auswahl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LadeDokumente();
    }

    private void LadeDokumente()
    {
        // Die ComboBoxen melden ihre Startauswahl schon während InitializeComponent().
        if (!IsLoaded) return;

        try
        {
            // Die Reihenfolge der ComboBox-Einträge entspricht den Enum-Werten.
            var filter = (DokumentFilter)CmbFilter.SelectedIndex;
            var sortierung = (DokumentSortierung)CmbSortierung.SelectedIndex;

            List<DokumentDto> dokumente = _dokumentService.Suche(TxtSuche.Text, filter, sortierung);

            GrdAlleDokumente.ItemsSource = dokumente;
            TxtAnzahl.Text = Anzeigetexte.AnzahlDokumente(dokumente.Count);
            TxtKeineTreffer.Visibility = dokumente.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Die Dokumente konnten nicht geladen werden.\n\n{ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnSchliessen_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
