using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using web1.Data;
using web1.Repositories.Interfaces;

namespace web1.Repositories.Implementations
{
    /// <summary>
    /// Generic repository implementation using Entity Framework
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected ApplicationDbContext _context;
        protected DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Get all entities as IQueryable
        /// </summary>
        public virtual IQueryable<T> GetAll()
        {
            return _dbSet.AsQueryable();
        }

        /// <summary>
        /// Get all entities matching predicate
        /// </summary>
        public virtual IQueryable<T> GetAll(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).AsQueryable();
        }

        /// <summary>
        /// Get entity by primary key
        /// </summary>
        public virtual T GetById(object id)
        {
            return _dbSet.Find(id);
        }

        /// <summary>
        /// Get first entity matching predicate
        /// </summary>
        public virtual T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Get all entities matching predicate as list
        /// </summary>
        public virtual IEnumerable<T> Where(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).ToList();
        }

        /// <summary>
        /// Count entities matching predicate
        /// </summary>
        public virtual int Count(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
                return _dbSet.Count();
            return _dbSet.Count(predicate);
        }

        /// <summary>
        /// Add single entity
        /// </summary>
        public virtual void Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _dbSet.Add(entity);
        }

        /// <summary>
        /// Add multiple entities
        /// </summary>
        public virtual void AddRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            _dbSet.AddRange(entities);
        }

        /// <summary>
        /// Update entity
        /// </summary>
        public virtual void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _context.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// Delete single entity
        /// </summary>
        public virtual void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Delete multiple entities
        /// </summary>
        public virtual void DeleteRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            _dbSet.RemoveRange(entities);
        }

        /// <summary>
        /// Detach entity from tracking
        /// </summary>
        public virtual void Detach(T entity)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }

        /// <summary>
        /// Save changes to database
        /// </summary>
        public virtual int SaveChanges()
        {
            return _context.SaveChanges();
        }
    }
}
