using SchulApp.Models;

namespace SchulApp.DataAccess;

/// <summary>
/// Kontrakt für den Datenzugriff auf Fächer: die fünf Standardmethoden aus
/// IRepository plus das, was nur Fächer brauchen.
/// </summary>
public interface IFachRepository : IRepository<Fach>
{
    /// <summary>Alle Fächer alphabetisch, jeweils mit ihren Dokumenten (Navigation Property gefüllt).</summary>
    List<Fach> GetAlleMitDokumenten();

    /// <summary>Prüft, ob ein Fachname bereits vergeben ist (optional eigene Id ausschliessen).</summary>
    bool ExistiertName(string name, int ausserId = 0);
}
