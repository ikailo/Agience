using Agience.Core.Models.Entities.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Agience.Authority.Identity.Data
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// Saves a single entity to the database, ensuring related entities are not modified.
        /// </summary>
        public static async Task<T> SaveEntityAsync<T>(this DbContext dbContext, T entity, bool isNewEntity) where T : BaseEntity, new()
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");

            foreach (var navigation in dbContext.Entry(entity).Navigations)
            {
                if (navigation.CurrentValue != null)
                {
                    navigation.CurrentValue = null; // Clear the navigation property value
                }
            }

            dbContext.Attach(entity);
            dbContext.Entry(entity).State = isNewEntity ? EntityState.Added : EntityState.Modified;

            await dbContext.SaveChangesAsync();

            return entity;
        }


        /// <summary>
        /// Saves multiple entities to the database, ensuring related entities are not modified.
        /// </summary>
        public static async Task<IEnumerable<T>> SaveEntitiesAsync<T>(this DbContext dbContext, IEnumerable<T> entities, bool areNewEntities) where T : BaseEntity, new()
        {
            if (entities == null || !entities.Any())
                throw new ArgumentNullException(nameof(entities), "Entities cannot be null or empty.");

            foreach (var entity in entities)
            {
                foreach (var navigation in dbContext.Entry(entity).Navigations)
                {
                    if (navigation.CurrentValue != null && navigation.EntityEntry.State != EntityState.Detached)
                    {
                        dbContext.Entry(navigation.CurrentValue).State = EntityState.Detached;
                    }
                }

                dbContext.Attach(entity);
                dbContext.Entry(entity).State = areNewEntities ? EntityState.Added : EntityState.Modified;
            }

            await dbContext.SaveChangesAsync();

            return entities;
        }
    }




}
