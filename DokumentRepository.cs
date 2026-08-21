using Microsoft.Data.Sqlite;
using SchulApp.Data;
using SchulApp.Models;

namespace SchulApp.Repositories;

/// <summary>
/// Sämtliche SQL-Zugriffe auf die Tabelle Dokument inkl. Fristenübersicht
/// und dem Workflow "als abgegeben markieren".
/// </summary>
public class DokumentRepository
{
    // ---------- READ ----------

    /// <summary>Alle Dokumente eines Fachs: offene zuerst, dann nach Frist, dann nach Titel.</summary>
    public List<Dokument> GetByFach(int fachId)
    {
        var liste = new List<Dokument>();

        using var connection = DbInitializer.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT  Id, Titel, IFNULL(Typ, ''), Dateipfad, Frist, Abgegeben, FachId
FROM    Dokument
WHERE   FachId = $fachId
ORDER BY Abgegeben ASC,
         CASE WHEN Frist IS NULL OR Frist = '' THEN 1 ELSE 0 END ASC,
         Frist ASC,
         Titel COLLATE NOCASE ASC;";
        command.Parameters.AddWithValue("$fachId", fachId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
            liste.Add(Lesen(reader));

        return liste;
    }

    public Dokument? GetById(int id)
    {
        using var connection = DbInitializer.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Id, Titel, IFNULL(Typ, ''), Dateipfad, Frist, Abgegeben, FachId
FROM   Dokument
WHERE  Id = $id;";
        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Lesen(reader) : null;
    }

    /// <summary>
    /// Fristenübersicht: alle Dokumente mit gesetzter Frist und Abgegeben = 0,
    /// aufsteigend nach Frist sortiert, inkl. Fachname (JOIN über die 1:N-Beziehung).
    /// </summary>
    public List<Dokument> GetOffeneFristen()
    {
        var liste = new List<Dokument>();

        using var connection = DbInitializer.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT  d.Id, d.Titel, IFNULL(d.Typ, ''), d.Dateipfad, d.Frist, d.Abgegeben, d.FachId,
        f.Name AS FachName
FROM    Dokument d
JOIN    Fach f ON f.Id = d.FachId
WHERE   d.Abgegeben = 0
  AND   d.Frist IS NOT NULL
  AND   d.Frist <> ''
ORDER BY d.Frist ASC;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var dokument = Lesen(reader);
            dokument.FachName = reader.GetString(7);
            liste.Add(dokument);
        }

        return liste;
    }

    /// <summary>Anzahl der überfälligen offenen Dokumente (Frist &lt; heute).</summary>
    public int ZaehleUeberfaellig()
    {
        using var connection = DbInitializer.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT COUNT(*)
FROM   Dokument
WHERE  Abgegeben = 0
  AND  Frist IS NOT NULL AND Frist <> ''
  AND  Frist < $heute;";
        command.Parameters.AddWithValue("$heute", DateTime.Today.ToString("yyyy-MM-dd"));

        return Convert.ToInt32(command.ExecuteScalar());
    }

    // ---------- CREATE ----------

    public int Add(Dokument dokument)
    {
        using var connection = DbInitializer.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO Dokument (Titel, Typ, Dateipfad, Frist, Abgegeben, FachId)
VALUES ($titel, $typ, $dateipfad, $frist, $abgegeben, $fachId);
SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$titel", dokument.Titel.Trim());
        command.Parameters.AddWithValue("$typ", dokument.Typ?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue("$dateipfad",
            string.IsNullOrWhiteSpace(dokument.Dateipfad) ? DBNull.Value : dokument.Dateipfad);
        command.Parameters.AddWithValue("$frist",
            string.IsNullOrWhiteSpace(dokument.Frist) ? DBNull.Value : dokument.Frist);
        command.Parameters.AddWithValue("$abgegeben", dokument.Abgegeben ? 1 : 0);
        command.Parameters.AddWithValue("$fachId", dokument.FachId);

        var neueId = Convert.ToInt32(command.ExecuteScalar());
        dokument.Id = neueId;
        return neueId;
    }

    // ---------- UPDATE ----------

    public void Update(Dokument dokument)
    {
        using var connection = DbInitializer.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE Dokument
SET    Titel = $titel,
       Typ = $typ,
       Dateipfad = $dateipfad,
       Frist = $frist,
       Abgegeben = $abgegeben,
       FachId = $fachId
WHERE  Id = $id;";
        command.Parameters.AddWithValue("$titel", dokument.Titel.Trim());
        command.Parameters.AddWithValue("$typ", dokument.Typ?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue("$dateipfad",
            string.IsNullOrWhiteSpace(dokument.Dateipfad) ? DBNull.Value : dokument.Dateipfad);
        command.Parameters.AddWithValue("$frist",
            string.IsNullOrWhiteSpace(dokument.Frist) ? DBNull.Value : dokument.Frist);
        command.Parameters.AddWithValue("$abgegeben", dokument.Abgegeben ? 1 : 0);
        command.Parameters.AddWithValue("$fachId", dokument.FachId);
        command.Parameters.AddWithValue("$id", dokument.Id);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Kern des Workflows: setzt Abgegeben in der Datenbank auf 1 bzw. 0.
    /// Ist Abgegeben = 1, fällt das Dokument automatisch aus der Fristenabfrage heraus.
    /// </summary>
    public void SetAbgegeben(int id, bool abgegeben)
    {
        using var connection = DbInitializer.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Dokument SET Abgegeben = $abgegeben WHERE Id = $id;";
        command.Parameters.AddWithValue("$abgegeben", abgegeben ? 1 : 0);
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    // ---------- DELETE ----------

    public void Delete(int id)
    {
        using var connection = DbInitializer.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Dokument WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    // ---------- Hilfsmethode ----------

    private static Dokument Lesen(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Titel = reader.GetString(1),
        Typ = reader.GetString(2),
        Dateipfad = reader.IsDBNull(3) ? null : reader.GetString(3),
        Frist = reader.IsDBNull(4) ? null : reader.GetString(4),
        Abgegeben = reader.GetInt32(5) == 1,
        FachId = reader.GetInt32(6)
    };
}
