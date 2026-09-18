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
