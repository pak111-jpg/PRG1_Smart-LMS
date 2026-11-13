using System.Text.Json;
using SchulApp.DataAccess;

namespace SchulApp.Tests;

public class DatenbankeinstellungenTests
{
    [Fact]
    public void LeseAusJson_MitConnectionString_GibtWertZurueck()
    {
        // Arrange
        string json = """{ "ConnectionStrings": { "SchulApp": "Data Source=test.db" } }""";

        // Act
        string? connectionString = Datenbankeinstellungen.LeseAusJson(json);

        // Assert
        Assert.Equal("Data Source=test.db", connectionString);
    }

    [Fact]
    public void LeseAusJson_OhneEintrag_GibtNullZurueck()
    {
        // Arrange
        string json = """{ "ConnectionStrings": { } }""";

        // Act
        string? connectionString = Datenbankeinstellungen.LeseAusJson(json);

        // Assert
        Assert.True(connectionString == null, "Ohne Eintrag muss der Standard gelten");
    }

    [Fact]
    public void LeseAusJson_LeererWert_GibtNullZurueck()
    {
        // Arrange
        string json = """{ "ConnectionStrings": { "SchulApp": "  " } }""";

        // Act
        string? connectionString = Datenbankeinstellungen.LeseAusJson(json);

        // Assert
        Assert.True(connectionString == null, "Ein leerer Wert darf den Standard nicht überschreiben");
    }

    [Fact]
    public void LeseAusJson_UngueltigesJson_WirftJsonException()
    {
        // Arrange
        string json = "{ das ist kein JSON";

        // Act + Assert
        Assert.ThrowsAny<JsonException>(() => Datenbankeinstellungen.LeseAusJson(json));
    }
}
