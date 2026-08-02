using System.Linq.Expressions;
using HR_PAYROLL_V2.Domain.Interfaces;
using HR_PAYROLL_V2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR_PAYROLL_V2.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id) => await DbSet.FindAsync(id);

    public async Task<IReadOnlyList<T>> GetAllAsync() => await DbSet.ToListAsync();

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await DbSet.Where(predicate).ToListAsync();

    public IQueryable<T> Query() => DbSet.AsQueryable();

    public async Task AddAsync(T entity) => await DbSet.AddAsync(entity);

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) => await DbSet.AnyAsync(predicate);
}
