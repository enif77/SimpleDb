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
    using System.Linq;

    using SimpleDb.Shared;
    using System.Reflection;


    /// <summary>
    /// Consumes data from a SqlDataReader and construts T instances from them.
    /// </summary>
    /// <typeparam name="T">A constructed instnaces data type.</typeparam>
    public class DataConsumer<T> : IDataConsumer<T> where T : AEntity, new()
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">A INamesProvider instance.</param>
        /// <param name="instances">A collection, where created instances are stored.</param>
        public DataConsumer(INamesProvider provider, IEnumerable<PropertyInfo> databaseColumns, ICollection<T> instances)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            DatabaseColumns = databaseColumns ?? throw new ArgumentNullException(nameof(databaseColumns));
            Instances = instances ?? throw new ArgumentNullException(nameof(instances));
        }


        /// <inheritdoc />
        public ICollection<T> Instances { get; }
        

        /// <inheritdoc />
        public virtual bool CreateInstance(IDataReader reader)
        {
            var instance = new T();

            GetData(reader, instance);

            Instances.Add(instance);

            return true;
        }

        /// <inheritdoc />
        public virtual bool RecreateInstance(IDataReader reader)
        {
            if (Instances.Count == 0) throw new Exception("An object instance for recreation expected.");

            // Expecting an instance in the list of instances.
            GetData(reader, Instances.FirstOrDefault());

            return true;
        }


        /// <summary>
        /// A names provider instance.
        /// </summary>
        private INamesProvider Provider { get; }

        /// <summary>
        /// The list of database columns we should get from the reader.
        /// </summary>
        private IEnumerable<PropertyInfo> DatabaseColumns { get; }


        /// <summary>
        /// Extracts data from DB reader to the given instance.
        /// </summary>
        /// <param name="reader">A DB reader ready to give data.</param>
        /// <param name="instance">An instance of a target object.</param>
        private void GetData(IDataReader reader, T instance)
        {
            // For all DB columns...
            foreach (var column in DatabaseColumns)
            {
                // A column name can be specified by the name of a property itself.
                var columnData = reader[Provider.TranslateColumnName(EntityReflector.GetDbColumnName(column))]; // Can throw IndexOutOfRangeException.
                var columnType = column.PropertyType;

                // Get a value from the reader object and convert it it to a property type.
                column.SetValue(instance, (columnData is DBNull)
                    ? (columnType.IsValueType ? Activator.CreateInstance(columnType) : null)
                    : columnData);

                //// Get a value from the reader object and convert it it to a property type.
                //column.SetValue(instance, (columnData is DBNull)
                //    ? (columnType.IsValueType ? Activator.CreateInstance(columnType) : null)
                //    : Convert.ChangeType(columnData, columnType));
            }
        }

        ///// <summary>
        ///// Creates an object instance from the actual SQL reader state and stores it in the Instances collection.
        ///// Data consumer should never call reader.Read() method.
        ///// </summary>
        ///// <param name="reader">A SQL reader instance with a database row ready to be processed.</param>
        ///// <returns>True on succes.</returns>
        //public virtual bool CreateSimpleInstance(SqlDataReader reader)
        //{
        //    return CreateInstance(reader);
        //}
    }
}
