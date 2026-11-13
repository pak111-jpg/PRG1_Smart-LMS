# K0 - KI-gestützte App-Erstellung: KI-Protokoll

Dieses Dokument deckt K0 (B1-B5) und die Zusatzeinheit K0-Z (A1-A3) ab.

> Hinweis zum Stand: Der ursprüngliche K0-Code (Code-Behind + ADO.NET mit
> `Microsoft.Data.Sqlite`) wurde in K1 in Schichten aufgeteilt und in K2 auf
> EF Core umgestellt. Der K0-Stand ist in der Git-Historie nachvollziehbar
> (erster Upload und die Commits vor `feature/k1-schichten`).

## 1. App-Idee (B1)

**SchulApp - Dokumente und Fristen:** Schulunterlagen pro Fach ablegen und
Abgabefristen im Blick behalten.

| Punkt            | Umsetzung                                                                 |
|------------------|---------------------------------------------------------------------------|
| Entität 1        | `Fach` (Id, Name, Lehrperson, Erstellt)                                   |
| Entität 2        | `Dokument` (Id, Titel, Typ, Dateipfad, Frist, Abgegeben, FachId)          |
| 1:N-Beziehung    | Ein Fach hat mehrere Dokumente; Fremdschlüssel `FachId` in `Dokument`     |
| Kernprozess      | Dokument als abgegeben markieren (und zurücknehmen)                       |
| Sichtbarer Effekt| Haken, grüne Zeile, Status-Text; das Dokument fällt aus "Offene Fristen"; überfällige Dokumente werden rot markiert und oben gezählt |
| Nutzwert         | Übersicht über alle offenen Abgaben über alle Fächer, sortiert nach Datum |

## 2. Angepasster Starter-Prompt (B1)

```text
Erstelle eine WPF Desktop-Applikation in C# mit .NET 10 mit folgenden Anforderungen:

App-Idee: Eine Schul-App, in der ich pro Fach Dokumente (Aufträge, Prüfungen,
Notizen, Skripte, Projekte) ablege und Abgabefristen überwache.

Technische Rahmenbedingungen (PFLICHT - NICHT ändern!):
- WPF mit Code-Behind (KEIN MVVM, kein INotifyPropertyChanged, kein ViewModel!)
- SQLite mit Microsoft.Data.Sqlite (KEIN Entity Framework, KEIN ORM!)
- Nur direkte SQL-Abfragen: SqliteConnection, SqliteCommand, SqliteDataReader
- Ziel-Framework: .NET 10

Datenmodell (1:N-Beziehung):
- Entität 1: Fach (Id INTEGER PK, Name TEXT, Lehrperson TEXT, Erstellt TEXT)
- Entität 2: Dokument (Id INTEGER PK, Titel TEXT, Typ TEXT, Dateipfad TEXT,
  Frist TEXT, Abgegeben INTEGER, FachId INTEGER FK)

Prozess / Workflow:
- Ein Dokument als abgegeben markieren; Status in der DB aktualisieren und in der
  UI sichtbar machen (Haken und Zeilenfarbe). Offene Dokumente mit Frist
  erscheinen in einer Fristenübersicht, überfällige werden rot hervorgehoben.

Mindestfunktionen:
1. DbInitializer.cs mit CREATE TABLE IF NOT EXISTS für beide Tabellen (inkl. FK)
2. Fächer anzeigen (ListBox)
3. Fach hinzufügen (Textfeld + Button + INSERT) und löschen (DELETE)
4. Dokumente anzeigen (DataGrid), gefiltert nach gewähltem Fach
5. Dokument hinzufügen und als abgegeben markieren

Struktur:
- Alle DB-Zugriffe in Repository-Klassen (FachRepository.cs, DokumentRepository.cs)
- Im Code-Behind nur UI-Logik und Aufrufe der Repository-Methoden
- Verbindungszeichenfolge: "Data Source=app.db"
```

## 3. KI-Protokoll (B1, B5)

Pro Eintrag: Kernfrage, was die KI geliefert hat, was gut war bzw. korrigiert
werden musste, was wir gelernt haben.

