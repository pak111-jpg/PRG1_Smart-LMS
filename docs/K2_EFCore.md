# K2 - ORM / Entity Framework Core

Der Datenzugriff läuft neu über EF Core statt über rohes ADO.NET. Umgebaut wurde nur
die DataAccess-Schicht: Die Methodensignaturen sind unverändert, `Services/` und
`MainWindow.xaml.cs` wurden in K2 nicht angefasst.

## Was ist neu?

| Datei                              | Aufgabe                                                                 |
|------------------------------------|-------------------------------------------------------------------------|
| `DataAccess/AppDbContext.cs`       | DbContext mit `DbSet<Fach> Faecher` und `DbSet<Dokument> Dokumente`, Verbindung per `UseSqlite`, Beziehung und Tabellennamen in `OnModelCreating` |
| `Migrations/`                      | Migration `InitialDbCreation` plus Model-Snapshot - daraus entsteht die Datenbank |
| `DataAccess/DbInitializer.cs`      | ruft beim Start nur noch `context.Database.Migrate()` auf               |
| `DataAccess/FachDataAccess.cs`     | LINQ statt SQL                                                          |
| `DataAccess/DokumentDataAccess.cs` | LINQ statt SQL, `Include()` für die Fristenübersicht                    |
| `Models/Fach.cs`, `Models/Dokument.cs` | Navigation Properties `Fach.Dokumente` und `Dokument.Fach`          |

## Entscheide

- **Tabellennamen:** EF Core würde die Tabellen nach den DbSets benennen (`Faecher`,
  `Dokumente`). Mit `ToTable("Fach")` / `ToTable("Dokument")` bleibt das Schema wie in K0/K1.
- **Anzeige-Eigenschaften:** Eigenschaften ohne Setter (`Initialen`, `FristAnzeige`,
  `FachName`, ...) bildet EF Core von sich aus nicht ab. Nur `Fach.AnzahlDokumente` hat
  einen Setter und ist deshalb mit `[NotMapped]` markiert.
- **Gross-/Kleinschreibung:** Vorher stand `COLLATE NOCASE` in den SQL-Abfragen. Neu ist
  die Collation einmal am Modell hinterlegt (`UseCollation("NOCASE")` für `Fach.Name`
  und `Dokument.Titel`) und gilt damit für Sortierung und Namensvergleich.
- **Fach löschen:** Die Dokumente löscht die Datenbank über die Beziehung mit
  (`OnDelete(DeleteBehavior.Cascade)`), das explizite zweite DELETE entfällt.
- **Neuer Dateiname `schulapp.db`:** Die alte `app.db` wurde per CREATE-TABLE-Skript
  angelegt und kennt keine Migrations. `Migrate()` würde dort mit "table Fach already
  exists" scheitern. Die alte Datei bleibt liegen, die Beispieldaten werden neu angelegt.
- **Kurzlebiger Context:** Jede DataAccess-Methode erzeugt ihren eigenen Context mit
  `using` - kein Context als Feld.

## Schema-Vergleich (B2, Schritt 6)

Die generierte Migration entspricht dem bisherigen Schema: Tabellen `Fach` und
`Dokument`, gleiche Spalten, Fremdschlüssel `FachId` mit ON DELETE CASCADE, Indizes
`IX_Dokument_FachId` und `IX_Dokument_Frist`. Unterschiede: `Lehrperson` und `Typ` sind
neu NOT NULL (der Code schrieb dort schon immer einen leeren Text statt NULL), `Name` und
`Titel` haben die Collation NOCASE.

## Vertiefung: Include() und das N+1-Problem

Gemessen an der Fristenübersicht mit den Beispieldaten (4 offene Dokumente mit Frist):

| Variante                                              | SQL-Abfragen |
|-------------------------------------------------------|--------------|
| `Include(d => d.Fach)`                                | 1            |
| ohne Include, Fach pro Dokument einzeln nachgeladen   | 5 (1 + 4)    |

`Include()` lohnt sich, wenn die verbundenen Daten für (fast) jede Zeile gebraucht werden -
wie hier der Fachname in der Fristenliste. Es lohnt sich nicht, wenn die Navigation
Property gar nicht angezeigt wird (z. B. `GetByFach`: das Fach ist dort schon bekannt) -
dann macht der JOIN die Abfrage nur grösser.

`FachDataAccess.GetAll()` zählt die Dokumente pro Fach ebenfalls in derselben Abfrage
(`Select(f => new { Fach = f, Anzahl = f.Dokumente.Count })`) statt pro Fach nachzuladen.

## Kontrolle (B4)

