using System.Windows;
using SchulApp.Models;
using SchulApp.Services;

namespace SchulApp;

/// <summary>
/// Fenster mit der Abgabe-Auswertung pro Fach. Nur UI-Logik: Die Zahlen
/// berechnet der AuswertungService.
/// </summary>
public partial class AuswertungWindow : Window
{
    private readonly AuswertungService _auswertungService = new();

    public AuswertungWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            GrdAuswertung.ItemsSource = _auswertungService.GetAuswertungProFach();

            FachAuswertung gesamt = _auswertungService.GetGesamtauswertung();
            TxtGesamt.Text = gesamt.Gesamt == 0
                ? "Noch keine Dokumente abgelegt."
                : $"{gesamt.FachName}: {gesamt.Abgegeben} von {gesamt.Gesamt} abgegeben ({gesamt.ProzentAnzeige})"
                  + $" · {gesamt.Offen} offen · {gesamt.Ueberfaellig} überfällig";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Die Auswertung konnte nicht geladen werden.\n\n{ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnSchliessen_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
