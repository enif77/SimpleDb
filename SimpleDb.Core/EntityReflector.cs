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

namespace SimpleDb.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;


    /// <summary>
    /// Helper class for getting information about an entity.
    /// </summary>
    public static class EntityReflector
    {
        /// <summary>
        /// Returns true, if an entity has defined the DbTable attribute.
        /// </summary>
        /// <param name="entity">An entity instance.</param>
        /// <returns>True, if an entity has defined the DbTable attribute.</returns>
        public static bool IsDatabaseTable(object entity)
        {
            return Attribute.IsDefined(entity.GetType(), typeof(DbTableAttribute));
        }

        /// <summary>
        /// Returns a database table name of the given entity instance.
        /// If the given entity does not have the DbTableAttribute attribute set, an InvalidOperationException is thrown.
        /// If the Name of the DbTableAttribute attribute is null, the name of the entity class is returned.
        /// </summary>
        /// <param name="entity">An entity instance.</param>
        public static string GetDatabaseTableName(object entity)
        {
            var entityType = entity.GetType();

            var attribute = entityType.GetCustomAttribute(typeof(DbTableAttribute)) as DbTableAttribute;
            if (attribute == null)
            {
                throw new InvalidOperationException(string.Format("The '{0}' has not the DBTable attribute set.", entityType.FullName));
            }

            // The name of a table can be defined by the name of the class.
            return attribute.Name ?? entityType.Name;
        }

        /// <summary>
        /// Returns a collection containing a list of properties with the DbColumn attribute or with attributes inherited from the DbColumn attribute.
        /// </summary>
        /// <param name="entity">An entity instance.</param>
        public static IEnumerable<PropertyInfo> GetDatabaseColumns(object entity)
        {
            return entity.GetType().GetProperties().Where(property => Attribute.IsDefined(property, typeof(DbColumnAttribute), true));
        }

        /// <summary>
        /// Returns a collection containing a list of properties with the DbColumn attribute or with attributes inherited from the DbColumn attribute.
        /// Retudned columns have IsId == true.
        /// </summary>
        /// <param name="entity">An entity instance.</param>
        public static IEnumerable<PropertyInfo> GetIdDatabaseColumns(object entity)
        {
            var idColumns = new List<PropertyInfo>();

            foreach (var column in GetDatabaseColumns(entity))
            {
                if (GetDbColumnAttribute(column).IsId) idColumns.Add(column);
            }

            return idColumns;
        }

        /// <summary>
        /// Returns all properties with a given tag.
        /// </summary>
        /// <param name="tag">A tag.</param>
        /// <param name="entity">An entity instance.</param>
        /// <returns>All properties with a given tag</returns>
        public static IEnumerable<PropertyInfo> GetColumnsWithTag(string tag, object entity)
        {
            if (string.IsNullOrEmpty(tag)) throw new ArgumentException("A database column tag expected.", nameof(tag));

            var taggedColumns = new List<PropertyInfo>();

            foreach (var column in GetDatabaseColumns(entity))
            {
                if (GetDbColumnAttribute(column).Tag == tag) taggedColumns.Add(column);
            }

            return taggedColumns;
        }

        /// <summary>
        /// Gets a DbColumn from a property.
        /// Throws an exception, if the property does not have the DbColumn set.
        /// </summary>
        /// <param name="property">A PropertyInfo instance of a property.</param>
        /// <returns>A DbColumn instance.</returns>
        public static DbColumnAttribute GetDbColumnAttribute(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute(typeof(DbColumnAttribute)) as DbColumnAttribute;
            if (attribute == null)
            {
                throw new InvalidOperationException(String.Format("The DbColumnAttribute is not defined for the {0} property.", property.Name));
            }

            return attribute;
        }

        /// <summary>
        /// Returns a database column name of a property.
        /// </summary>
        /// <param name="property">A property for which we need an database column name.</param>
        /// <returns>A database column name of a property.</returns>
        public static string GetDbColumnName(PropertyInfo property)
        {
            return GetDbColumnAttribute(property).Name ?? property.Name;
        }
    }
}
