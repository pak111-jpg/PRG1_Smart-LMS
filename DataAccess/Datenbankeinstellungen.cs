using System.IO;
using System.Text.Json;

namespace SchulApp.DataAccess;

/// <summary>
/// Liefert die Verbindungszeichenfolge zur Datenbank. Sie kann in einer lokalen
/// appsettings.json neben der App überschrieben werden; diese Datei wird nicht
/// versioniert (siehe .gitignore), als Vorlage dient appsettings.Example.json.
/// Ohne Datei gilt die lokale SQLite-Datei.
/// </summary>
public static class Datenbankeinstellungen
{
    public const string StandardConnectionString = "Data Source=schulapp.db";
    public const string Dateiname = "appsettings.json";

    private const string AbschnittConnectionStrings = "ConnectionStrings";
    private const string NameDerVerbindung = "SchulApp";

    private static string? s_connectionString;

    /// <summary>Die gültige Verbindungszeichenfolge; wird beim ersten Zugriff ermittelt.</summary>
    /// <exception cref="InvalidOperationException">Wenn appsettings.json kein gültiges JSON enthält.</exception>
    public static string ConnectionString => s_connectionString ??= Ermittle();

    /// <summary>
    /// Liest die Verbindungszeichenfolge aus dem Inhalt einer Einstellungsdatei.
    /// </summary>
    /// <returns>Den Wert von ConnectionStrings:SchulApp - oder null, wenn er fehlt oder leer ist.</returns>
    /// <exception cref="JsonException">Wenn der Inhalt kein gültiges JSON ist.</exception>
    public static string? LeseAusJson(string json)
    {
        using var dokument = JsonDocument.Parse(json);

        if (dokument.RootElement.ValueKind != JsonValueKind.Object) return null;
        if (!dokument.RootElement.TryGetProperty(AbschnittConnectionStrings, out var abschnitt)) return null;
        if (abschnitt.ValueKind != JsonValueKind.Object) return null;
        if (!abschnitt.TryGetProperty(NameDerVerbindung, out var wert)) return null;
        if (wert.ValueKind != JsonValueKind.String) return null;

        string? connectionString = wert.GetString();
        return string.IsNullOrWhiteSpace(connectionString) ? null : connectionString;
    }

    private static string Ermittle()
    {
        string pfad = Path.Combine(AppContext.BaseDirectory, Dateiname);
        if (!File.Exists(pfad)) return StandardConnectionString;

        try
        {
            return LeseAusJson(File.ReadAllText(pfad)) ?? StandardConnectionString;
        }
        catch (JsonException fehler)
        {
            throw new InvalidOperationException(
                $"Die Datei {Dateiname} enthält kein gültiges JSON: {fehler.Message}", fehler);
        }
    }
}
