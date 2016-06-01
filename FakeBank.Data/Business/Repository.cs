using System;
using System.Collections.Generic;
using System.Linq;
using FakeBank.Data.Entities;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Data.Entity.Validation;
using System.Data.Entity;

namespace FakeBank.Data.Business
{
    public abstract class Repository<TEntity> where TEntity : class
    {
        protected readonly FAKE_BANKEntities Context;
        protected Repository(FAKE_BANKEntities context)
        {
            Context = context;
        }
        /// <summary>
        /// Inserts an entity in the corresponding entity set.
        /// </summary>
        /// <param name="entity">Entity to be inserted</param>
        public virtual void Insert(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }

        /// <summary>
        /// Deletes an entity from the context.
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Delete(TEntity entity)
        {
            Context.Set<TEntity>().Attach(entity);
            Context.Set<TEntity>().Remove(entity);
        }

        /// <summary>
        /// Gets the whole list of entities of a set.
        /// </summary>
        /// <returns>List of entities of a set.</returns>
        public virtual Task<List<TEntity>> GetAll()
        {
            return Context.Set<TEntity>().ToListAsync();
        }

        /// <summary>
        /// Gets an entity by ids.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public TEntity GetById(params object[] ids)
        {
            return Context.Set<TEntity>().Find(ids);
        }

        /// <summary>
        /// Updates an entity.
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Update(TEntity entity)
        {
            Context.Set<TEntity>().Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// Returns a list of entities according to the parameters given.
        /// </summary>
        /// <param name="filterExpression">Filter the results.</param>
        /// <param name="includeExpressions">Which navigation properties to include, null for none.</param>
        /// <param name="orderBy">How given list will be ordered.</param>
        /// <returns>A list of entities.</returns>
        public List<TEntity> Search(Expression<Func<TEntity, bool>> filterExpression = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            params Expression<Func<TEntity, object>>[] includeExpressions)
        {
            try
            {
                IQueryable<TEntity> query = Context.Set<TEntity>();

                if (filterExpression != null)
                    query = query.Where(filterExpression);

                if (includeExpressions != null)
                {
                    foreach (var includeProperty in includeExpressions)
                    {
                        query = query.Include(includeProperty);
                    }
                }
                try
                {
                    var result = orderBy != null ? orderBy(query).ToList() : query.ToList();
                    return result;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            catch (DbEntityValidationException ex)
            {
                throw new DbEntityValidationException(string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage)));
            }

        }

        /// <summary>
        /// Searches for one entity according to the parameters given.
        /// </summary>
        /// <param name="filterExpression">Expression which reduces the results quantity to one.</param>
        /// <param name="includeExpressions">Which navigation properties need to be included, null for no includes.</param>
        /// <returns></returns>
        public TEntity SearchOne(Expression<Func<TEntity, bool>> filterExpression,params Expression<Func<TEntity, object>>[] includeExpressions)
        {
            IQueryable<TEntity> query = Context.Set<TEntity>();

            if (filterExpression != null)
                query = query.Where(filterExpression);

            if (includeExpressions != null)
            {
                foreach (var includeProperty in includeExpressions)
                {
                    query = query.Include(includeProperty);
                }
            }
            return query.SingleOrDefault();
        }
    }
}