| Nr | Thema | Prompt (Kernfrage) | Was die KI geliefert hat | Gut / korrigiert | Gelernt |
|----|-------|--------------------|--------------------------|------------------|---------|
| 1  | Starter-Prompt (K0) | _TODO Team: siehe Abschnitt 2_ | _TODO_ | _TODO_ | _TODO_ |
| 2  | _TODO Team: Interaktion aus der K0-Entwicklung_ | _TODO_ | _TODO_ | _TODO_ | _TODO_ |
| 3  | _TODO Team: Interaktion aus der K0-Entwicklung_ | _TODO_ | _TODO_ | _TODO_ | _TODO_ |
| 4  | K0 abschliessen, K0-Z umsetzen (Claude Code) | "Arbeite nach den K-Punkten, stelle unser Projekt anhand der Unterlagen fertig, fange bei K0 an." | Abgleich des Projekts mit K0/K0-Z: Basis B2-B4 war erfüllt; offen waren KI-Protokoll und K0-Z. Vorschlag: ProgressBar + Menu (A2), Eingabemaske als UserControl (A3). | Gut: Die KI hat zuerst den Ist-Stand geprüft statt neu zu generieren. Zu prüfen: _TODO Team_ | _TODO Team_ |
| 5  | ProgressBar + Menu (A2) | Teil von Nr. 4: zwei weitere Steuerelemente, die zum Workflow passen und echte Daten verwenden. | `ProgressBar` "Abgabe-Fortschritt" im Dokumente-Kopf (abgegeben / alle Dokumente des Fachs) und `Menu` (Datei, Fach, Dokument, Hilfe) mit Click-Handlern im Code-Behind. | _TODO Team_ | _TODO Team_ |
| 6  | UserControl (A3) | Teil von Nr. 4: Eingabemaske von Entität 2 in ein UserControl umbauen. | `Controls/DokumentEingabe.xaml(.cs)`; Styles von `Window.Resources` nach `App.xaml` verschoben, weil ein UserControl die Ressourcen des Fensters beim Laden nicht findet. | _TODO Team_ | _TODO Team_ |

**Selbstbewertung (B5):** Was hat die KI gut gemacht, wo mussten wir korrigieren?

_TODO Team: 3-5 Sätze aus eigener Erfahrung._

## 4. K0-Z A1 - Analyse der eigenen App

**Layoutelemente (Panels) in `MainWindow.xaml`:**

- `Grid` - Grundraster des Fensters (Zeilen: Kopf, Inhalt, Statuszeile) und die
  drei Spalten Fächer / Dokumente / Fristen.
- `StackPanel` - stapelt z.B. Menü und Kopfleiste oder Feldtitel und Textfeld.
- `Border` - die weissen "Karten" mit runden Ecken und Schatten (Style `Karte`).
- `ScrollViewer` - macht die Fristenliste scrollbar.

**Steuerelemente:** `ListBox` (`LstFaecher`), `DataGrid` (`GrdDokumente`),
`TextBox` (`TxtNeuesFach`), `Button` (`BtnFachHinzufuegen`), `ProgressBar`
(`PrgAbgabe`), `Menu`; im UserControl `DokumentEingabe` zusätzlich `ComboBox`
(`CmbTyp`) und `DatePicker` (`DpFrist`).

**Zwei Ereignisse und ihre Registrierung:**

| Ereignis | Registrierung | Handler |
|----------|---------------|---------|
| Klick auf "Fach anlegen" | XAML-Attribut `Click="BtnFachHinzufuegen_Click"` | `BtnFachHinzufuegen_Click` in `MainWindow.xaml.cs` |
| Auswahl in der Fächerliste | XAML-Attribut `SelectionChanged="LstFaecher_SelectionChanged"` | `LstFaecher_SelectionChanged` lädt die Dokumente des gewählten Fachs |

Alle Handler sind im XAML registriert, keiner per `+=` im Code. Eine Ausnahme
in die andere Richtung ist das eigene Ereignis `Ablegen` des UserControls: Es
wird im Control mit `Ablegen?.Invoke(...)` ausgelöst und im Hauptfenster wieder
per XAML-Attribut (`Ablegen="DokEingabe_Ablegen"`) abonniert.

