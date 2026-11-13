namespace SchulApp.DataAccess;

/// <summary>
/// Kontrakt für die Standard-Operationen auf einer beliebigen Entität T.
/// Die Service-Schicht arbeitet nur gegen dieses Interface (bzw. die davon
/// abgeleiteten Interfaces), nie gegen eine konkrete Repository-Klasse.
/// </summary>
public interface IRepository<T>
{
    List<T> GetAll();
    T? GetById(int id);

    /// <summary>Speichert die Entität; ihre neue Id steht danach in der Entität selbst.</summary>
    void Add(T entity);

    void Update(T entity);
    void Delete(int id);
}
