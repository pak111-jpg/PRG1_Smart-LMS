using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace SchulApp.Controls;

/// <summary>
/// UserControl mit der Eingabemaske für ein neues Dokument (Entität 2).
/// Enthält nur UI-Logik: Das Speichern übernimmt das Fenster, das auf das
/// Ereignis <see cref="Ablegen"/> reagiert und die Eingaben über die
/// Eigenschaften ausliest.
/// </summary>
public partial class DokumentEingabe : UserControl
{
    /// <summary>Wird ausgelöst, wenn "Dokument ablegen" geklickt oder Enter gedrückt wurde.</summary>
    public event EventHandler? Ablegen;

    public DokumentEingabe()
    {
        InitializeComponent();
    }

    // ---------- Eingaben auslesen ----------

    public string Titel => TxtDokTitel.Text.Trim();

    public string? Typ => (CmbTyp.SelectedItem as ComboBoxItem)?.Content?.ToString();

    public string Dateipfad => TxtDateipfad.Text;

    public DateTime? Frist => DpFrist.SelectedDate;

    /// <summary>Ohne ausgewähltes Fach kann nichts abgelegt werden.</summary>
    public bool KannAblegen
    {
        get => BtnDokumentHinzufuegen.IsEnabled;
        set => BtnDokumentHinzufuegen.IsEnabled = value;
    }

    // ---------- Vom Fenster aufrufbar ----------

    /// <summary>Leert die Maske nach dem Speichern; der Typ bleibt für die nächste Eingabe stehen.</summary>
    public void Leeren()
    {
        TxtDokTitel.Clear();
        TxtDateipfad.Clear();
        DpFrist.SelectedDate = null;
    }

    public void FokusAufTitel()
    {
        TxtDokTitel.Focus();
    }

    // ---------- Event-Handler ----------

    private void BtnDokumentHinzufuegen_Click(object sender, RoutedEventArgs e)
    {
        Ablegen?.Invoke(this, EventArgs.Empty);
    }

    private void TxtDokTitel_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && KannAblegen)
            Ablegen?.Invoke(this, EventArgs.Empty);
    }

    private void BtnDateiWaehlen_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Datei verknüpfen",
            Filter = "Alle Dateien (*.*)|*.*"
        };

        if (dialog.ShowDialog(Window.GetWindow(this)) == true)
        {
            TxtDateipfad.Text = dialog.FileName;
            if (TxtDokTitel.Text.Trim().Length == 0)
                TxtDokTitel.Text = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
        }
    }
}
