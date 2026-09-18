using Microsoft.EntityFrameworkCore;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Infrastructure.Persistence;

namespace JobApplicationManagement.Infrastructure.Repositories;

/// <summary>
/// Generic EF Core repository implementation.
///
/// Design decisions:
///   - Add/Update/Delete do NOT call SaveChangesAsync. Persistence is the responsibility
///     of UnitOfWork.SaveChangesAsync().
///   - Query() returns IQueryable for composable database-side queries (pagination, etc.).
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.ToListAsync();

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    /// <inheritdoc/>
    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);

    /// <inheritdoc/>
    public void Update(T entity)
        => _dbSet.Update(entity);

    /// <inheritdoc/>
    public void Delete(T entity)
        => _dbSet.Remove(entity);

    /// <inheritdoc/>
    public IQueryable<T> Query()
        => _dbSet.AsQueryable();
}
