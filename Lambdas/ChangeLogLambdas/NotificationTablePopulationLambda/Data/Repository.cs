using Microsoft.EntityFrameworkCore;

namespace NotificationTablePopulationLambda.Data;

public interface IRepository<T> where T : class
{
    IQueryable<T> GetAll();
    T GetById(object id);
    void Insert(T entity);
    void InsertRange(IEnumerable<T> entities);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);
}

public class Repository<T> : IRepository<T> where T : class
{
    private readonly IManageCO2DbContext _context;
    private DbSet<T> _entities;

    public Repository(IManageCO2DbContext context)
    {
        _context = context;
        _entities = _context.Set<T>();
    }

    public IQueryable<T> GetAll()
    {
        return _entities.AsQueryable();
    }

    public void Delete(T entity)
    {
        _entities.Remove(entity);
        _context.SaveChanges();
    }

    public T GetById(object id)
    {
        return _entities.Find(id);
    }

    public void Insert(T entity)
    {
        _entities.Add(entity);
        _context.SaveChanges();
    }

    public void InsertRange(IEnumerable<T> entities)
    {
        _entities.AddRange(entities);
        _context.SaveChanges();
    }

    public void DeleteRange(IEnumerable<T> entities)
    {
        _entities.RemoveRange(entities);
        _context.SaveChanges();
    }
}
