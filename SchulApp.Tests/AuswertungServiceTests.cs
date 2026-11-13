using SchulApp.Models;
using SchulApp.Services;
using SchulApp.Tests.Fakes;

namespace SchulApp.Tests;

public class AuswertungServiceTests
{
    [Fact]
    public void GetAuswertungProFach_FachMitGemischtenDokumenten_BerechnetKennzahlen()
    {
        // Arrange
        var gestern = DateTime.Today.AddDays(-1).ToString(Dokument.FristFormat);
        var fachRepository = new FakeFachRepository();
        fachRepository.Add(new Fach
        {
            Name = "Mathematik",
            Dokumente =
            {
                new Dokument { Titel = "Abgegeben", Abgegeben = true },
                new Dokument { Titel = "Offen", Abgegeben = false },
                new Dokument { Titel = "Überfällig", Abgegeben = false, Frist = gestern }
            }
        });
        var service = new AuswertungService(fachRepository, new FakeDokumentRepository());

        // Act
        var auswertung = service.GetAuswertungProFach().Single();

        // Assert
        Assert.Equal("Mathematik", auswertung.FachName);
        Assert.Equal(3, auswertung.Gesamt);
        Assert.Equal(1, auswertung.Abgegeben);
        Assert.Equal(2, auswertung.Offen);
        Assert.Equal(1, auswertung.Ueberfaellig);
        Assert.Equal(33, auswertung.ProzentAbgegeben);
        Assert.Equal("33 %", auswertung.ProzentAnzeige);
    }

    [Fact]
    public void GetAuswertungProFach_FachOhneDokumente_ZeigtNullUndPlatzhalter()
    {
        // Arrange
        var fachRepository = new FakeFachRepository();
        fachRepository.Add(new Fach { Name = "Mathematik" });
        var service = new AuswertungService(fachRepository, new FakeDokumentRepository());

        // Act
        var auswertung = service.GetAuswertungProFach().Single();

        // Assert
        Assert.Equal(0, auswertung.Gesamt);
        Assert.Equal(0, auswertung.ProzentAbgegeben);
        Assert.Equal(Anzeigetexte.KeinWert, auswertung.ProzentAnzeige);
    }

    [Fact]
    public void GetGesamtauswertung_DokumenteAusMehrerenFaechern_SummiertUeberAlleFaecher()
    {
        // Arrange
        var dokumentRepository = new FakeDokumentRepository();
        dokumentRepository.Add(new Dokument { Titel = "A", FachId = 1, Abgegeben = true });
        dokumentRepository.Add(new Dokument { Titel = "B", FachId = 1, Abgegeben = true });
        dokumentRepository.Add(new Dokument { Titel = "C", FachId = 2, Abgegeben = true });
        dokumentRepository.Add(new Dokument { Titel = "D", FachId = 2, Abgegeben = false });
        var service = new AuswertungService(new FakeFachRepository(), dokumentRepository);

        // Act
        var gesamt = service.GetGesamtauswertung();

        // Assert
        Assert.Equal(AuswertungService.GesamtBezeichnung, gesamt.FachName);
        Assert.Equal(4, gesamt.Gesamt);
        Assert.Equal(3, gesamt.Abgegeben);
        Assert.Equal(75, gesamt.ProzentAbgegeben);
    }
}