- Suche nach `SqliteConnection` und `SqliteCommand` im ganzen Projekt: keine Treffer.
  Das Paket `Microsoft.Data.Sqlite` ist nicht mehr direkt referenziert.
- Verhaltensvergleich: Ein Testszenario über Services und DataAccess (Beispieldaten,
  Anlegen mit Validierung, Sortierung, Abgabe umschalten, Update, Löschen mit Kaskade)
  liefert mit EF Core zeilengenau dieselbe Ausgabe wie vorher mit ADO.NET.

## Arbeiten mit Migrations

Nach einer Änderung an einer Entity-Klasse in der Paket-Manager-Konsole:

```
Add-Migration <SprechenderName>
```

`Update-Database` ist nicht nötig - die App führt fehlende Migrations beim Start selbst aus.

## Challenge: Datenbindung ans DataGrid

Das Fenster **Dokument > Alle Dokumente…** (`AlleDokumenteWindow`) zeigt die ganze
Tabelle `Dokument` über alle Fächer in einem DataGrid.

- **Daten über die Service-Schicht:** Das Code-Behind ruft nur
  `DokumentService.Suche(suchtext, filter, sortierung)` auf und setzt das Ergebnis als
  `ItemsSource`. Kein `AppDbContext` und kein `using SchulApp.DataAccess` im Fenster.
- **Suche, Filter, Sortierung als LINQ-Abfrage:** `DokumentDataAccess.Suche(...)` setzt
  die Abfrage schrittweise zusammen (`IQueryable<Dokument>`): `Include(d => d.Fach)` für
  den Fachnamen, `Where` mit `EF.Functions.Like` für die Titelsuche, `Where` je nach
  Statusfilter (`DokumentFilter`: Alle, Offen, Abgegeben, Überfällig) und `OrderBy` je
  nach `DokumentSortierung` (Frist, Titel, Fach). Erst `ToList()` schickt ein einziges
  SQL-Statement an die Datenbank - gefiltert und sortiert wird also in SQLite, nicht im
  Speicher.
- **Sortierung im Grid ausgeschaltet** (`CanUserSortColumns="False"`), damit die
  Reihenfolge wirklich aus der LINQ-Abfrage kommt.

## Reflexion & KI-Protokoll (K2)

Fortsetzung des Protokolls aus `K0_KI_Protokoll.md` und `K1_Schichtenarchitektur.md`.

| Nr | Thema | Prompt (Kernfrage) | Was die KI geliefert hat | Gut / korrigiert | Gelernt |
|----|-------|--------------------|--------------------------|------------------|---------|
| K2-1 | Analyse (Prompt 1) | _TODO Team: Prompt aus dem ursprünglichen K2-Umbau_ | _TODO_ | _TODO_ | _TODO_ |
| K2-2 | Umbau (Prompt 2) | _TODO Team_ | _TODO_ | _TODO_ | _TODO_ |
| K2-3 | Kontrolle (Prompt 3) | _TODO Team_ | _TODO_ | _TODO_ | _TODO_ |
| K2-4 | K2 abschliessen (Claude Code) | "Branch ist angelegt, kannst mit K2 weitermachen" - Abgleich des Projekts mit den K2-Aufgaben. | Befund: B1-B4 und die Vertiefung (Include, N+1-Messung) waren vorhanden; offen waren die Challenge (DataGrid) und das KI-Protokoll. | _TODO Team_ | _TODO Team_ |
| K2-5 | DataGrid mit Filter und Sortierung (Challenge) | Teil von K2-4: ganze Tabelle in einem DataGrid, Daten über die Service-Schicht, Filter/Sortierung als LINQ-Abfrage. | `AlleDokumenteWindow`, `DokumentService.Suche(...)`, `DokumentDataAccess.Suche(...)` mit zusammengesetztem `IQueryable`, Enums `DokumentFilter` und `DokumentSortierung`. | _TODO Team_ | _TODO Team_ |

Fragen aus Kapitel 7 - bitte aus eigener Erfahrung beantworten:

- **Welche Prompts habt ihr verwendet, um den Umstieg auf EF Core zu planen?** _TODO Team_
- **Was hat die KI beim Umbau auf EF Core gut gemacht - wo musstet ihr korrigieren?** _TODO Team_
- **Wo wollte die KI zu schnell Interfaces oder ein Repository-Pattern einführen - und wie habt ihr sie zurückgeholt?** _TODO Team_
- **Was ist am Umgang mit der Datenbank jetzt einfacher als vorher - und was ist (noch) ungewohnt?** _TODO Team_
