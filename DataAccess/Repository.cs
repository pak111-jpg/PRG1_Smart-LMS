using Microsoft.EntityFrameworkCore;

namespace SchulApp.DataAccess;

/// <summary>
/// Generische Basisklasse (K3-Vertiefung): setzt die fünf Standardmethoden von
/// IRepository einmal mit EF Core um. Die konkreten Repository-Klassen erben
/// davon und ergänzen nur, was für ihre Entität speziell ist.
/// Pro Methode ein eigener, kurzlebiger Context.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    public virtual List<T> GetAll()
    {
        using var context = new AppDbContext();
        return context.Set<T>().ToList();
    }

    public virtual T? GetById(int id)
    {
        using var context = new AppDbContext();
        return context.Set<T>().Find(id);
    }

    public virtual void Add(T entity)
    {
        using var context = new AppDbContext();
        context.Set<T>().Add(entity);

        // EF Core trägt beim Speichern die neue Id direkt in die Entität ein.
        context.SaveChanges();
    }

    public virtual void Update(T entity)
    {
        using var context = new AppDbContext();

        // Nur diese eine Entität als geändert markieren - mitgeladene Navigation
        // Properties (z. B. Dokument.Fach) werden nicht mitgespeichert.
        context.Entry(entity).State = EntityState.Modified;
        context.SaveChanges();
    }

    public virtual void Delete(int id)
    {
        using var context = new AppDbContext();
        var entity = context.Set<T>().Find(id);
        if (entity == null) return;

        context.Set<T>().Remove(entity);
        context.SaveChanges();
    }
}
