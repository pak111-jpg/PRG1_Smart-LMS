using Microsoft.Data.Sqlite;

namespace SchulApp.Data;

/// <summary>
/// Legt die SQLite-Datenbank und beide Tabellen an, falls sie noch nicht existieren.
/// Kein ORM – nur SqliteConnection / SqliteCommand.
/// </summary>
public static class DbInitializer
{
    public const string ConnectionString = "Data Source=app.db";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        // Fremdschlüssel müssen in SQLite pro Verbindung aktiviert werden.
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            pragma.ExecuteNonQuery();
        }

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Fach (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT NOT NULL,
    Lehrperson  TEXT,
    Erstellt    TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Dokument (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Titel       TEXT NOT NULL,
    Typ         TEXT,
    Dateipfad   TEXT,
    Frist       TEXT,
    Abgegeben   INTEGER NOT NULL DEFAULT 0,
    FachId      INTEGER NOT NULL,
    FOREIGN KEY (FachId) REFERENCES Fach(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_Dokument_FachId ON Dokument(FachId);
CREATE INDEX IF NOT EXISTS IX_Dokument_Frist  ON Dokument(Frist);";
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>Öffnet eine Verbindung mit aktivierten Fremdschlüsseln.</summary>
    public static SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return connection;
    }
}
