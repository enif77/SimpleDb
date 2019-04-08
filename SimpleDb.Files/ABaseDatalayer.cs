/* SimpleDb - (C) 2016 - 2019 Premysl Fara 
 
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

namespace SimpleDb.Files
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using SimpleDb.Core;


    /// <summary>
    /// The base of all datalayers.
    /// </summary>
    /// <typeparam name="T">An ABusinessObject type.</typeparam>
    public abstract class ABaseDatalayer<T> where T : AIdEntity<int>, new()
    {
        #region fields
        
        // The last known ID.
        private static int _lastId = 0;
        private static object _lastIdLock = new object();

        #endregion


        #region ctor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="database">An initialised Database instance to be used for all database operations.</param>
        protected ABaseDatalayer(Database database)
        {
            if (database == null) throw new ArgumentNullException("database");

            Database = database;
            TypeInstance = new T();

            // TODO: Fails, when DatabaseTableName/DbTable attribute is not set.
            DataDirectory = Path.Combine(Database.RootDirectoryPath, EntityReflector.GetDatabaseTableName(TypeInstance));

            if (Directory.Exists(DataDirectory) == false)
            {
                Directory.CreateDirectory(DataDirectory);
            }
            
            DataObjects = new Dictionary<int, T>();
        }

        #endregion


        #region properties

        /// <summary>
        /// A Database instance used for all database operations.
        /// </summary>
        public Database Database { get; private set; }

        /// <summary>
        /// An instance of a T, used for reflection operations.
        /// </summary>
        protected T TypeInstance { get; private set; }

        /// <summary>
        /// A full path to the directory, where this datalayer stores data.
        /// </summary>
        protected string DataDirectory { get; private set; }

        /// <summary>
        /// All previously loaded objects.
        /// </summary>
        public Dictionary<int, T> DataObjects { get; private set; }
        

        /// <summary>
        /// The last know (the highest) ID.
        /// </summary>
        public static int LastId
        {
            get
            {
                lock (_lastIdLock)
                {
                    return _lastId;
                }
            }

            set
            {
                lock (_lastIdLock)
                {
                    if (value > _lastId) _lastId = value;
                }
            }
        }

        #endregion


        #region public methods

        /// <summary>
        /// Reloads all objects in the DB into the memory and updates the LastId.
        /// Call once before any use of a newly created ABaseDatalayer instance.
        /// </summary>
        public void UpdateState()
        {
            GetAll();
        }

        /// <summary>
        /// Returns all instances of a T.
        /// </summary>
        /// <param name="userDataConsumer">An optional user data consumer instance.</param>
        /// <returns>IEnumerable of all object instances.</returns>
        public virtual IEnumerable<T> GetAll(IDataConsumer<T> userDataConsumer = null)
        {
            try
            {
                var res = new List<T>();
                var consumer = userDataConsumer ?? new DataConsumer<T>(res);

                var tableDirectory = new DirectoryInfo(DataDirectory);
                foreach (var file in tableDirectory.GetFiles("*.txt"))
                {
                    var de = DataEntity.LoadDataEntity(file.FullName);
                    consumer.CreateInstance(de);
                }

                DataObjects.Clear();

                var maxId = 0;
                foreach (var r in res)
                {
                    if (r.Id > maxId)
                    {
                        maxId = r.Id;
                    }

                    DataObjects.Add(r.Id, r);
                }

                LastId = maxId;

                return res;
            }
            catch (Exception ex)
            {
                LogError(ex);

                throw;
            }
        }
                 
        /// <summary>
        /// Returns instance to object by id
        /// </summary>
        /// <param name="id">Id value</param>
        /// <param name="userDataConsumer">An optional user data consumer instance.</param>
        /// <returns>Instance of an object or null.</returns>
        public virtual T Get(int id, IDataConsumer<T> userDataConsumer = null)
        {
            try
            {
                if (id <= 0) throw new ArgumentException("A positive number expected.", "id");

                if (DataObjects.ContainsKey(id))
                {
                    return DataObjects[id];
                }

                T instance = LoadInstance(id, userDataConsumer);
                if (instance == null) return null;

                DataObjects.Add(instance.Id, instance);

                LastId = instance.Id;

                return instance;
            }
            catch (Exception ex)
            {
                LogError(ex);

                throw;
            }
        }
             
        /// <summary>
        /// Returns instance to object by id
        /// </summary>
        /// <param name="id">Id value</param>
        /// <param name="userDataConsumer">An optional user data consumer instance.</param>
        /// <returns>Instance of an object or null.</returns>
        public virtual T Reload(int id, IDataConsumer<T> userDataConsumer = null)
        {
            try
            {
                if (id <= 0) throw new ArgumentException("A positive number expected.", "id");

                if (DataObjects.ContainsKey(id))
                {
                    DataObjects.Remove(id);
                }

                T instance = LoadInstance(id, userDataConsumer);
                if (instance == null) return null;
                
                DataObjects.Add(instance.Id, instance);

                LastId = instance.Id;

                return instance;
            }
            catch (Exception ex)
            {
                LogError(ex);

                throw;
            }
        }


        /// <summary>
        /// Inserts/updates object in database
        /// </summary>
        /// <param name="obj">Instance to save</param>
        /// <returns>Id of saved instance</returns>
        public virtual int Save(T obj)
        {
            try
            {
                if (obj == null) throw new ArgumentNullException("obj");

                lock (_lastIdLock)
                {
                    if (obj.Id == 0)
                    {
                        obj.Id = LastId + 1;
                    }

                    DataEntity.SaveDataEntity(CreateDataEntity(obj), GetEntityFilePath(obj.Id));

                    return obj.Id;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);

                throw;
            }
        }
               
        /// <summary>
        /// Inserts/updates all objects in transaction.
        /// </summary>
        /// <param name="objects">A list of objects.</param>
        public virtual void SaveAll(IEnumerable<T> objects)
        {
            if (objects == null) throw new ArgumentNullException("objects");

            foreach (var obj in objects)
            {
                try
                {
                    Save(obj);
                }
                catch (Exception ex)
                {
                    var exception = new DatabaseException("Can not save a data item.", ex);

                    exception.Data.Add(obj.GetType().Name, obj.ToString());

                    throw exception;
                }
            }
        }
          
        /// <summary>
        /// Deletes object from database
        /// </summary>
        /// <param name="obj">Instance to delete</param>
        public virtual void Delete(T obj)
        {
            try
            {
                if (obj == null) throw new ArgumentNullException("obj");

                if (obj.Id == 0)
                {
                    return;
                }

                DeleteInstance(obj.Id);
            }
            catch (Exception ex)
            {
                LogError(ex);

                throw;
            }
        }

        /// <summary>
        /// Deletes object from database.
        /// </summary>
        /// <param name="id">An object ID to delete</param>
        public virtual void Delete(int id)
        {
            try
            {
                if (id <= 0) throw new ArgumentException("An object ID expected.", "id");

                DeleteInstance(id);
            }
            catch (Exception ex)
            {
                LogError(ex);

                throw;
            }
        }


        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void LogError(Exception ex)
        {
            // No logging by default.
        }

        #endregion


        #region private methods

        /// <summary>
        /// Creates a new DataEntity instance from this ADataObject instance.
        /// </summary>
        /// <returns>A new DataEntity instance.</returns>
        private DataEntity CreateDataEntity(T obj)
        {
            var entity = new DataEntity();

            // For all DB columns...
            foreach (var column in obj.DatabaseColumns)
            {
                // Get the instance of this column attribute.
                var attribute = EntityReflector.GetDbColumnAttribute(column);

                var columnType = column.PropertyType;
                switch (columnType.Name)
                {
                    // TODO: Nullable types?

                    case "Int32": entity.SetValue(attribute.Name, (int)column.GetValue(obj)); break;
                    case "Boolean": entity.SetValue(attribute.Name, (bool)column.GetValue(obj)); break;
                    case "Decimal": entity.SetValue(attribute.Name, (decimal)column.GetValue(obj)); break;
                    case "DateTime": entity.SetValue(attribute.Name, (DateTime)column.GetValue(obj)); break;
                    case "String": entity.SetValue(attribute.Name, (string)column.GetValue(obj)); break;
                }
            }

            return entity;
        }

        /// <summary>
        /// Loads a data object from an entity data file.
        /// </summary>
        /// <param name="id">An ID of a data object.</param>
        /// <param name="userDataConsumer">A data consumer instance or null.</param>
        /// <returns>A data object instance or null.</returns>
        private T LoadInstance(int id, IDataConsumer<T> userDataConsumer)
        {
            var entityFilePath = GetEntityFilePath(id);
            if (File.Exists(entityFilePath) == false)
            {
                return null;
            }

            var res = new List<T>();
            var consumer = userDataConsumer ?? new DataConsumer<T>(res);
            var de = DataEntity.LoadDataEntity(entityFilePath);
            consumer.CreateInstance(de);

            return res.First();
        }

        /// <summary>
        /// Deletes an instance from the filesystem.
        /// </summary>
        /// <param name="id">An ID of a data object.</param>
        private void DeleteInstance(int id)
        {
            if (DataObjects.ContainsKey(id))
            {
                DataObjects.Remove(id);
            }

            var entityFileName = GetEntityFilePath(id);
            if (File.Exists(entityFileName))
            {
                File.Delete(entityFileName);
            }
        }

        /// <summary>
        /// Creates a full file path from this datalayer instance data directory, a table name and data object ID.
        /// </summary>
        /// <param name="id">An ID of a data object.</param>
        /// <returns>A full path to the entity data file.</returns>
        private string GetEntityFilePath(int id)
        {
            return Path.Combine(DataDirectory, String.Format("{0}_{1}.txt", id, EntityReflector.GetDatabaseTableName(TypeInstance)));
        }

        #endregion
    }
}
