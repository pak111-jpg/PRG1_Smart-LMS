using SchulApp.Dtos;
using SchulApp.Models;

namespace SchulApp.Services;

/// <summary>
/// Bildet ein Fach samt seinen Dokumenten auf ein FachDto ab und erzeugt dabei
/// die Anzeigetexte. Eigene Klasse, damit der FachService nur Regeln und Abläufe
/// enthält (Single Responsibility).
/// </summary>
public static class FachMapper
{
    public const string TextKeineLehrperson = "Keine Lehrperson";
    public const string InitialenOhneName = "?";

    private const int AnzahlInitialen = 2;

    /// <summary>
    /// Die beiden Kennzahlen werden aus der Navigation Property berechnet - das
    /// Fach muss deshalb mit seinen Dokumenten geladen sein.
    /// </summary>
    public static FachDto ZuDto(Fach fach)
    {
        int anzahlDokumente = fach.Dokumente.Count;
        int anzahlOffen = fach.Dokumente.Count(dokument => !dokument.Abgegeben);

        return new FachDto
        {
            Id = fach.Id,
            Name = fach.Name,
            Lehrperson = fach.Lehrperson,
            Initialen = BerechneInitialen(fach.Name),
            LehrpersonAnzeige = string.IsNullOrWhiteSpace(fach.Lehrperson) ? TextKeineLehrperson : fach.Lehrperson,
            AnzahlDokumente = anzahlDokumente,
            AnzahlOffen = anzahlOffen,
            DokumenteAnzeige = Anzeigetexte.AnzahlDokumente(anzahlDokumente),
            OffenAnzeige = anzahlOffen == 0 ? string.Empty : $" · {anzahlOffen} offen"
        };
    }

    /// <summary>
    /// Initialen für den runden Avatar. Bei mehreren Wörtern je der erste Buchstabe
    /// der ersten beiden Wörter ("PRG I" ergibt "PI"), bei einem Wort dessen erste
    /// zwei Buchstaben ("Mathe" ergibt "MA").
    /// </summary>
    public static string BerechneInitialen(string name)
    {
        string bereinigt = name.Trim();
        if (bereinigt.Length == 0) return InitialenOhneName;

        string[] woerter = bereinigt.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (woerter.Length >= AnzahlInitialen)
            return $"{char.ToUpper(woerter[0][0])}{char.ToUpper(woerter[1][0])}";

        return bereinigt.Length >= AnzahlInitialen
            ? bereinigt.Substring(0, AnzahlInitialen).ToUpperInvariant()
            : bereinigt.ToUpperInvariant();
    }
}