**Was passiert beim Start des Fensters?**
`App.xaml` öffnet über `StartupUri` das `MainWindow`. Dessen Konstruktor ruft
nur `InitializeComponent()` auf; damit wird das XAML geladen und alle
Steuerelemente mit `x:Name` stehen als Felder zur Verfügung. Unser eigener Code
läuft danach im `Loaded`-Ereignis (`Window_Loaded`): Datenbank vorbereiten
(`StartService.Initialisiere()`), Datum setzen, dann `LadeFaecher()` und
`LadeFristen()`. Weil `LadeFaecher()` das erste Fach auswählt, löst das
`SelectionChanged` aus und die Dokumente werden ebenfalls geladen.

**Zugriff über `x:Name`:**
In `LadeFaecher()` wird `LstFaecher.ItemsSource = faecher;` gesetzt - die
ListBox zeigt damit die frisch geladenen Fächer an. Direkt danach wird über
`TxtFachAnzahl.Text` der Zähler ("3 Fächer") und über
`TxtKeineFaecher.Visibility` der Leer-Hinweis ein- oder ausgeblendet.

**Erkenntnisse (5-8 Sätze): Was war neu, was war bekannt?**

_TODO Team: aus eigener Sicht formulieren._

## 5. K0-Z A2 - Weitere Steuerelemente

| Steuerelement | Wo | Daten / Aktion |
|---------------|----|----------------|
| `ComboBox` (`CmbTyp`) | Eingabemaske Dokument | Typ des Dokuments, wird in der DB gespeichert |
| `DatePicker` (`DpFrist`) | Eingabemaske Dokument | Abgabefrist, wird in der DB gespeichert |
| `ProgressBar` (`PrgAbgabe`) | Kopf der Dokumente-Karte | Anteil abgegebener Dokumente des gewählten Fachs; reagiert sofort auf "Abgeben" / "Zurücknehmen", Hinzufügen und Löschen |
| `Menu` | oberster Fensterrand | Datei (Aktualisieren, Beenden), Fach (Neu, Löschen), Dokument (Neu), Hilfe (Über) |

Test: Fach wählen, ein Dokument abgeben -> Balken wächst und der Tooltip zeigt
"x von y abgegeben"; bei einem Fach ohne Dokumente ist der Balken ausgeblendet.

## 6. K0-Z A3 - Eigenes UserControl

Die Eingabemaske für Dokumente (Titel, Typ, Frist, Dateipfad, Buttons) liegt
neu in `Controls/DokumentEingabe.xaml` und wird im Hauptfenster so eingebunden:

```xml
<controls:DokumentEingabe x:Name="DokEingabe" Grid.Row="2" Margin="0,14,0,0"
                          Ablegen="DokEingabe_Ablegen"/>
```

Was sich durch die Umstellung geändert hat:

- **Klassendeklaration:** `public partial class DokumentEingabe : UserControl`
  (statt `: Window`), im Namespace `SchulApp.Controls`.
- **XAML-Wurzelelement:** `<UserControl x:Class="SchulApp.Controls.DokumentEingabe" ...>`
  statt `<Window ...>`.
- **Title-Attribut:** entfällt, ebenso `Height`/`Width`/`WindowStartupLocation` -
  ein UserControl hat keinen Fensterrahmen.
- **Einbindung:** Im Hauptfenster braucht es den Namespace
  `xmlns:controls="clr-namespace:SchulApp.Controls"`.
- **Kommunikation:** Das Fenster greift nicht mehr direkt auf `TxtDokTitel` usw.
  zu, sondern auf Eigenschaften (`Titel`, `Typ`, `Dateipfad`, `Frist`,
  `KannAblegen`), Methoden (`Leeren()`, `FokusAufTitel()`) und das Ereignis
  `Ablegen`. Gespeichert wird weiterhin im Hauptfenster über `DokumentService` -
  das UserControl enthält nur UI-Logik.
- **Styles:** von `Window.Resources` nach `Application.Resources` (`App.xaml`)
  verschoben, damit Fenster und UserControl dieselben Styles verwenden.
