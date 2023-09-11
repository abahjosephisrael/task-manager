using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskManager.Application.Interfaces;
using TaskManager.Infrastructure.Persistence.Contexts;
using TaskManager.Infrastructure.Persistence.Tools;

namespace TaskManager.Infrastructure.Persistence.Repository
{
    public class RepositoryAsync<T> : IRepositoryAsync<T> where T : class
    {
        private readonly AppDbContext _dbContext;
        private readonly DbSet<T> table;
        public RepositoryAsync(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            table = _dbContext.Set<T>();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> GetPagedReponseAsync(int pageNumber, int pageSize)
        {
            return await _dbContext
                .Set<T>()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<List<T>> AddRangeAsync(List<T> entity)
        {
            await _dbContext.Set<T>().AddRangeAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbContext
                 .Set<T>()
                 .ToListAsync();
        }
        public IQueryable<T> GetAllQuery()
        {
            return table.AsQueryable();
        }

        public virtual async Task<T> GetByAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeExpressions)
        {
            IQueryable<T> set = _dbContext.Set<T>();
            foreach (var includeExpression in includeExpressions)
            {
                set = set.Include(includeExpression);
            }
            T result = await set.FirstOrDefaultAsync(predicate);
            return result;
        }

        public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeExpressions)
        {
            IQueryable<T> set = _dbContext.Set<T>();
            foreach (var includeExpression in includeExpressions)
            {
                set = set.Include(includeExpression);
            }
            IReadOnlyList<T> results = await set.AsNoTracking().Where(predicate).ToListAsync();
            return results;
        }

        public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task<int> CountAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).CountAsync();
        }

        private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            return SpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>().AsQueryable(), spec);
        }

    }
}
