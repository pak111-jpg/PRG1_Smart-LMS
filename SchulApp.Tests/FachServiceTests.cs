using SchulApp.Models;
using SchulApp.Services;
using SchulApp.Tests.Fakes;

namespace SchulApp.Tests;

public class FachServiceTests
{
    [Fact]
    public void Hinzufuegen_GueltigerName_SpeichertFachUndGibtIdZurueck()
    {
        // Arrange
        var repository = new FakeFachRepository();
        var service = new FachService(repository);

        // Act
        int fachId = service.Hinzufuegen("Mathematik", "T. Muster");

        // Assert
        var gespeichert = repository.GetById(fachId);
        Assert.True(gespeichert != null, "Das Fach wurde nicht gespeichert");
        Assert.Equal("Mathematik", gespeichert!.Name);
        Assert.Equal("T. Muster", gespeichert.Lehrperson);
    }

    [Fact]
    public void Hinzufuegen_NameMitLeerzeichenAmRand_SpeichertBereinigtenNamen()
    {
        // Arrange
        var repository = new FakeFachRepository();
        var service = new FachService(repository);

        // Act
        int fachId = service.Hinzufuegen("  Mathematik  ", "  T. Muster ");

        // Assert
        var gespeichert = repository.GetById(fachId)!;
        Assert.Equal("Mathematik", gespeichert.Name);
        Assert.Equal("T. Muster", gespeichert.Lehrperson);
    }

    [Fact]
    public void Hinzufuegen_LeererName_WirftValidierungsException()
    {
        // Arrange
        var repository = new FakeFachRepository();
        var service = new FachService(repository);

        // Act + Assert
        Assert.Throws<ValidierungsException>(() => service.Hinzufuegen("   ", "T. Muster"));
        Assert.True(repository.GetAll().Count == 0, "Trotz leerem Namen wurde ein Fach gespeichert");
    }

    [Fact]
    public void Hinzufuegen_NameExistiertBereits_WirftValidierungsException()
    {
        // Arrange
        var repository = new FakeFachRepository();
        var service = new FachService(repository);
        service.Hinzufuegen("Mathematik", "T. Muster");

        // Act + Assert
        Assert.Throws<ValidierungsException>(() => service.Hinzufuegen("mathematik", "A. Andere"));
        Assert.Single(repository.GetAll());
    }

    [Fact]
    public void Loeschen_ExistierendesFach_EntferntEsAusDemRepository()
    {
        // Arrange
        var repository = new FakeFachRepository();
        var service = new FachService(repository);
        int fachId = service.Hinzufuegen("Mathematik", "T. Muster");

        // Act
        service.Loeschen(fachId);

        // Assert
        Assert.True(repository.GetById(fachId) == null, "Das Fach ist nach dem Löschen noch vorhanden");
    }

    [Fact]
    public void GetAlle_FachMitOffenenUndAbgegebenenDokumenten_BerechnetBeideKennzahlen()
    {
        // Arrange
        var repository = new FakeFachRepository();
        repository.Add(new Fach
        {
            Name = "Mathematik",
            Dokumente =
            {
                new Dokument { Titel = "Serie 1", Abgegeben = true },
                new Dokument { Titel = "Serie 2", Abgegeben = false },
                new Dokument { Titel = "Serie 3", Abgegeben = false }
            }
        });
        var service = new FachService(repository);

        // Act
        var fach = service.GetAlle().Single();

        // Assert
        Assert.Equal(3, fach.AnzahlDokumente);
        Assert.Equal(2, fach.AnzahlOffen);
        Assert.Equal("3 Dokumente", fach.DokumenteAnzeige);
        Assert.Equal(" · 2 offen", fach.OffenAnzeige);
    }

    [Fact]
    public void GetAlle_FachOhneLehrperson_ZeigtPlatzhaltertext()
    {
        // Arrange
        var repository = new FakeFachRepository();
        repository.Add(new Fach { Name = "Mathematik", Lehrperson = "" });
        var service = new FachService(repository);

        // Act
        var fach = service.GetAlle().Single();

        // Assert
        Assert.Equal(FachMapper.TextKeineLehrperson, fach.LehrpersonAnzeige);
        Assert.Equal("1 Dokument", Anzeigetexte.AnzahlDokumente(1));
        Assert.Equal(string.Empty, fach.OffenAnzeige);
    }
}
