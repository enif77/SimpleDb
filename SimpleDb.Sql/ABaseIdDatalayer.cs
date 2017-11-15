/* SimpleDb - (C) 2016 - 2017 Premysl Fara 
 
SimpleDb is available under the zlib license:

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not
   claim that you wrote the original software. If you use this software
   in a product, an acknowledgment in the product documentation would be
   appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
   misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
 
 */

namespace SimpleDb.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;

    using SimpleDb.Shared;


    /// <summary>
    /// The base of all datalayers for entities with an Id column.
    /// </summary>
    /// <typeparam name="T">An ABusinessObject type.</typeparam>
    /// <typeparam name="TId">A type of the entity Id column (int, long, GUID, ...).</typeparam>
    public abstract class ABaseIdDatalayer<T, TId> : ABaseDatalayer<T> where T : AIdEntity<TId>, new()
    {
        #region ctor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="database">An initialised Database instance to be used for all database operations.</param>
        protected ABaseIdDatalayer(Database database)
            : base(database)
        {
        }

        #endregion


        #region public methods

        /// <summary>
        /// Returns an entity instance with a specific Id.
        /// </summary>
        /// <param name="id">An entity Id.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        /// <returns>Instance of an entity or null.</returns>
        public virtual T Get(TId id, IDbTransaction transaction = null)
        {
            return Get(id, null, transaction);
        }

        /// <summary>
        /// Returns an entity instance with a specific Id.
        /// </summary>
        /// <param name="id">An entity Id.</param>
        /// <param name="dataConsumer">An optional data consumer instance.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        /// <returns>Instance of an entity or null.</returns>
        public virtual T Get(TId id, IDataConsumer<T> dataConsumer, IDbTransaction transaction = null)
        {
            OperationAllowed(DatabaseOperation.Select);

            var res = new List<T>();

            var consumer = dataConsumer ?? new DataConsumer<T>(NamesProvider, DatabaseColumns, res);

            Database.ExecuteReader(
                SelectDetailsStoredProcedureName,
                CreateIdParameters(id),
                consumer.CreateInstance,
                transaction);

            return res.FirstOrDefault();
        }

        /// <summary>
        /// Inserts or updates entity in database.
        /// </summary>
        /// <param name="entity">An entity instance to be saved.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        /// <returns>An Id of the saved instance.</returns>
        new public virtual TId Save(T entity, IDbTransaction transaction = null)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (entity.IsNew)
            {
                OperationAllowed(DatabaseOperation.Insert);

                return entity.Id = (TId)Database.ExecuteScalarObject(InsertStoredProcedureName, CreateInsertParameters(entity), transaction);
            }

            OperationAllowed(DatabaseOperation.Update);

            return (TId)Database.ExecuteScalarObject(UpdateStoredProcedureName, CreateUpdateParameters(entity), transaction);
        }

        /// <summary>
        /// Deletes an entity from a database.
        /// </summary>
        /// <param name="entity">An entity instance to be deleted.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        public virtual void Delete(T entity, IDbTransaction transaction = null)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            OperationAllowed(DatabaseOperation.Delete);

            Database.ExecuteNonQuery(DeleteStoredProcedureName, CreateIdParameters(entity), transaction);
        }

        /// <summary>
        /// Deletes an entity from a database.
        /// </summary>
        /// <param name="id">An entity Id to be deleted.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        public virtual void Delete(TId id, IDbTransaction transaction = null)
        {
            OperationAllowed(DatabaseOperation.Delete);

            Database.ExecuteNonQuery(DeleteStoredProcedureName, CreateIdParameters(id), transaction);
        }

        /// <summary>
        /// Reloads an entity from a database.
        /// </summary>
        /// <param name="entity">An entity instance to be reloaded from a database.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        public virtual void Reload(T entity, IDbTransaction transaction = null)
        {
            Reload(entity, null, transaction);
        }

        /// <summary>
        /// Reloads an entity from the database.
        /// </summary>
        /// <param name="entity">An entity instance to be reloaded from a database.</param>
        /// <param name="dataConsumer">An optional user data consumer instance.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        public virtual void Reload(T entity, IDataConsumer<T> dataConsumer, IDbTransaction transaction = null)
        {
            OperationAllowed(DatabaseOperation.Select);

            var consumer = dataConsumer ?? new DataConsumer<T>(NamesProvider, DatabaseColumns, new List<T> { entity });

            Database.ExecuteReader(
                SelectDetailsStoredProcedureName,
                CreateIdParameters(entity),
                consumer.RecreateInstance,
                transaction);
        }

        #endregion


        #region non-public methods

        /// <summary>
        /// Creates parameters for the GET or the DELETE database operation. 
        /// </summary>
        /// <param name="entity">An entity instance from which we want to get Id parameters.</param>
        /// <returns>A list of database parameters.</returns>
        protected virtual IEnumerable<NamedDbParameter> CreateIdParameters(AEntity entity)
        {
            var paramList = new List<NamedDbParameter>();

            foreach (var column in DatabaseColumns)
            {
                // Get the instance of this column attribute.
                var attribute = EntityReflector.GetDbColumnAttribute(column);

                // Add Id attributes only.
                if (attribute.IsId)
                {
                    // Add parameter to the list of parameters.
                    var baseName = attribute.Name ?? column.Name;
                    var translatedName = NamesProvider.TranslateColumnName(baseName);
                    paramList.Add(new NamedDbParameter()
                    {
                        BaseName = baseName,
                        Name = translatedName,
                        DbParameter = Database.Provider.CreateDbParameter(translatedName, column.GetValue(entity), false)
                    });
                }
            }

            return paramList;
        }

        /// <summary>
        /// Creates an Id parameter for the GET or the DELETE database operation. 
        /// </summary>
        /// <param name="id">An entity Id.</param>
        /// <returns>A list of database parameters.</returns>
        protected virtual IEnumerable<NamedDbParameter> CreateIdParameters(TId id)
        {
            var paramList = new List<NamedDbParameter>();

            foreach (var column in DatabaseColumns)
            {
                // Ignore column with a different type.
                if (column.GetType() != typeof(TId)) continue;

                var attribute = EntityReflector.GetDbColumnAttribute(column);
                if (attribute.IsId)
                {
                    // Add parameter to the list of parameters.
                    var baseName = attribute.Name ?? column.Name;
                    var translatedName = NamesProvider.TranslateColumnName(baseName);
                    paramList.Add(new NamedDbParameter()
                    {
                        BaseName = baseName,
                        Name = translatedName,
                        DbParameter = Database.Provider.CreateDbParameter(translatedName, id, false)
                    });

                    // Use the first found property only.
                    break;
                }
            }

            return paramList;
        }

        #endregion
    }
}
