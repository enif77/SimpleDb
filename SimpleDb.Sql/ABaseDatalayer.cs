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
    using System.Reflection;

    using SimpleDb.Core;
    using SimpleDb.Sql.Expressions;
    using SimpleDb.Sql.Expressions.Operators;


    /// <summary>
    /// The base of all datalayers.
    /// </summary>
    /// <typeparam name="T">An ABusinessObject type.</typeparam>
    public abstract class ABaseDatalayer<T> where T : AEntity, new()
    {
        #region constants

        /// <summary>
        /// Database operations.
        /// </summary>
        protected enum DatabaseOperation
        {
            /// <summary>
            /// For Get, GetAll, GetIdByName, etc.
            /// </summary>
            Select,

            /// <summary>
            /// For Save and SaveAll, when a new object is saved into a database.
            /// </summary>
            Insert,

            /// <summary>
            /// For Save and SaveAll, when an existing object is saved into a database.
            /// </summary>
            Update,

            /// <summary>
            /// For Delete. 
            /// </summary>
            Delete,
        }

        #endregion


        #region ctor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="database">An initialised Database instance to be used for all database operations.</param>
        protected ABaseDatalayer(Database database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            TypeInstance = new T();

            var baseName = EntityReflector.GetDatabaseTableName(TypeInstance);

            StoredProcedureBaseName = NamesProvider.GetStoredProcedureBaseName(baseName);
            FunctionBaseName = NamesProvider.GetFunctionBaseName(baseName);

            DatabaseColumns = EntityReflector.GetDatabaseColumns(TypeInstance);
        }

        #endregion


        #region properties

        /// <summary>
        /// A Database instance used for all database operations.
        /// </summary>
        public Database Database { get; }
        
        /// <summary>
        /// An INamesProvider instance used by this DALs database.
        /// </summary>
        public INamesProvider NamesProvider { get { return Database.Provider.NamesProvider; } }

        /// <summary>
        /// An QueryGenerator instance used by this DALs database.
        /// </summary>
        public IQueryGenerator QueryGenerator { get { return Database.Provider.QueryGenerator; } }

        /// <summary>
        /// If true, all database operations are done by SQL queries and not by stored procedures.
        /// </summary>
        public bool UseQueries { get; set; }

        /// <summary>
        /// An instance of a T, used for reflection operations.
        /// </summary>
        protected T TypeInstance { get; }

        /// <summary>
        /// The list of database columns this entity has.
        /// </summary>
        protected IEnumerable<PropertyInfo> DatabaseColumns { get; }

        /// <summary>
        /// If true, the security is not used.
        /// True by default (the security is not used).
        /// </summary>
        protected virtual bool BypassSecurity => true;

        /// <summary>
        /// Gets a base of a stored procedure name.
        /// </summary>
        protected virtual string StoredProcedureBaseName { get; }

        /// <summary>
        /// Gets a base of a function name.
        /// </summary>
        protected virtual string FunctionBaseName { get; }

        #endregion


        #region public methods

        /// <summary>
        /// Returns all instances of a T.
        /// </summary>
        /// <param name="parameters">A list of parameters for the SELECT operation. 
        /// If set and UseQueries is false, the stored procedure should have all parameters, which were supplied here.</param>
        /// <param name="expression">An optional user defined WHERE clause expression for the SELECT command. 
        /// If not set and some parameters are defined, the WHERE clause is generated as column = param AND ...</param>
        /// <param name="instanceFactory">An optional user IInstanceFactory instance.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        /// <returns>IEnumerable of all object instances.</returns>
        public virtual IEnumerable<T> GetAll(IEnumerable<NamedDbParameter> parameters = null, Expression expression = null, IInstanceFactory<T> instanceFactory = null, IDbTransaction transaction = null)
        {
            OperationAllowed(DatabaseOperation.Select);

            if (UseQueries)
            {
                return Database.ExecuteReader(
                    instanceFactory ?? new InstanceFactory<T>(NamesProvider, DatabaseColumns),
                    CommandType.Text,
                    QueryGenerator.GenerateSelectQuery(NamesProvider.GetTableName(TypeInstance.DataTableName), CreateSelectColumnNames(), CreateWhereClauseExpression(parameters, expression)),
                    parameters,
                    transaction);
            }
            else
            {
                if (expression != null) throw new InvalidOperationException("WHERE clause expression not supported for stored procedures.");

                return Database.ExecuteReader(
                    instanceFactory ?? new InstanceFactory<T>(NamesProvider, DatabaseColumns),
                    CommandType.StoredProcedure,
                    SelectStoredProcedureName,
                    parameters,
                    transaction);
            }
        }

        /// <summary>
        /// Inserts an entity into a database.
        /// </summary>
        /// <param name="entity">An entity instance to be saved.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        public virtual void Save(T entity, IDbTransaction transaction = null)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            OperationAllowed(DatabaseOperation.Insert);

            var parameters = CreateInsertParameters(entity);

            if (UseQueries)
            {
                Database.ExecuteNonQuery(CommandType.Text, QueryGenerator.GenerateInsertQuery(NamesProvider.GetTableName(TypeInstance.DataTableName), parameters, false), parameters, transaction);
            }
            else
            {
                Database.ExecuteNonQuery(CommandType.StoredProcedure, InsertStoredProcedureName, parameters, transaction);
            }
        }

        /// <summary>
        /// Inserts all entities into a database.
        /// </summary>
        /// <param name="entities">A list of entities.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        public virtual void SaveAll(IEnumerable<T> entities, IDbTransaction transaction = null)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            if (entities.Any())
            {
                if (transaction == null)
                {
                    Database.DoInTransaction(SaveAllOperation, entities);
                }
                else
                {
                    SaveAllOperation(transaction, entities);
                }
            }
        }

        /// <summary>
        /// Deletes all entities from a database that match given parameters.
        /// </summary>
        /// <param name="parameters">A list of parameters for the WHERE calusule of the DELETE operation.
        /// If set and UseQueries is false, the stored procedure should have all parameters, which were supplied here.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        public virtual void Delete(IEnumerable<NamedDbParameter> parameters = null, Expression expression = null, IDbTransaction transaction = null)
        {
            OperationAllowed(DatabaseOperation.Delete);

            if (UseQueries)
            {
                Database.ExecuteNonQuery(
                    CommandType.Text,
                    QueryGenerator.GenerateDeleteQuery(NamesProvider.GetTableName(TypeInstance.DataTableName), CreateWhereClauseExpression(parameters, expression)),
                    parameters,
                    transaction);
            }
            else
            {
                if (expression != null) throw new InvalidOperationException("WHERE clause expression not supported for stored procedures.");

                Database.ExecuteNonQuery(CommandType.StoredProcedure, DeleteStoredProcedureName, parameters, transaction);
            }
        }

        #endregion


        #region non-public methods

        /// <summary>
        /// Returns a security function code for a database operation.
        /// </summary>
        /// <param name="operation">A database opertation.</param>
        /// <returns>A security function code.</returns>
        protected virtual string GetSecurityFunctionCode(DatabaseOperation operation)
        {
            return operation.ToString();
        }

        /// <summary>
        /// Checks, if an operation is allowed for a certain user.
        /// </summary>
        /// <param name="operation">A DatabaseOperation code.</param>
        protected void OperationAllowed(DatabaseOperation operation)
        {
            if (BypassSecurity) return;

            OperationAllowedImplementation(operation);
        }

        /// <summary>
        /// Checks, if an operation is allowed for a certain user - implementation.
        /// </summary>
        /// <param name="operation">A DatabaseOperation code.</param>
        protected virtual void OperationAllowedImplementation(DatabaseOperation operation)
        {
            // Here can be a user created code, that does checks against the database.
        }

        /// <summary>
        /// Creates parameters for the INSERT database operation. 
        /// </summary>
        /// <param name="entity">An entity instance.</param>
        /// <returns>A list of database parameters.</returns>
        protected IEnumerable<NamedDbParameter> CreateInsertParameters(AEntity entity)
        {
            return CreateInsertOrUpdateParameters(entity, true);
        }

        /// <summary>
        /// Creates parameters for the UPDATE database operation. 
        /// </summary>
        /// <param name="entity">An entity instance.</param>
        /// <returns>A list of database parameters.</returns>
        protected IEnumerable<NamedDbParameter> CreateUpdateParameters(AEntity entity)
        {
            return CreateInsertOrUpdateParameters(entity, false);
        }

        /// <summary>
        /// Creates parameters for the INSERT or the UPDATE database operation. 
        /// </summary>
        /// <param name="entity">An entity instance.</param>
        /// <param name="insert">If true, we are inserting, so the Id columns are not generated into the list of database parameters.</param>
        /// <returns>A list of database parameters.</returns>
        protected virtual IEnumerable<NamedDbParameter> CreateInsertOrUpdateParameters(AEntity entity, bool insert)
        {
            var paramList = new List<NamedDbParameter>();

            foreach (var column in DatabaseColumns)
            {
                // Get the instance of this column attribute.
                var attribute = EntityReflector.GetDbColumnAttribute(column);

                // Skip Id attributes on insert and read only attributes allways.
                if ((insert && attribute.IsId) || attribute.IsReadOnly)
                {
                    continue;
                }

                // Add parameter to the list of parameters.
                var baseName = attribute.Name ?? column.Name;
                paramList.Add(new NamedDbParameter()
                {
                    BaseName = baseName,
                    Name = NamesProvider.GetColumnName(baseName),
                    IsId = attribute.IsId,
                    DbParameter = UseQueries
                        ? Database.Provider.CreateDbParameter(baseName, column.GetValue(entity) ?? DBNull.Value)
                        : Database.Provider.CreateStoredProcedureDbParameter(baseName, column.GetValue(entity) ?? DBNull.Value)
                });
            }

            return paramList;
        }

        /// <summary>
        /// Creates a WHERE clause expression from a list of parameters.
        /// </summary>
        /// <param name="parameters">An optional list of parameters for column = param [ AND column = param ] expression.</param>
        /// <param name="expression">An optional user defined expression.</param>
        /// <returns>An WHERE clause expression.</returns>
        protected Expression CreateWhereClauseExpression(IEnumerable<NamedDbParameter> parameters, Expression expression = null)
        {
            var whereExpression = expression;

            if (parameters != null && parameters.Any() && expression == null)
            {
                var paramExpressions = new List<Expression>();
                foreach (var idParam in parameters)
                {
                    paramExpressions.Add(Expression.ParameterExpression(new EqualOperator(), idParam));
                }

                if (paramExpressions.Count > 1)
                {
                    whereExpression = (Expression)Expression.And(paramExpressions);
                }
                else
                {
                    whereExpression = paramExpressions.First();
                }
            }

            return whereExpression;
        }
        
        /// <summary>
        /// Creates column parameters for the SELECT database operation. 
        /// These parameters contain names only.
        /// </summary> 
        /// <returns>A list of database parameters.</returns>
        protected virtual IEnumerable<NamedDbParameter> CreateSelectColumnNames()
        {
            var columnList = new List<NamedDbParameter>();

            foreach (var column in DatabaseColumns)
            {
                // Get the instance of this column attribute.
                var attribute = EntityReflector.GetDbColumnAttribute(column);

                // Add column to the list of columns.
                var baseName = attribute.Name ?? column.Name;
                columnList.Add(new NamedDbParameter()
                {
                    BaseName = baseName,
                    Name = NamesProvider.GetColumnName(baseName)
                });
            }

            return columnList;
        }
        
        /// <summary>
        /// A select all to a list stored procedure name.
        /// </summary>
        protected string SelectStoredProcedureName
        {
            get { return NamesProvider.GetSelectStoredProcedureName(StoredProcedureBaseName); }
        }

        /// <summary>
        /// A select single by ID stored procedure name.
        /// </summary>
        protected string SelectDetailsStoredProcedureName
        {
            get { return NamesProvider.GetSelectDetailsStoredProcedureName(StoredProcedureBaseName); }
        }

        /// <summary>
        /// An insert stored procedure name.
        /// </summary>
        protected string InsertStoredProcedureName
        {
            get { return NamesProvider.GetInsertStoredProcedureName(StoredProcedureBaseName); }
        }

        /// <summary>
        /// An update stored procedure name.
        /// </summary>
        protected string UpdateStoredProcedureName
        {
            get { return NamesProvider.GetUpdateStoredProcedureName(StoredProcedureBaseName); }
        }

        /// <summary>
        /// A delete stored procedure name.
        /// </summary>
        protected string DeleteStoredProcedureName
        {
            get { return NamesProvider.GetDeleteStoredProcedureName(StoredProcedureBaseName); }
        }

        /// <summary>
        /// The save-all operation implementation.
        /// </summary>
        /// <param name="transaction">A SQL transaction.</param>
        /// <param name="data">A list of objects to be stored in the database.</param>
        protected void SaveAllOperation(IDbTransaction transaction, object data)
        {
            foreach (var entity in (IEnumerable<T>)data)
            {
                try
                {
                    Save(entity, transaction);
                }
                catch (Exception ex)
                {
                    var exception = new DatabaseException("Can not save an entity.", ex);

                    exception.Data.Add(entity.GetType().Name, entity);

                    throw exception;
                }
            }
        }

        #endregion
    }
}
