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
        /// An instance of a T, used for reflection operations.
        /// </summary>
        protected T TypeInstance { get; }

        /// <summary>
        /// If true, the AuthorizationManager security is nod used.
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

            var consumer = userDataConsumer ?? new DataConsumer<T>(NamesProvider, res);

            Database.ExecuteReader(
                SelectStoredProcedureName,
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
            if (id <= 0) throw new ArgumentException("A positive number expected.", nameof(id));

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
        public virtual int Save(T obj, IDbTransaction transaction = null)
        {
            var operation = DatabaseOperation.Insert;

            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var iid = obj as IIdEntity<int>;
            if (iid == null)
            {
                OperationAllowed(operation);

                return Database.ExecuteScalar(InsertStoredProcedureName, CreateInsertParameters(obj), transaction);
            }

            if (iid.Id == 0)
            {
                OperationAllowed(operation);

                return iid.Id = Database.ExecuteScalar(InsertStoredProcedureName, CreateInsertParameters(obj), transaction);
            }

            operation = DatabaseOperation.Update;

            OperationAllowed(operation);

            return Database.ExecuteScalar(UpdateStoredProcedureName, CreateUpdateParameters(obj), transaction);
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

            Database.ExecuteNonQuery(DeleteStoredProcedureName, CreateIdParameters(obj), transaction);
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

            var iid = TypeInstance as IIdEntity<int>;
            if (iid == null) throw new NotSupportedException("Object does not contain ID");

            Database.ExecuteNonQuery(DeleteStoredProcedureName, CreateIdParameters(iid.Id), transaction);
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
            var iid = obj as IIdEntity<int>;
            if (iid == null) throw new NotSupportedException("Object does not contain ID");

            OperationAllowed(DatabaseOperation.Select);

            var consumer = userDataConsumer ?? new DataConsumer<T>(NamesProvider, new List<T> { obj });

            Database.ExecuteReader(
                SelectDetailsStoredProcedureName,
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
        protected DbParameter[] CreateInsertParameters(AEntity instance)
        {
            return CreateInsertOrUpdateParameters(instance, instance is IIdEntity<int>);  // TODO: Insert parameters for non IId objects?
        }

        /// <summary>
        /// Creates parameters for a UPDATE database operation. 
        /// </summary>
        /// <returns>A list of SqlParameters.</returns>
        protected DbParameter[] CreateUpdateParameters(AEntity instance)
        {
            return CreateInsertOrUpdateParameters(instance, false);
        }

        /// <summary>
        /// Creates parameters for an INSERT or an UPDATE database operation. 
        /// </summary>
        /// <returns>A list of SqlParameters.</returns>
        protected virtual DbParameter[] CreateInsertOrUpdateParameters(AEntity instance, bool insert)
        {
            var paramList = new List<DbParameter>();

            foreach (var column in instance.DatabaseColumns)
            {
                // Get the instance of this column attribute.
                var attribute = EntityReflector.GetDbColumnAttribute(column);

                // Skip Id attributes on insert and read only attributes.
                if ((insert && attribute.IsId) || attribute.IsReadOnly)
                {
                    continue;
                }

                // Add parameter to the list of parameters.
                paramList.Add(Database.Provider.CreateDbParameter(attribute.Name ?? column.Name, column.GetValue(instance)));
            }

            return paramList.ToArray();
        }

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
        /// <returns>A list of SqlParameters.</returns>
        protected virtual DbParameter[] CreateIdParameters(int id)
        {
            if (TypeInstance is IIdEntity<int> == false || EntityReflector.IsDatabaseTable(TypeInstance) == false)
            {
                throw new Exception("Can not create an Id parameter from a non IEntity or non database object.");
            }

            var paramList = new List<DbParameter>();

            //foreach (var attribute in TypeInstance.DatabaseColumns.Select(ADataObject.GetDbColumnAttribute).Where(attribute => attribute.IsId))
            //{
            //    // Add parameter to the list of parameters.
            //    paramList.Add(Database.DatabaseProvider.CreateDbParameter(attribute.Name, id));

            //    break;
            //}

            foreach (var column in TypeInstance.DatabaseColumns)
            {
                var attribute = EntityReflector.GetDbColumnAttribute(column);
                if (attribute.IsId)
                {
                    paramList.Add(Database.Provider.CreateDbParameter(attribute.Name ?? column.Name, id));

                    break;
                }
            }

            return paramList.ToArray();
        }

        /// <summary>
        /// A select all to a list stored procedure name.
        /// </summary>
        private string SelectStoredProcedureName
        {
            get { return NamesProvider.GetSelectStoredProcedureName(StoredProcedureBaseName); }
        }

        /// <summary>
        /// A select single by ID stored procedure name.
        /// </summary>
        private string SelectDetailsStoredProcedureName
        {
            get { return NamesProvider.GetSelectDetailsStoredProcedureName(StoredProcedureBaseName); }
        }

        /// <summary>
        /// An insert stored procedure name.
        /// </summary>
        private string InsertStoredProcedureName
        {
            get { return NamesProvider.GetInsertStoredProcedureName(StoredProcedureBaseName); }
        }

        /// <summary>
        /// An update stored procedure name.
        /// </summary>
        private string UpdateStoredProcedureName
        {
            get { return NamesProvider.GetUpdateStoredProcedureName(StoredProcedureBaseName); }
        }

        /// <summary>
        /// A delete stored procedure name.
        /// </summary>
        private string DeleteStoredProcedureName
        {
            get { return NamesProvider.GetDeleteStoredProcedureName(StoredProcedureBaseName); }
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
