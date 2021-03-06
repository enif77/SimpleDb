﻿/* SimpleDb - (C) 2016 - 2019 Premysl Fara 
 
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
    using System.Linq;

    using SimpleDb.Core;
    using System.Reflection;


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
            IdDatabaseColumns = EntityReflector.GetIdDatabaseColumns(TypeInstance);
        }

        #endregion


        #region properties

        /// <summary>
        /// The list of Id database columns this entity has.
        /// </summary>
        protected IEnumerable<PropertyInfo> IdDatabaseColumns { get; }

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
        /// <param name="instanceFactory">An optional IInstanceFactory instance.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        /// <returns>Instance of an entity or null.</returns>
        public virtual T Get(TId id, IInstanceFactory<T> instanceFactory, IDbTransaction transaction = null)
        {
            return GetAll(CreateIdParameters(id), null, instanceFactory, transaction).FirstOrDefault();
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

                var insertParameters = CreateInsertParameters(entity);

                if (UseQueries)
                {
                    return entity.Id = Database.ExecuteScalar<TId>(
                        CommandType.Text,
                        QueryGenerator.GenerateInsertQuery(NamesProvider.GetTableName(TypeInstance.DataTableName), insertParameters, true),
                        insertParameters,
                        transaction);
                }
                else
                {
                    return entity.Id = Database.ExecuteScalar<TId>(
                        CommandType.StoredProcedure,
                        InsertStoredProcedureName,
                        insertParameters,
                        transaction);
                }
            }

            OperationAllowed(DatabaseOperation.Update);

            var updateParameters = CreateUpdateParameters(entity);

            if (UseQueries)
            {
                Database.ExecuteScalar<TId>(
                    CommandType.Text,
                    QueryGenerator.GenerateUpdateQuery(NamesProvider.GetTableName(TypeInstance.DataTableName), updateParameters),
                    updateParameters,
                    transaction);

                return entity.Id;
            }
            else
            {
                Database.ExecuteNonQuery(CommandType.StoredProcedure, UpdateStoredProcedureName, updateParameters, transaction);

                return entity.Id;
            }
        }

        /// <summary>
        /// Deletes an entity from a database.
        /// </summary>
        /// <param name="entity">An entity instance to be deleted.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        public virtual void Delete(T entity, IDbTransaction transaction = null)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            Delete(CreateIdParameters(entity), null, transaction);
        }

        /// <summary>
        /// Deletes an entity from a database.
        /// </summary>
        /// <param name="id">An entity Id to be deleted.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        public virtual void Delete(TId id, IDbTransaction transaction = null)
        {
            OperationAllowed(DatabaseOperation.Delete);

            Delete(CreateIdParameters(id), null, transaction);
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
        /// <param name="instanceFactory">An optional user IInstanceFactory instance.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        public virtual bool Reload(T entity, IInstanceFactory<T> userInstanceFactory = null, IDbTransaction transaction = null)
        {
            OperationAllowed(DatabaseOperation.Select);

            var idParameters = CreateIdParameters(entity);

            IDataRecord record;

            if (UseQueries)
            {
                record = Database.ExecuteReader(
                    CommandType.Text,
                    QueryGenerator.GenerateSelectQuery(NamesProvider.GetTableName(TypeInstance.DataTableName), CreateSelectColumnNames(), CreateWhereClauseExpression(idParameters, null)),  // TODO: SELECT column names can be precomputed.
                    idParameters,
                    transaction).FirstOrDefault();
            }
            else
            {
                record = Database.ExecuteReader(
                    CommandType.StoredProcedure,
                    SelectDetailsStoredProcedureName,
                    idParameters,
                    transaction).FirstOrDefault();
            }

            if (record == null)
            {
                return false;
            }

            var instanceFactory = userInstanceFactory ?? new InstanceFactory<T>(NamesProvider, DatabaseColumns);

            instanceFactory.CreateInstance(record, entity);

            return true;
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
                    paramList.Add(new NamedDbParameter()
                    {
                        BaseName = baseName,
                        Name = NamesProvider.GetColumnName(baseName),
                        IsId = attribute.IsId,
                        DbParameter = UseQueries
                            ? Database.Provider.CreateDbParameter(baseName, column.GetValue(entity))
                            : Database.Provider.CreateStoredProcedureDbParameter(baseName, column.GetValue(entity))
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
                if (column.PropertyType.FullName != typeof(TId).FullName) continue;

                var attribute = EntityReflector.GetDbColumnAttribute(column);
                if (attribute.IsId)
                {
                    // Add parameter to the list of parameters.
                    var baseName = attribute.Name ?? column.Name;
                    paramList.Add(new NamedDbParameter()
                    {
                        BaseName = baseName,
                        Name = NamesProvider.GetColumnName(baseName),
                        IsId = attribute.IsId,
                        DbParameter = UseQueries
                            ? Database.Provider.CreateDbParameter(baseName, id)
                            : Database.Provider.CreateStoredProcedureDbParameter(baseName, id)
                    });

                    // Use the first found property only.
                    break;
                }
            }

            return paramList;
        }

        /// <summary>
        /// Creates Id column parameters for the SELECT database operation. 
        /// These parameters contain names only.
        /// </summary> 
        /// <returns>A list of database parameters.</returns>
        protected virtual IEnumerable<NamedDbParameter> CreateSelectIdColumnNames()
        {
            var columnList = new List<NamedDbParameter>();

            foreach (var column in DatabaseColumns)
            {
                // Get the instance of this column attribute.
                var attribute = EntityReflector.GetDbColumnAttribute(column);

                // Add Id attributes only.
                if (attribute.IsId)
                {
                    // Add column to the list of columns.
                    var baseName = attribute.Name ?? column.Name;
                    columnList.Add(new NamedDbParameter()
                    {
                        BaseName = baseName,
                        Name = NamesProvider.GetColumnName(baseName),
                        IsId = attribute.IsId
                    });
                }
            }

            return columnList;
        }

        #endregion
    }
}
