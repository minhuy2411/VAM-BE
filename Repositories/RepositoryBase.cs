using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VAM.Data;
using VAM.Entities;

namespace VAM.Repositories
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public RepositoryBase(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetAllPaginatedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            int totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null && !entity.IsDeleted) return entity;
            return null;
        }


        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task CreateAsync(T entity)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            await _dbSet.AddAsync(entity);
        }

        public async Task CreateManyAsync(IEnumerable<T> entities)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var e in entities) e.CreatedAt = now;
            await _dbSet.AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            _dbSet.Update(entity);
        }

        public void UpdateMany(IEnumerable<T> entities)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var e in entities) e.UpdatedAt = now;
            _dbSet.UpdateRange(entities);
        }

        public void Delete(T entity)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            _dbSet.Update(entity);
        }

        public void DeleteMany(IEnumerable<T> entities)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var e in entities) 
            {
                e.IsDeleted = true;
                e.UpdatedAt = now;
            }
            _dbSet.UpdateRange(entities);
        }
    }
}
