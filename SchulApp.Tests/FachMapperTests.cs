using SchulApp.Services;

namespace SchulApp.Tests;

public class FachMapperTests
{
    [Fact]
    public void BerechneInitialen_ZweiWoerter_NimmtJeDenErstenBuchstaben()
    {
        // Act
        string initialen = FachMapper.BerechneInitialen("prg eins");

        // Assert
        Assert.Equal("PE", initialen);
    }

    [Fact]
    public void BerechneInitialen_EinWort_NimmtDieErstenZweiBuchstaben()
    {
        // Act
        string initialen = FachMapper.BerechneInitialen("Mathe");

        // Assert
        Assert.Equal("MA", initialen);
    }

    [Fact]
    public void BerechneInitialen_EinBuchstabe_GibtDiesenGrossZurueck()
    {
        // Act
        string initialen = FachMapper.BerechneInitialen("m");

        // Assert
        Assert.Equal("M", initialen);
    }

    [Fact]
    public void BerechneInitialen_LeererName_GibtPlatzhalterZurueck()
    {
        // Act
        string initialen = FachMapper.BerechneInitialen("   ");

        // Assert
        Assert.Equal(FachMapper.InitialenOhneName, initialen);
    }
}
