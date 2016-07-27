/* SimpleDb - (C) 2016 Premysl Fara 
 
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

namespace SimpleDb.Shared
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;


    /// <summary>
    /// A base class for a business object.
    /// </summary>
    public abstract class ADataObject : IValidatable, INotifyPropertyChanged
    {
        #region properties
        
        /// <summary>
        /// Returns true, if this object has defined the DbTable attribute.
        /// </summary>
        /// <returns></returns>
        public bool IsDatabaseTable
        {
            get { return Attribute.IsDefined(GetType(), typeof(DbTableAttribute)); }
        }

        /// <summary>
        /// Returns a database table name of this instance.
        /// </summary>
        public string DatabaseTableName
        {
            get
            {
                var thisType = GetType();

                if (!IsDatabaseTable) return String.Format("ADataObject<{0}>", thisType.Name);

                var attribute = thisType.GetCustomAttribute(typeof(DbTableAttribute)) as DbTableAttribute;
                if (attribute == null)
                {
                    // This never happens. We are working with DbColumn properties only.
                    throw new Exception(String.Format("Can not get DbTable from the {0} class.", thisType.Name));
                }

                return attribute.Name;
            }
        }

        /// <summary>
        /// Returns a collection containing a list of properties with the DbColumn attribute.
        /// </summary>
        public IEnumerable<PropertyInfo> DatabaseColumns
        {
            get
            {
                return GetType().GetProperties().Where(property => Attribute.IsDefined(property, typeof(DbColumnAttribute)));
            }
        }

        /// <summary>
        /// Returns a collection containing a list of properties with the DbColumnTag attribute.
        /// </summary>
        public IEnumerable<PropertyInfo> TaggedDatabaseColumns
        {
            get
            {
                return GetType().GetProperties().Where(property => Attribute.IsDefined(property, typeof(DbColumnTagAttribute)));
            }
        }

        #endregion


        #region public

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns>True, if this instance is valid.</returns>
        public virtual void Validate()
        {
            // If this object does not represent a database table, do not run column checks below.
            if (IsDatabaseTable == false)
            {
                return;
            }

            // Check all properties defined as database table columns.
            foreach (var column in DatabaseColumns)
            {
                // Get the instance of this column attribute.
                var attribute = GetDbColumnAttribute(column);

                // Ignored columns should not be validated.
                if (attribute.IsIgnored) continue;

                // Check for non nullable type, when the a.IsNullable is true.
                CheckNullableType(attribute, column);

                // Check for a null value when the a.IsNullable is false.
                CheckNullValue(attribute, column);
                
                // Check know value limits of supported data types.
                CheckTypeValues(attribute, column);
            }
        }

        /// <summary>
        /// Returns all properties with a given tag.
        /// </summary>
        /// <param name="tag">A tag.</param>
        /// <returns>All properties with a given tag</returns>
        public IEnumerable<PropertyInfo> GetColumnsWithTag(string tag)
        {
            if (String.IsNullOrEmpty(tag)) throw new ArgumentException("A tag expected.", "tag");

            var taggedColumns = new List<PropertyInfo>();

            foreach (var column in TaggedDatabaseColumns)
            {
                var attribute = GetDbColumnTagAttribute(column);

                if (attribute.Tag == tag) taggedColumns.Add(column);
            }

            return taggedColumns;
        }
            
        /// <summary>
        /// Gets a DbColumn from a property.
        /// Throws an exception, if the propert does not have the DbColumn set.
        /// </summary>
        /// <param name="column">A PropertyInfo instance of a property.</param>
        /// <returns>A DbColumn instance.</returns>
        public static DbColumnAttribute GetDbColumnAttribute(PropertyInfo column)
        {
            var attribute = column.GetCustomAttribute(typeof(DbColumnAttribute)) as DbColumnAttribute;
            if (attribute == null)
            {
                // This never happens. We are working with DbColumn properties only.
                throw new Exception(String.Format("Can not get DbColumn from the {0} property.", column.Name));
            }

            return attribute;
        }

        /// <summary>
        /// Gets a DbColumnTag from a property.
        /// Throws an exception, if the propert does not have the DbColumnTag set.
        /// </summary>
        /// <param name="column">A PropertyInfo instance of a property.</param>
        /// <returns>A DbColumnTag instance.</returns>
        public static DbColumnTagAttribute GetDbColumnTagAttribute(PropertyInfo column)
        {
            var attribute = column.GetCustomAttribute(typeof(DbColumnTagAttribute)) as DbColumnTagAttribute;
            if (attribute == null)
            {
                // This never happens. We are working with DbColumn properties only.
                throw new Exception(String.Format("Can not get DbColumnTag from the {0} property.", column.Name));
            }

            return attribute;
        }
        
        #endregion


        #region events

        /// <summary>
        /// Event for INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        #endregion


        #region private

        /// <summary>
        /// Invoker for PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of property</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Checks, if nullability of a DbColumn matches the type of a property.
        /// </summary>
        /// <param name="attribute">An DbColumn instance od a property.</param>
        /// <param name="column">A PropertyInfo instance of a property.</param>
        private void CheckNullableType(DbColumnAttribute attribute, PropertyInfo column)
        {
            if (attribute.IsNullable && column.PropertyType.IsValueType)  // A value type.
            {
                if (Nullable.GetUnderlyingType(column.PropertyType) == null)  // A value type that is NOT a Nullable<T>.)
                {
                    throw new ValidationException(String.Format("The {0} property is not a nullable type.", column.Name));
                }
            }
        }

        /// <summary>
        /// Checks, if a null can be set to a certain property.
        /// </summary>
        /// <param name="attribute">An DbColumn instance od a property.</param>
        /// <param name="column">A PropertyInfo instance of a property.</param>
        private void CheckNullValue(DbColumnAttribute attribute, PropertyInfo column)
        {
            if (attribute.IsNullable == false &&
                    (column.PropertyType.IsValueType == false ||   // A ref. type.
                    (column.PropertyType.IsValueType && Nullable.GetUnderlyingType(column.PropertyType) != null)) &&   // Nullable<T>
                    column.GetValue(this) == null)
            {
                throw new ValidationException(String.Format("The {0} property value can not be null.", column.Name));
            }
        }

        /// <summary>
        /// Checks a value of a property agains know type-based constraints.
        /// </summary>
        /// <param name="attribute">An DbColumn instance od a property.</param>
        /// <param name="column">A PropertyInfo instance of a property.</param>
        private void CheckTypeValues(DbColumnAttribute attribute, PropertyInfo column)
        {
            // Read only colums can contain any value, when they are inserted/saved.
            if (attribute.IsReadOnly)
            {
                return;
            }

            if (column.PropertyType == typeof(string))
            {
                // Check a string length and the null value for string types.
                var value = column.GetValue(this) as string;
                if (value == null && attribute.IsNullable == false)
                {
                    // Strings are a value type, so we have to check for the null here too.
                    throw new ValidationException(String.Format("The {0} property value can not be null.", column.Name));
                }

                // String lengths are limited.
                if (value != null && value.Length > attribute.Length)
                {
                    throw new ValidationException(String.Format("The {0} property value '{1}' is too long. ({2}/{3})", column.Name, value, value.Length, attribute.Length));
                }
            }
            else if (column.PropertyType == typeof(DateTime))
            {
                CheckDateTimeMinValue((DateTime)column.GetValue(this), column.Name);
            }
            else if (column.PropertyType == typeof(DateTime?))
            {
                var value = (DateTime?)column.GetValue(this);
                if (value != null)
                {
                    CheckDateTimeMinValue(value.Value, column.Name);
                }
            }
        }

        /// <summary>
        /// Checks, if a value of a DateTime property can be stored in the database.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <param name="columnName">A table column name.</param>
        private void CheckDateTimeMinValue(DateTime value, string columnName)
        {
            // SQL DateTime min value is January 1, 1753.
            if (value >= new DateTime(1753, 1, 1)) return;

            throw new ValidationException(String.Format("The {0} property value {1} is less than 1.1.1753.", columnName, value));
        }

        #endregion
    }
}
