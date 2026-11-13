using SchulApp.Models;

namespace SchulApp.DataAccess;

/// <summary>
/// Kontrakt für den Datenzugriff auf Dokumente: die fünf Standardmethoden aus
/// IRepository plus die Abfragen und der Workflow, die nur Dokumente brauchen.
/// </summary>
public interface IDokumentRepository : IRepository<Dokument>
{
    /// <summary>Alle Dokumente eines Fachs: offene zuerst, dann nach Frist, dann nach Titel.</summary>
    List<Dokument> GetByFach(int fachId);

    /// <summary>Alle offenen Dokumente mit Frist, aufsteigend nach Frist, inkl. Fach.</summary>
    List<Dokument> GetOffeneFristen();

    /// <summary>Alle Dokumente über alle Fächer inkl. Fach - gesucht, gefiltert und sortiert.</summary>
    List<Dokument> Suche(string suchtext, DokumentFilter filter, DokumentSortierung sortierung);

    /// <summary>Kern des Workflows: setzt den Abgabestatus eines Dokuments.</summary>
    void SetAbgegeben(int id, bool abgegeben);
}
