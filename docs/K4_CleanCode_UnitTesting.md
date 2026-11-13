# K4 - Clean Code und Unit Testing

Die Architektur aus K3 ist unverändert. In K4 wurde der Code nach der
Clean-Code-Checkliste überarbeitet und sein Verhalten mit Unit Tests abgesichert.

## B1 - Code-Review

### Naming

| Vorher | Nachher | Regel |
|--------|---------|-------|
| `Status(...)`, `Hinweis(...)`, `Fehler(...)` (MainWindow) | `ZeigeStatus`, `ZeigeHinweis`, `ZeigeFehler` | Methoden sind Verben |
| `FokusAufTitel()` (DokumentEingabe) | `SetzeFokusAufTitel()` | Methoden sind Verben |
| `BeispieldatenAnlegen()` (StartService) | `LegeBeispieldatenAn()` | einheitliche Verbform |
| `KannAblegen` | `IstAblegenMoeglich` | bool beginnt mit ist/hat |
| `neuerStatus` | `istNeuAbgegeben` | bool beginnt mit ist/hat |
| `DokEingabe`, `TxtDokTitel` | `Eingabemaske`, `TxtDokumentTitel` | keine Abkürzungen |
| `prg`, `ism`, `lds` (lokale Variablen) | `programmierenId`, `sicherheitId`, `datenstrukturenId` | aussagekräftig, keine Abkürzungen |
| `d`, `f`, `t` in Lambdas und Methoden | `dokument`, `fach`, `bereinigt` | aussagekräftig statt kurz |

Kommentare nach Zusatzblatt: Abschnittsbanner (`// ---------- READ ----------`,
`// ==================== Fächer ====================`) entfernt, am Zeilenende
angehängte Kommentare auf eine eigene Zeile davor verschoben, öffentliche Methoden mit
XML-Kommentaren (`<returns>`, `<exception>`) ergänzt.

Bewusst belassen:

- **Präfixe an Steuerelementen** (`TxtStatus`, `BtnFachLoeschen`, `LstFaecher`): Sie
  bezeichnen die Art des Steuerelements, nicht einen Datentyp, und unterscheiden z. B.
  `TxtFachAnzahl` (Anzeige) von einer Zahl `fachAnzahl`.
- **`Get...`, `Add`, `Update`, `Delete`, `Set...`** in Repositories und Services: Die
  Namen stammen aus `IRepository<T>` (K3-Unterlagen) und werden einheitlich so
  weitergeführt (`GetByFach`, `SetAbgegeben`). Fachbegriffe bleiben deutsch.
- **`Dokument.Abgegeben`** (bool ohne ist/hat): Der Name ist gleichzeitig der
  Spaltenname in der Datenbank; eine Umbenennung bräuchte eine Migration.

### Magic Numbers und Magic Strings

| Wert | Wo vorher | Jetzt |
|------|-----------|-------|
| `"yyyy-MM-dd"` | DokumentService, StartService, DokumentRepository (3x) | `Dokument.FristFormat` |
| `"yyyy-MM-dd HH:mm:ss"` | FachService, FachRepository (2x) | `Fach.ErstelltFormat` |
| `"Auftrag"`, `"Prüfung"`, `"Notizen"`, `"Skript"`, `"Projekt"` | XAML der Eingabemaske, DokumentService, StartService | Klasse `DokumentTypen` (inkl. `Standard` und `Alle`); die ComboBox wird daraus gefüllt |
| `"–"` | DokumentService, AuswertungService | `Anzeigetexte.KeinWert` |
| `"✓"`, `"○"`, `"Abgegeben"`, `"Offen"`, `"dd.MM.yyyy"` | Mapping im DokumentService | Konstanten in `DokumentMapper` |
| `"Keine Lehrperson"`, `"?"`, `2` (Anzahl Initialen) | Mapping im FachService | Konstanten in `FachMapper` |
| `100.0` | AuswertungService | `HundertProzent` |

### DRY

