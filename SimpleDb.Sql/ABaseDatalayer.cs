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
    public abstract class ABaseDatalayer<T> where T : ADataObject, new()
    {
        #region constants

        /// <summary>
        /// Database operations.
        /// </summary>
        protected enum DatabaseOperation
        {
            /// <summary>
            /// For Get, GetAll, etc.
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
            Delete
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

            var baseName = TypeInstance.DatabaseTableName;

            StoredProcedureBaseName = "sp" + baseName;
            FunctionBaseName = "fn" + baseName;
        }

        #endregion


        #region properties

        /// <summary>
        /// A Database instance used for all database operations.
        /// </summary>
        public Database Database { get; }

        /// <summary>
        /// An instance of a T, used for reflection operations.
        /// </summary>
        protected T TypeInstance { get; }

        /// <summary>
        /// If true, the AuthorizationManager security is nod used.
        /// False by default (the security is used).
        /// </summary>
        protected virtual bool BypassSecurity => false;

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
        /// Returns all instances of a T.
        /// </summary>
        /// <param name="userDataConsumer">An optional user data consumer instance.</param>
        /// <returns>IEnumerable of all object instances.</returns>
        public virtual IEnumerable<T> GetAll(IDataConsumer<T> userDataConsumer = null)
        {
            OperationAllowed(DatabaseOperation.Select);

            var res = new List<T>();

            var consumer = userDataConsumer ?? new DataConsumer<T>(res);

            Database.ExecuteReader(
                GetSelectStoredProcedureName(),
                CreateSelectListParameters(),
                consumer.CreateInstance,
                null);

            return res;
        }

        /// <summary>
        /// Returns instance to object by id
        /// </summary>
        /// <param name="id">Id value</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        /// <returns>Instance to object</returns>
        public virtual T Get(int id, IDbTransaction transaction = null)
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
        public virtual T Get(int id, IDataConsumer<T> userDataConsumer, IDbTransaction transaction = null)
        {
            if (id <= 0) throw new ArgumentException("A positive number expected.", "id");

            OperationAllowed(DatabaseOperation.Select);

            var res = new List<T>();

            var consumer = userDataConsumer ?? new DataConsumer<T>(res);

            Database.ExecuteReader(
                GetSelectDetailsStoredProcedureName(),
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
        public virtual int Save(T obj, IDbTransaction transaction = null)
        {
            var operation = DatabaseOperation.Insert;

            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var iid = obj as IId;
            if (iid == null)
            {
                OperationAllowed(operation);

                return Database.ExecuteScalar(GetInsertStoredProcedureName(), CreateInsertParameters(obj), transaction);
            }

            if (iid.Id == 0)
            {
                OperationAllowed(operation);

                return iid.Id = Database.ExecuteScalar(GetInsertStoredProcedureName(), CreateInsertParameters(obj), transaction);
            }

            operation = DatabaseOperation.Update;

            OperationAllowed(operation);

            return Database.ExecuteScalar(GetUpdateStoredProcedureName(), CreateUpdateParameters(obj), transaction);
        }

        /// <summary>
        /// Inserts/updates all objects in transaction.
        /// </summary>
        /// <param name="objects">A list of objects.</param>
        /// <param name="transaction">A transaction.</param>
        public virtual void SaveAll(IEnumerable<T> objects, IDbTransaction transaction = null)
        {
            if (objects == null) throw new ArgumentNullException(nameof(objects));

            if (objects.Any() == false) return;

            if (transaction == null)
            {
                Database.DoInTransaction(SaveAllOperation, objects);
            }
            else
            {
                SaveAllOperation(transaction, objects);
            }
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

            Database.ExecuteNonQuery(GetDeleteStoredProcedureName(), CreateIdParameters(obj), transaction);
        }

        /// <summary>
        /// Deletes object from database.
        /// </summary>
        /// <param name="id">An object ID to delete</param>
        /// <param name="transaction">Instance to SqlTransaction object</param>
        public virtual void Delete(int id, IDbTransaction transaction = null)
        {
            if (id <= 0) throw new ArgumentException("An object ID expected.", nameof(id));

            OperationAllowed(DatabaseOperation.Delete);

            var iid = TypeInstance as IId;
            if (iid == null) throw new NotSupportedException("Object does not contain ID");

            Database.ExecuteNonQuery(GetDeleteStoredProcedureName(), CreateIdParameters(iid.Id), transaction);
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
            var iid = obj as IId;
            if (iid == null) throw new NotSupportedException("Object does not contain ID");

            OperationAllowed(DatabaseOperation.Select);

            var consumer = userDataConsumer ?? new DataConsumer<T>(new List<T> { obj });

            Database.ExecuteReader(
                GetSelectDetailsStoredProcedureName(),
                CreateIdParameters(iid.Id),
                consumer.RecreateInstance,
                transaction);
        }

        #endregion


        #region non-public methods

        /// <summary>
        /// Creates parameters for a SELECT database operation. 
        /// </summary>
        /// <returns>A list of SqlParameters.</returns>
        protected virtual DbParameter[] CreateSelectListParameters()
        {
            return null;
        }

        /// <summary>
        /// Creates parameters for a INSERT database operation. 
        /// </summary>
        /// <returns>A list of SqlParameters.</returns>
        protected DbParameter[] CreateInsertParameters(ADataObject instance)
        {
            return CreateInsertOrUpdateParameters(instance, instance is IId);  // TODO: Insert parameters for non IId objects?
        }

        /// <summary>
        /// Creates parameters for a UPDATE database operation. 
        /// </summary>
        /// <returns>A list of SqlParameters.</returns>
        protected DbParameter[] CreateUpdateParameters(ADataObject instance)
        {
            return CreateInsertOrUpdateParameters(instance, false);
        }

        /// <summary>
        /// Creates parameters for an INSERT or an UPDATE database operation. 
        /// </summary>
        /// <returns>A list of SqlParameters.</returns>
        protected virtual DbParameter[] CreateInsertOrUpdateParameters(ADataObject instance, bool insert)
        {
            var paramList = new List<DbParameter>();

            foreach (var column in instance.DatabaseColumns)
            {
                // Get the instance of this column attribute.
                var attribute = ADataObject.GetDbColumnAttribute(column);

                // Skip Id attributes on insert and read only attributes.
                if ((insert && attribute.IsId) || attribute.IsReadOnly)
                {
                    continue;
                }

                // Add parameter to the list of parameters.
                paramList.Add(Database.DatabaseProvider.CreateDbParameter(GetParameterName(attribute.Name), column.GetValue(instance)));
            }

            return paramList.ToArray();
        }

        /// <summary>
        /// Creates parameters for an GET or an DELETE database operation. 
        /// </summary>
        /// <returns>A list of SqlParameters.</returns>
        protected virtual DbParameter[] CreateIdParameters(ADataObject instance)
        {
            var paramList = new List<DbParameter>();

            foreach (var column in instance.DatabaseColumns)
            {
                // Get the instance of this column attribute.
                var attribute = ADataObject.GetDbColumnAttribute(column);

                // Add Id attributes only.
                if (attribute.IsId)
                {
                    // Add parameter to the list of parameters.
                    paramList.Add(Database.DatabaseProvider.CreateDbParameter(GetParameterName(attribute.Name), column.GetValue(instance)));
                }
            }

            return paramList.ToArray();
        }

        /// <summary>
        /// Creates parameters for an GET or an DELETE database operation. 
        /// </summary>
        /// <returns>A list of SqlParameters.</returns>
        protected virtual DbParameter[] CreateIdParameters(int id)
        {
            if (TypeInstance is IId == false || TypeInstance.IsDatabaseTable == false)
            {
                throw new Exception("Can not create an Id parameter from a non IId or non database object.");
            }

            var paramList = new List<DbParameter>();

            foreach (var attribute in TypeInstance.DatabaseColumns.Select(ADataObject.GetDbColumnAttribute).Where(attribute => attribute.IsId))
            {
                // Add parameter to the list of parameters.
                paramList.Add(Database.DatabaseProvider.CreateDbParameter(GetParameterName(attribute.Name), id));

                break;
            }

            return paramList.ToArray();
        }

        /// <summary>
        /// Returns a stored procedure name for an SELECT operation.
        /// </summary>
        /// <returns>A name of a stored procedure.</returns>
        protected virtual string GetSelectStoredProcedureName()
        {
            return StoredProcedureBaseName + "_SelectList";
        }

        /// <summary>
        /// Returns A stored procedure name for an SELECT by ID operation.
        /// </summary>
        /// <returns>A name of a stored procedure.</returns>
        protected virtual string GetSelectDetailsStoredProcedureName()
        {
            return StoredProcedureBaseName + "_SelectDetails";
        }

        /// <summary>
        /// Returns a stored procedure name for an UPDATE operation.
        /// </summary>
        /// <returns>A name of a stored procedure.</returns>
        protected virtual string GetUpdateStoredProcedureName()
        {
            return StoredProcedureBaseName + "_Update";
        }

        /// <summary>
        /// Returns a stored procedure name for an INSERT operation.
        /// </summary>
        /// <returns>A name of a stored procedure.</returns>
        protected virtual string GetInsertStoredProcedureName()
        {
            return StoredProcedureBaseName + "_Insert";
        }

        /// <summary>
        /// Returns stored procedure name for the DELETE operation.
        /// </summary>
        /// <returns>A name of a stored procedure.</returns>
        protected virtual string GetDeleteStoredProcedureName()
        {
            return StoredProcedureBaseName + "_Delete";
        }
        
        /// <summary>
        /// Creates a parameter name from a column name.
        /// </summary>
        /// <param name="columnName">A column name.</param>
        /// <returns>A parameter name.</returns>
        protected string GetParameterName(string columnName)
        {
            return "@" + columnName;
        }

        /// <summary>
        /// The SaveAll operation.
        /// </summary>
        /// <param name="transaction">A SQL transaction.</param>
        /// <param name="data">A list of object to be stored in the database.</param>
        private void SaveAllOperation(IDbTransaction transaction, object data)
        {
            foreach (var obj in (IEnumerable<T>)data)
            {
                try
                {
                    Save(obj, transaction);
                }
                catch (Exception ex)
                {
                    var exception = new DatabaseException("Can not save a data item.", ex);

                    exception.Data.Add(obj.GetType().Name, obj.ToString());

                    throw exception;
                }
            }
        }

        #endregion
    }
}
