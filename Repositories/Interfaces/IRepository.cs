using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace web1.Repositories.Interfaces
{
    /// <summary>
    /// Generic repository interface for data access
    /// </summary>
    public interface IRepository<T> where T : class
    {
        // Read operations
        IQueryable<T> GetAll();
        IQueryable<T> GetAll(Expression<Func<T, bool>> predicate);
        T GetById(object id);
        T FirstOrDefault(Expression<Func<T, bool>> predicate);
        IEnumerable<T> Where(Expression<Func<T, bool>> predicate);
        int Count(Expression<Func<T, bool>> predicate = null);

        // Write operations
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);

        // Batch operations
        void Detach(T entity);

        // Commit
        int SaveChanges();
    }
}
