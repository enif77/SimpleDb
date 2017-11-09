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
    /// The base of all datalayers.
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
        /// Returns instance to object by id
        /// </summary>
        /// <param name="id">Id value</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        /// <returns>Instance to object</returns>
        public virtual T Get(TId id, IDbTransaction transaction = null)
        {
            return Get(id, null, transaction);
        }

        /// <summary>
        /// Returns instance to object by id
        /// </summary>
        /// <param name="id">Id value</param>
        /// <param name="userDataConsumer">An optional user data consumer instance.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        /// <returns>Instance to object</returns>
        public virtual T Get(TId id, IDataConsumer<T> userDataConsumer, IDbTransaction transaction = null)
        {
            OperationAllowed(DatabaseOperation.Select);

            var res = new List<T>();

            var consumer = userDataConsumer ?? new DataConsumer<T>(NamesProvider, res);

            Database.ExecuteReader(
                SelectDetailsStoredProcedureName,
                CreateIdParameters(id),
                consumer.CreateInstance,
                transaction);

            return res.FirstOrDefault();
        }

        /// <summary>
        /// Inserts/updates object in database
        /// </summary>
        /// <param name="obj">Instance to save</param>
        /// <param name="transaction">Instance to SqlTransaction object</param>
        /// <returns>Id of saved instance or the number of modified rows for non IId instances.</returns>
        public new TId Save(T obj, IDbTransaction transaction = null)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            if (obj.IsNew)
            {
                OperationAllowed(DatabaseOperation.Insert);

                return obj.Id = (TId)Database.ExecuteScalarObject(InsertStoredProcedureName, CreateInsertParameters(obj), transaction);
            }

            OperationAllowed(DatabaseOperation.Update);

            return (TId)Database.ExecuteScalarObject(UpdateStoredProcedureName, CreateUpdateParameters(obj), transaction);
        }

        /// <summary>
        /// Deletes object from database
        /// </summary>
        /// <param name="obj">Instance to delete</param>
        /// <param name="transaction">Instance to SqlTransaction object</param>
        public virtual void Delete(T obj, IDbTransaction transaction = null)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            OperationAllowed(DatabaseOperation.Delete);

            Database.ExecuteNonQuery(DeleteStoredProcedureName, CreateIdParameters(obj), transaction);
        }

        /// <summary>
        /// Deletes object from database.
        /// </summary>
        /// <param name="id">An object Id to delete</param>
        /// <param name="transaction">Instance to SqlTransaction object</param>
        public virtual void Delete(TId id, IDbTransaction transaction = null)
        {
            OperationAllowed(DatabaseOperation.Delete);

            Database.ExecuteNonQuery(DeleteStoredProcedureName, CreateIdParameters(id), transaction);
        }

        /// <summary>
        /// Reloads an object from the database.
        /// </summary>
        /// <param name="obj">An object instance to be reloaded from a database.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        public virtual void Reload(T obj, IDbTransaction transaction = null)
        {
            Reload(obj, null, transaction);
        }

        /// <summary>
        /// Reloads an object from the database.
        /// </summary>
        /// <param name="obj">An object instance to be reloaded from a database.</param>
        /// <param name="userDataConsumer">An optional user data consumer instance.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        public virtual void Reload(T obj, IDataConsumer<T> userDataConsumer, IDbTransaction transaction = null)
        {
            var iid = obj as IIdEntity<TId>;
            if (iid == null) throw new NotSupportedException("An entity without the Id column can not be reloaded.");

            OperationAllowed(DatabaseOperation.Select);

            var consumer = userDataConsumer ?? new DataConsumer<T>(NamesProvider, new List<T> { obj });

            Database.ExecuteReader(
                SelectDetailsStoredProcedureName,
                CreateIdParameters(obj),
                consumer.RecreateInstance,
                transaction);
        }

        #endregion


        #region non-public methods

        /// <summary>
        /// Creates parameters for an GET or an DELETE database operation. 
        /// </summary>
        /// <returns>A list of SqlParameters.</returns>
        protected virtual DbParameter[] CreateIdParameters(AEntity instance)
        {
            var paramList = new List<DbParameter>();

            foreach (var column in instance.DatabaseColumns)
            {
                // Get the instance of this column attribute.
                var attribute = EntityReflector.GetDbColumnAttribute(column);

                // Add Id attributes only.
                if (attribute.IsId)
                {
                    // Add parameter to the list of parameters.
                    paramList.Add(Database.Provider.CreateDbParameter(attribute.Name ?? column.Name, column.GetValue(instance)));
                }
            }

            return paramList.ToArray();
        }

        /// <summary>
        /// Creates parameters for an GET or an DELETE database operation. 
        /// </summary>
        /// <param name="id">An entity Id.</param>
        /// <returns>A list of SqlParameters.</returns>
        protected virtual DbParameter[] CreateIdParameters(TId id)
        {
            var paramList = new List<DbParameter>();

            foreach (var column in EntityReflector.GetDatabaseColumns(TypeInstance))
            {
                // Ignore column with a different type.
                if (column.GetType() != typeof(TId)) continue;

                var attribute = EntityReflector.GetDbColumnAttribute(column);
                if (attribute.IsId)
                {
                    paramList.Add(Database.Provider.CreateDbParameter(attribute.Name ?? column.Name, id));

                    // Use the first found property only.
                    break;
                }
            }

            return paramList.ToArray();
        }

        #endregion
    }
}
