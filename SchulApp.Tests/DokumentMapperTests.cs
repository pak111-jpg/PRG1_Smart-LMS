using SchulApp.Models;
using SchulApp.Services;

namespace SchulApp.Tests;

/// <summary>
/// K4-Challenge: BerechneRestText hat sechs Fallunterscheidungen - pro Fall ein
/// eigener Test. Dazu die Fälle der Regel "überfällig".
/// </summary>
public class DokumentMapperTests
{
    private static readonly DateTime s_heute = new(2026, 11, 13);

    [Fact]
    public void BerechneRestText_OhneFrist_GibtLeerenTextZurueck()
    {
        // Act
        string restText = DokumentMapper.BerechneRestText(null, s_heute);

        // Assert
        Assert.Equal(string.Empty, restText);
    }

    [Fact]
    public void BerechneRestText_FristVorMehrerenTagen_NenntAnzahlTageUeberfaellig()
    {
        // Act
        string restText = DokumentMapper.BerechneRestText(s_heute.AddDays(-5), s_heute);

        // Assert
        Assert.Equal("5 Tage überfällig", restText);
    }

    [Fact]
    public void BerechneRestText_FristGestern_VerwendetEinzahl()
    {
        // Act
        string restText = DokumentMapper.BerechneRestText(s_heute.AddDays(-1), s_heute);

        // Assert
        Assert.Equal("1 Tag überfällig", restText);
    }

    [Fact]
    public void BerechneRestText_FristHeute_GibtHeuteFaelligZurueck()
    {
        // Act
        string restText = DokumentMapper.BerechneRestText(s_heute, s_heute);

        // Assert
        Assert.Equal("heute fällig", restText);
    }

    [Fact]
    public void BerechneRestText_FristMorgen_GibtMorgenFaelligZurueck()
    {
        // Act
        string restText = DokumentMapper.BerechneRestText(s_heute.AddDays(1), s_heute);

        // Assert
        Assert.Equal("morgen fällig", restText);
    }

    [Fact]
    public void BerechneRestText_FristInMehrerenTagen_NenntAnzahlTage()
    {
        // Act
        string restText = DokumentMapper.BerechneRestText(s_heute.AddDays(3), s_heute);

        // Assert
        Assert.Equal("in 3 Tagen", restText);
    }

    [Fact]
    public void IstUeberfaelligAm_OffenUndFristVorStichtag_GibtTrueZurueck()
    {
        // Arrange
        var dokument = new Dokument { Frist = "2026-11-12", Abgegeben = false };

        // Act
        bool istUeberfaellig = dokument.IstUeberfaelligAm(s_heute);

        // Assert
        Assert.True(istUeberfaellig, "Offenes Dokument mit Frist gestern gilt nicht als überfällig");
    }

    [Fact]
    public void IstUeberfaelligAm_FristAmStichtag_GibtFalseZurueck()
    {
        // Arrange
        var dokument = new Dokument { Frist = "2026-11-13", Abgegeben = false };

        // Act
        bool istUeberfaellig = dokument.IstUeberfaelligAm(s_heute);

        // Assert
        Assert.False(istUeberfaellig, "Am Tag der Frist ist ein Dokument noch nicht überfällig");
    }

    [Fact]
    public void IstUeberfaelligAm_AbgegebenTrotzVergangenerFrist_GibtFalseZurueck()
    {
        // Arrange
        var dokument = new Dokument { Frist = "2026-11-01", Abgegeben = true };

        // Act
        bool istUeberfaellig = dokument.IstUeberfaelligAm(s_heute);

        // Assert
        Assert.False(istUeberfaellig, "Ein abgegebenes Dokument kann nicht überfällig sein");
    }

    [Fact]
    public void IstUeberfaelligAm_OhneFrist_GibtFalseZurueck()
    {
        // Arrange
        var dokument = new Dokument { Frist = null, Abgegeben = false };

        // Act
        bool istUeberfaellig = dokument.IstUeberfaelligAm(s_heute);

        // Assert
        Assert.False(istUeberfaellig, "Ohne Frist kann ein Dokument nicht überfällig sein");
    }

    [Fact]
    public void ZuDto_DokumentMitFachUndDatei_FuelltAnzeigefelder()
    {
        // Arrange
        var dokument = new Dokument
        {
            Id = 7,
            Titel = "Projektdokumentation",
            Typ = DokumentTypen.Projekt,
            Dateipfad = @"C:\Schule\PRG\doku.docx",
            Frist = "2026-11-17",
            Abgegeben = false,
            Fach = new Fach { Name = "PRG I" }
        };

        // Act
        var dto = DokumentMapper.ZuDto(dokument, s_heute);

        // Assert
        Assert.Equal(7, dto.Id);
        Assert.Equal("PRG I", dto.FachName);
        Assert.Equal("doku.docx", dto.DateiAnzeige);
        Assert.Equal("17.11.2026", dto.FristAnzeige);
        Assert.Equal("in 4 Tagen", dto.RestText);
        Assert.Equal(DokumentMapper.TextOffen, dto.StatusText);
        Assert.Equal(DokumentMapper.ZeichenOffen, dto.StatusZeichen);
        Assert.True(dto.HatFrist, "Die gesetzte Frist wurde nicht erkannt");
    }

    [Fact]
    public void ZuDto_DokumentOhneFristUndDatei_VerwendetPlatzhalter()
    {
        // Arrange
        var dokument = new Dokument { Titel = "Notizen", Abgegeben = true };

        // Act
        var dto = DokumentMapper.ZuDto(dokument, s_heute);

        // Assert
        Assert.Equal(Anzeigetexte.KeinWert, dto.DateiAnzeige);
        Assert.Equal(Anzeigetexte.KeinWert, dto.FristAnzeige);
        Assert.Equal(string.Empty, dto.FachName);
        Assert.Equal(DokumentMapper.TextAbgegeben, dto.StatusText);
        Assert.False(dto.HatFrist, "Ohne Frist darf HatFrist nicht gesetzt sein");
    }
}
