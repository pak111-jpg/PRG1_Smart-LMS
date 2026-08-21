using Microsoft.Data.Sqlite;
using SchulApp.Data;
using SchulApp.Models;

namespace SchulApp.Repositories;

/// <summary>
/// Sämtliche SQL-Zugriffe auf die Tabelle Fach. Direkte SQL-Abfragen,
/// Parameter immer über SqliteParameter (kein String-Zusammenbau).
/// </summary>
public class FachRepository
{
    // ---------- READ ----------

    /// <summary>Alle Fächer inkl. Anzahl der zugehörigen Dokumente, alphabetisch.</summary>
    public List<Fach> GetAll()
    {
        var liste = new List<Fach>();

        using var connection = DbInitializer.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT  f.Id,
        f.Name,
        IFNULL(f.Lehrperson, '') AS Lehrperson,
        f.Erstellt,
        (SELECT COUNT(*) FROM Dokument d WHERE d.FachId = f.Id) AS AnzahlDokumente
FROM    Fach f
ORDER BY f.Name COLLATE NOCASE ASC;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            liste.Add(new Fach
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Lehrperson = reader.GetString(2),
                Erstellt = reader.GetString(3),
                AnzahlDokumente = reader.GetInt32(4)
            });
        }

        return liste;
    }

    public Fach? GetById(int id)
    {
        using var connection = DbInitializer.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Id, Name, IFNULL(Lehrperson, ''), Erstellt
FROM   Fach
WHERE  Id = $id;";
        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        return new Fach
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Lehrperson = reader.GetString(2),
            Erstellt = reader.GetString(3)
        };
    }

    /// <summary>Prüft, ob ein Fachname bereits vergeben ist (optional eigene Id ausschliessen).</summary>
    public bool ExistiertName(string name, int ausserId = 0)
    {
        using var connection = DbInitializer.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT COUNT(*) FROM Fach
WHERE Name = $name COLLATE NOCASE AND Id <> $id;";
        command.Parameters.AddWithValue("$name", name.Trim());
        command.Parameters.AddWithValue("$id", ausserId);

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    // ---------- CREATE ----------

    /// <summary>Fügt ein Fach ein und liefert die neue Id zurück.</summary>
    public int Add(Fach fach)
    {
        using var connection = DbInitializer.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Fach (Name, Lehrperson, Erstellt)
VALUES ($name, $lehrperson, $erstellt);
SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$name", fach.Name.Trim());
        command.Parameters.AddWithValue("$lehrperson", fach.Lehrperson?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue("$erstellt",
            string.IsNullOrWhiteSpace(fach.Erstellt)
                ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                : fach.Erstellt);

        var neueId = Convert.ToInt32(command.ExecuteScalar());
        fach.Id = neueId;
        return neueId;
    }

    // ---------- UPDATE ----------

    public void Update(Fach fach)
    {
        using var connection = DbInitializer.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE Fach
SET    Name = $name,
       Lehrperson = $lehrperson
WHERE  Id = $id;";
        command.Parameters.AddWithValue("$name", fach.Name.Trim());
        command.Parameters.AddWithValue("$lehrperson", fach.Lehrperson?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue("$id", fach.Id);
        command.ExecuteNonQuery();
    }

    // ---------- DELETE ----------

    /// <summary>
    /// Löscht ein Fach samt seinen Dokumenten in einer Transaktion.
    /// (ON DELETE CASCADE greift zusätzlich, das explizite DELETE macht die Absicht sichtbar.)
    /// </summary>
    public void Delete(int id)
    {
        using var connection = DbInitializer.OpenConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();

        using (var dokumente = connection.CreateCommand())
        {
            dokumente.Transaction = transaction;
            dokumente.CommandText = "DELETE FROM Dokument WHERE FachId = $id;";
            dokumente.Parameters.AddWithValue("$id", id);
            dokumente.ExecuteNonQuery();
        }

        using (var fach = connection.CreateCommand())
        {
            fach.Transaction = transaction;
            fach.CommandText = "DELETE FROM Fach WHERE Id = $id;";
            fach.Parameters.AddWithValue("$id", id);
            fach.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}
