using SchulApp.Models;
using SchulApp.Services;
using SchulApp.Tests.Fakes;

namespace SchulApp.Tests;

public class DokumentServiceTests
{
    private const int FachId = 1;

    [Fact]
    public void Hinzufuegen_GueltigeEingaben_SpeichertOffenesDokument()
    {
        // Arrange
        var repository = new FakeDokumentRepository();
        var service = new DokumentService(repository);
        var frist = new DateTime(2030, 5, 17);

        // Act
        var dokument = service.Hinzufuegen("Serie 1", DokumentTypen.Projekt, null, frist, FachId);

        // Assert
        var gespeichert = repository.GetById(dokument.Id);
        Assert.True(gespeichert != null, "Das Dokument wurde nicht gespeichert");
        Assert.Equal("Serie 1", gespeichert!.Titel);
        Assert.Equal(DokumentTypen.Projekt, gespeichert.Typ);
        Assert.Equal("2030-05-17", gespeichert.Frist);
        Assert.Equal(FachId, gespeichert.FachId);
        Assert.False(gespeichert.Abgegeben, "Ein neues Dokument darf nicht schon abgegeben sein");
    }

    [Fact]
    public void Hinzufuegen_OhneTyp_VerwendetStandardtyp()
    {
        // Arrange
        var service = new DokumentService(new FakeDokumentRepository());

        // Act
        var dokument = service.Hinzufuegen("Serie 1", null, null, null, FachId);

        // Assert
        Assert.Equal(DokumentTypen.Standard, dokument.Typ);
    }

    [Fact]
    public void Hinzufuegen_LeererTitel_WirftValidierungsException()
    {
        // Arrange
        var repository = new FakeDokumentRepository();
        var service = new DokumentService(repository);

        // Act + Assert
        Assert.Throws<ValidierungsException>(() => service.Hinzufuegen("  ", DokumentTypen.Auftrag, null, null, FachId));
        Assert.True(repository.GetAll().Count == 0, "Trotz leerem Titel wurde ein Dokument gespeichert");
    }

    [Fact]
    public void AbgabeUmschalten_OffenesDokument_WirdAbgegebenUndGibtTrueZurueck()
    {
        // Arrange
        var repository = new FakeDokumentRepository();
        var service = new DokumentService(repository);
        var dokument = service.Hinzufuegen("Serie 1", null, null, null, FachId);

        // Act
        bool istNeuAbgegeben = service.AbgabeUmschalten(dokument);

        // Assert
        Assert.True(istNeuAbgegeben, "Offenes Dokument wurde nicht als abgegeben gemeldet");
        Assert.True(repository.GetById(dokument.Id)!.Abgegeben, "Der neue Status wurde nicht gespeichert");
    }

    [Fact]
    public void AbgabeUmschalten_AbgegebenesDokument_WirdWiederOffenUndGibtFalseZurueck()
    {
        // Arrange
        var repository = new FakeDokumentRepository();
        var service = new DokumentService(repository);
        int dokumentId = service.Hinzufuegen("Serie 1", null, null, null, FachId).Id;
        service.MarkiereAlsAbgegeben(dokumentId);
        var abgegebenesDokument = service.GetByFach(FachId).Single();

        // Act
        bool istNeuAbgegeben = service.AbgabeUmschalten(abgegebenesDokument);

        // Assert
        Assert.False(istNeuAbgegeben, "Abgegebenes Dokument wurde nicht als offen gemeldet");
        Assert.False(repository.GetById(dokumentId)!.Abgegeben, "Der neue Status wurde nicht gespeichert");
    }

    [Fact]
    public void MarkiereAlsAbgegeben_NichtExistierendesDokument_AendertNichts()
    {
        // Arrange
        var repository = new FakeDokumentRepository();
        var service = new DokumentService(repository);
        int dokumentId = service.Hinzufuegen("Serie 1", null, null, null, FachId).Id;
        int unbekannteId = 999;

        // Act
        service.MarkiereAlsAbgegeben(unbekannteId);

        // Assert
        Assert.False(repository.GetById(dokumentId)!.Abgegeben, "Ein anderes Dokument wurde fälschlicherweise abgegeben");
    }

    [Fact]
    public void GetOffeneFristen_AbgegebenesDokument_FaelltAusDerFristenuebersicht()
    {
        // Arrange
        var service = new DokumentService(new FakeDokumentRepository());
        var morgen = DateTime.Today.AddDays(1);
        int abgegebenId = service.Hinzufuegen("Abgegeben", null, null, morgen, FachId).Id;
        service.Hinzufuegen("Offen", null, null, morgen, FachId);
        service.Hinzufuegen("Ohne Frist", null, null, null, FachId);
        service.MarkiereAlsAbgegeben(abgegebenId);

        // Act
        var fristen = service.GetOffeneFristen();

        // Assert
        Assert.Equal("Offen", fristen.Single().Titel);
    }

    [Fact]
    public void ZaehleUeberfaellige_GemischteListe_ZaehltNurOffeneMitVergangenerFrist()
    {
        // Arrange
        var service = new DokumentService(new FakeDokumentRepository());
        var gestern = DateTime.Today.AddDays(-1);
        var morgen = DateTime.Today.AddDays(1);
        service.Hinzufuegen("Überfällig", null, null, gestern, FachId);
        service.Hinzufuegen("Noch Zeit", null, null, morgen, FachId);
        service.Hinzufuegen("Ohne Frist", null, null, null, FachId);
        int rechtzeitigAbgegebenId = service.Hinzufuegen("Abgegeben", null, null, gestern, FachId).Id;
        service.MarkiereAlsAbgegeben(rechtzeitigAbgegebenId);
        var dokumente = service.GetByFach(FachId);

        // Act
        int anzahlUeberfaellig = service.ZaehleUeberfaellige(dokumente);
        int anzahlOffen = service.ZaehleOffene(dokumente);

        // Assert
        Assert.Equal(1, anzahlUeberfaellig);
        Assert.Equal(3, anzahlOffen);
    }

    [Fact]
    public void Loeschen_ExistierendesDokument_EntferntEsAusDemRepository()
    {
        // Arrange
        var repository = new FakeDokumentRepository();
        var service = new DokumentService(repository);
        int dokumentId = service.Hinzufuegen("Serie 1", null, null, null, FachId).Id;

        // Act
        service.Loeschen(dokumentId);

        // Assert
        Assert.True(repository.GetById(dokumentId) == null, "Das Dokument ist nach dem Löschen noch vorhanden");
    }
}
