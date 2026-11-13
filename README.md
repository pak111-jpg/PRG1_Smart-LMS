# SchulApp - Dokumente und Fristen

WPF-Desktop-App aus dem Modul PROG1: Schulunterlagen pro Fach ablegen und
Abgabefristen im Blick behalten. Dieselbe App wurde von K0 bis K5 Schritt für Schritt
umgebaut - dieses README fasst den Lernweg anhand der Branches und Pull Requests
zusammen.

## Starten

Voraussetzung: .NET 10 SDK (Windows).

```
dotnet run --project SchulApp.csproj
```

Tests ausführen:

```
dotnet test
```

Die SQLite-Datenbank `schulapp.db` entsteht beim ersten Start neben der App und wird
über EF-Core-Migrations angelegt. Wer eine andere Datenbankdatei verwenden will,
kopiert `appsettings.Example.json` nach `appsettings.json` und passt den Wert an.
`appsettings.json` wird nicht versioniert.

## Lernweg K0-K5

| Schritt | Thema | Branch | Pull Request | Tag |
|---------|-------|--------|--------------|-----|
| K0 | App mit KI erstellen: Code-Behind, ADO.NET, SQLite | - (erster Upload auf `main`) | - | - |
| K1 | Schichten: UI / Service / DataAccess | `feature/k1-schichten` | [#1](https://github.com/pak111-jpg/PRG1_Smart-LMS/pull/1) | - |
| K2 | ADO.NET wird EF Core, Migrations, LINQ | `feature/k2-efcore` | [#2](https://github.com/pak111-jpg/PRG1_Smart-LMS/pull/2) | - |
| K0-Z | WPF-Vertiefung: Menu, ProgressBar, UserControl, KI-Protokoll | `feature/k0-wpf-vertiefung` | [#3](https://github.com/pak111-jpg/PRG1_Smart-LMS/pull/3) | `v1.0-k0` |
| K1 | Challenge: zweite Serviceklasse (Auswertung) | `feature/k1-schichten-abschluss` | [#4](https://github.com/pak111-jpg/PRG1_Smart-LMS/pull/4) | `v1.0-k1` |
| K2 | Challenge: DataGrid mit Suche, Filter, Sortierung | `feature/k2-efcore-abschluss` | [#5](https://github.com/pak111-jpg/PRG1_Smart-LMS/pull/5) | `v1.0-k2` |
| K3 | Interfaces, Repository-Pattern, DTOs | `feature/k3-interfaces-repository` | [#6](https://github.com/pak111-jpg/PRG1_Smart-LMS/pull/6) | `v1.0-k3` |
| K4 | Clean Code, xUnit-Tests mit Fake-Repositories | `feature/k4-cleancode-testing` | [#7](https://github.com/pak111-jpg/PRG1_Smart-LMS/pull/7) | `v1.0-k4` |
| K5 | Git-Workflow: README, .gitignore, Einstellungsdatei, Tags | `feature/k5-git-workflow` | _TODO: verlinken_ | `v1.0-k5` |

Die Basis von K1 und K2 entstand im September/Oktober; die Vertiefungen und Challenges
dazu wurden später in eigenen Branches (`...-abschluss`) nachgezogen. Deshalb erscheinen
K1 und K2 zweimal.

### Wichtigste Commits pro Schritt

- **K1:** `cdd6866` Models und DataAccess in Ordner verschoben, `b813b46` FachService,
  `ab230d1` DokumentService, `6df3c9f` StartService - das Code-Behind kennt nur noch die
  Service-Schicht.
- **K2:** `43e2fd2` EF-Core-Pakete und AppDbContext, `421b932` Schema über Migrations,
  `4f4a37f` / `c2d1ae7` DataAccess auf LINQ umgestellt, `9e738ee` letzter ADO.NET-Code
  entfernt.
- **K0-Z:** `d57ddc2` Menu, ProgressBar und UserControl `DokumentEingabe`.
- **K1 (Challenge):** `f07d080` AuswertungService und Fenster "Abgabestand pro Fach".
- **K2 (Challenge):** `3565425` Fenster "Alle Dokumente" mit Suche, Filter und Sortierung.
- **K3:** `699a71f` Services erhalten Repository-Interfaces und bilden auf DTOs ab,
  `504f44b` Fenster arbeiten mit DTOs, Verdrahtung in `App.xaml.cs`.
- **K4:** `c8a0a80` Clean-Code-Überarbeitung und Mapper-Klassen, `1112595`
  xUnit-Testprojekt mit Fake-Repositories.

## Aufbau

```
SchulApp.sln
├── SchulApp.csproj          WPF-App
│   ├── App.xaml(.cs)        Start: erzeugt Repositories und Services
│   ├── *Window.xaml(.cs)    UI (Code-Behind), kennt nur Services und DTOs
│   ├── Controls/            UserControl DokumentEingabe
│   ├── Services/            Regeln, Abläufe, Mapping Entity -> DTO
│   ├── Dtos/                Datenobjekte für die UI
│   ├── DataAccess/          Interfaces, Repositories, AppDbContext
│   ├── Models/              Entities Fach und Dokument
│   └── Migrations/          EF-Core-Migrations
├── SchulApp.Tests/          xUnit-Tests und Fake-Repositories
└── docs/                    Dokumentation und KI-Protokoll pro K-Schritt
```

## Dokumentation

- [K0 - KI-Protokoll und WPF-Vertiefung](docs/K0_KI_Protokoll.md)
- [K1 - Schichtenarchitektur](docs/K1_Schichtenarchitektur.md)
- [K2 - EF Core](docs/K2_EFCore.md)
- [K3 - Interfaces, Repository, DTOs](docs/K3_Interfaces_Repository_DTOs.md)
- [K4 - Clean Code und Unit Testing](docs/K4_CleanCode_UnitTesting.md)
- [K5 - Git und Kollaboration](docs/K5_Git_Kollaboration.md)

## Arbeitsweise im Repository

- `main` ist immer lauffähig; gearbeitet wird auf `feature/kN-thema`.
- Commit-Messages nach Conventional Commits (`feat:`, `refactor:`, `test:`, `docs:`).
- Jeder Branch kommt über einen Pull Request mit Review nach `main`.