- "1 Dokument" / "n Dokumente" stand an drei Stellen. Jetzt:
  `Anzeigetexte.AnzahlDokumente(anzahl)`. Dabei ist ein Anzeigefehler verschwunden:
  Der Kopf der Dokumentenliste zeigte "1 Dokumente".
- Die Sortierung "ohne Frist ans Ende, dann nach Frist, dann nach Titel" stand im
  `DokumentRepository` zweimal und liegt jetzt in `SortiereNachFrist`.

## B2 - Single Responsibility

**1. Service und Formatierung getrennt.** `DokumentService` und `FachService` enthielten
seit K3 neben Regeln und Abläufen auch das Mapping auf DTOs samt allen Anzeigetexten
(Datumsformat, "in 3 Tagen", Initialen). Das ist genau das Warnsignal aus den
Unterlagen: Geschäftslogik und Formatierung in einer Klasse. Das Mapping liegt jetzt in
`DokumentMapper` und `FachMapper`. Eine Änderung an einem Anzeigetext betrifft nur noch
den Mapper, eine Änderung an einer Regel nur noch den Service - und beides lässt sich
getrennt testen.

**2. Eine Methode, drei Aufgaben.** `DokumentRepository.Suche` filterte nach Titel,
filterte nach Status (switch) und sortierte (switch) in einer einzigen Methode. Sie ruft
jetzt drei Methoden mit je einer Aufgabe auf: `FiltereNachTitel`, `FiltereNachStatus`
und `Sortiere`.

**3. Systemuhr aus der Regel entfernt.** `Dokument.IstUeberfaellig` und die Berechnung
der Restzeit griffen direkt auf `DateTime.Today` zu. Neu wird der Stichtag übergeben
(`IstUeberfaelligAm(stichtag)`, `BerechneRestText(frist, heute)`); nur die Services
fragen die Uhr ab. Dadurch sind die Regeln mit festen Daten testbar.

## B3 - xUnit-Testprojekt

- `SchulApp.sln` enthält `SchulApp` und `SchulApp.Tests`.
- `SchulApp.Tests/SchulApp.Tests.csproj`: xUnit (`xunit`, `xunit.runner.visualstudio`,
  `Microsoft.NET.Test.Sdk`), Projektverweis auf `SchulApp.csproj`.
- Weil das Testprojekt in einem Unterordner des Hauptprojekts liegt, schliesst
  `SchulApp.csproj` den Ordner `SchulApp.Tests` vom Kompilieren aus.

Tests ausführen: Test-Explorer in Visual Studio oder `dotnet test` im Projektordner.

| Testklasse | Tests | Was wird geprüft |
|------------|-------|------------------|
| `FachServiceTests` | 7 | Fach anlegen (gültig, mit Leerzeichen), leerer Name, doppelter Name, Löschen, Kennzahlen im FachDto |
| `DokumentServiceTests` | 9 | Dokument ablegen, Standardtyp, leerer Titel, Abgabe umschalten in beide Richtungen, unbekannte Id, Fristenübersicht, Zähler, Löschen |
| `DokumentMapperTests` | 12 | Restzeit-Text (6 Fälle), Regel "überfällig" (4 Fälle), Mapping mit und ohne Frist/Datei |
| `AuswertungServiceTests` | 3 | Kennzahlen pro Fach, Fach ohne Dokumente, Gesamtauswertung |
| `FachMapperTests` | 4 | Initialen (zwei Wörter, ein Wort, ein Buchstabe, leer) |
| **Total** | **35** | |

Alle Tests folgen dem AAA-Muster; die Namen sind nach
`Methode_Ausgangslage_ErwartetesResultat` aufgebaut.

## B4 - Kontrolle

Negativtests (echte Fehlerfälle) sind unter anderem:

- `Hinzufuegen_LeererName_WirftValidierungsException`
- `Hinzufuegen_NameExistiertBereits_WirftValidierungsException`
- `Hinzufuegen_LeererTitel_WirftValidierungsException`
- `MarkiereAlsAbgegeben_NichtExistierendesDokument_AendertNichts`

