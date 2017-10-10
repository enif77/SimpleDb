/* SimpleDbFiles - (C) 2016 Premysl Fara 
 
SimpleDbFiles is available under the zlib license:

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
    using System.Linq;

    using SimpleDb.Shared;


    /// <summary>
    /// Consumes data from a data entity and construts T instances from them.
    /// </summary>
    /// <typeparam name="T">A constructed instnaces data type.</typeparam>
    public class DataConsumer<T> : IDataConsumer<T> where T : ADataObject, new()
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="instances">A collection, where created instances are stored.</param>
        public DataConsumer(ICollection<T> instances)
        {
            if (instances == null) throw new ArgumentNullException("instances");

            Instances = instances;
        }
        

        /// <summary>
        /// A list of T instances.
        /// </summary>
        public ICollection<T> Instances
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates an object instance from a data entity and strores it in the Instances collection.
        /// </summary>
        /// <param name="entity">An entity instance to be processed.</param>
        /// <returns>True on succes.</returns>
        public virtual bool CreateInstance(DataEntity entity)
        {
            var instance = new T();

            GetData(entity, instance);

            Instances.Add(instance);

            return true;
        }

        /// <summary>
        /// Recreates an object instance from an entity.
        /// </summary>
        /// <param name="entity">A data entity to be processed.</param>
        /// <returns>True on succes.</returns>
        public virtual bool RecreateInstance(DataEntity entity)
        {
            if (Instances.Count == 0) throw new Exception("An object instance for recreation expected.");

            // Expecting an instance in the list of instances.
            GetData(entity, Instances.FirstOrDefault());

            return true;
        }

        /// <summary>
        /// Extracts data from a data entity to the given instance.
        /// </summary>
        /// <param name="entity">A data entity ready to give data.</param>
        /// <param name="instance">An instance of a target object.</param>
        private static void GetData(DataEntity entity, T instance)
        {
            // For all DB columns...
            foreach (var column in instance.DatabaseColumns)
            {
                // Get the instance of this column attribute.
                var attribute = ADataObject.GetDbColumnAttribute(column);

                var columnType = column.PropertyType;
                switch (columnType.Name)
                {
                    // TODO: Nullable types?

                    case "Int32": column.SetValue(instance, entity.GetInt32Value(attribute.Name)); break;
                    case "Boolean": column.SetValue(instance, entity.GetBoolValue(attribute.Name)); break;
                    case "Decimal": column.SetValue(instance, entity.GetDecimalValue(attribute.Name)); break;
                    case "DateTime": column.SetValue(instance, entity.GetDateTimeValue(attribute.Name)); break;
                    case "String": column.SetValue(instance, entity.GetValue(attribute.Name)); break;
                }
            }
        }
    }
}
