namespace JobApplicationManagement.Application.Common.Interfaces;

/// <summary>
/// Generic repository contract defining common data-access operations.
/// Lives in the Application layer so the Application layer depends on an abstraction,
/// not on Entity Framework or any Infrastructure concern.
/// </summary>
/// <typeparam name="T">The domain entity type.</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>Returns all entities of type T.</summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>Returns the entity with the given primary key, or null if not found.</summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>Stages the entity for insertion. Does NOT call SaveChanges.</summary>
    Task AddAsync(T entity);

    /// <summary>Marks the entity as modified. Does NOT call SaveChanges.</summary>
    void Update(T entity);

    /// <summary>Stages the entity for deletion. Does NOT call SaveChanges.</summary>
    void Delete(T entity);

    /// <summary>
    /// Returns an IQueryable for composing database-side queries (pagination, filtering, projection).
    /// Callers must apply AsNoTracking() for read-only scenarios.
    /// </summary>
    IQueryable<T> Query();
}
