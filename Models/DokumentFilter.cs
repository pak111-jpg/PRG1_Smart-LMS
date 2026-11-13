namespace SchulApp.Models;

/// <summary>Statusfilter für die Gesamtübersicht aller Dokumente.</summary>
public enum DokumentFilter
{
    Alle,
    Offen,
    Abgegeben,
    Ueberfaellig
}

/// <summary>Sortierung für die Gesamtübersicht aller Dokumente.</summary>
public enum DokumentSortierung
{
    Frist,
    Titel,
    Fach
}