Stand der Prüfung: Die 35 Tests liefen grün in einer Kopie des Projekts mit .NET 8 und
EF Core 8, weil auf dem Entwicklungsrechner kein .NET-10-SDK installiert war.
_TODO Team: einmal im Test-Explorer mit .NET 10 ausführen und hier bestätigen._

## Vertiefung: Fake-Repositories

`SchulApp.Tests/Fakes/FakeFachRepository.cs` und `FakeDokumentRepository.cs`
implementieren `IFachRepository` bzw. `IDokumentRepository` mit einer `List<T>` im
Speicher. Alle Service-Tests verwenden diese Fakes - kein Test spricht eine Datenbank an.

**Wäre das ohne das Interface aus K3 möglich gewesen?** Nein. Vor K3 erzeugten die
Services ihre DataAccess-Klasse selbst (`new FachDataAccess()`), und diese öffnete direkt
die SQLite-Datei. Ein Test hätte eine echte Datenbank gebraucht und wäre von deren
Inhalt abhängig gewesen. Erst weil die Services ihr Repository über den Konstruktor als
Interface erhalten, kann der Test eine andere Implementierung einsetzen.

## Challenge: Testabdeckung

- Methode mit mehreren Fallunterscheidungen: `DokumentMapper.BerechneRestText` hat sechs
  Fälle (ohne Frist, mehrere Tage überfällig, 1 Tag überfällig, heute, morgen, in
  mehreren Tagen) - pro Fall ein eigener Test in `DokumentMapperTests`.
- Anzahl Tests im Projekt: **35**.
- Code Coverage: nicht gemessen. Einschätzung: Services, Mapper und die Regeln in
  `Dokument` sind weitgehend abgedeckt. Nicht abgedeckt sind die echten
  Repository-Klassen (EF Core), `DbInitializer` und das Code-Behind der Fenster.
  _TODO Team (optional): in Visual Studio über Test > Code Coverage messen._

## Reflexion & KI-Protokoll (K4)

Fortsetzung des Protokolls aus K0-K3.

| Nr | Thema | Prompt (Kernfrage) | Was die KI geliefert hat | Gut / korrigiert | Gelernt |
|----|-------|--------------------|--------------------------|------------------|---------|
| K4-1 | Code-Review (B1, B2) | "Weiter mit K4" - K4-Unterlagen und Naming-Zusatzblatt als Vorgabe, bestehender Code als Kontext. | Liste der Naming-Verstösse, Magic Strings und SRP-Stellen (siehe oben) und deren Umsetzung. | _TODO Team_ | _TODO Team_ |
| K4-2 | Testprojekt und Fakes (B3, Vertiefung) | Teil von K4-1. | Solution, xUnit-Testprojekt, zwei Fake-Repositories. | _TODO Team_ | _TODO Team_ |
| K4-3 | Tests generieren (B3, Challenge) | Teil von K4-1. | 35 Tests in fünf Testklassen, davon pro Fall von `BerechneRestText` einer. | Eine Analyzer-Warnung (`Assert.Equal` für die Listengrösse) wurde auf `Assert.Single` korrigiert. _TODO Team: Tests lesen und fachlich prüfen._ | _TODO Team_ |

Fragen aus Kapitel 7 - bitte aus eigener Erfahrung beantworten:

- **Welche Prompts habt ihr verwendet, um Code-Smells zu finden und Tests zu generieren?** _TODO Team_
- **Wo hat die KI beim Testen gut geholfen - wo musstet ihr die generierten Tests korrigieren oder ergänzen?** _TODO Team_
- **Hat die KI Testfälle vorgeschlagen, die fachlich falsch waren? Woran habt ihr das erkannt?** _TODO Team_ (Zum Prüfen: Die Tests legen fest, dass ein Dokument am Tag der Frist noch nicht überfällig ist und dass Fachnamen ohne Rücksicht auf Gross-/Kleinschreibung eindeutig sein müssen - stimmt das mit eurer Absicht überein?)
- **Was hat sich für euch am meisten verändert - der Code selbst oder euer Vertrauen in seine Korrektheit?** _TODO Team_
