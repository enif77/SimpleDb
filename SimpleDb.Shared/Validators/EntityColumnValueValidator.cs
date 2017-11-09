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

namespace SimpleDb.Shared.Validators
{
    using System;
    using System.Reflection;


    /// <summary>
    /// Validates entity column values against defined constraints.
    /// </summary>
    public static class EntityColumnValueValidator
    {
        /// <summary>
        /// Checks, if a null can be set to a certain property.
        /// </summary>
        /// <param name="attribute">A DbColumn extracted from a property.</param>
        /// <param name="column">A PropertyInfo describing a checked column.</param>
        /// <param name="columnValue">A value of the checked column.</param>
        public static void CheckNullValue(DbColumnAttribute attribute, PropertyInfo column, object columnValue)
        {
            if (attribute.IsNullable == false &&
                    (column.PropertyType.IsValueType == false ||   // A ref. type.
                    (column.PropertyType.IsValueType && Nullable.GetUnderlyingType(column.PropertyType) != null)) &&   // Nullable<T>
                    columnValue == null)
            {
                throw new ValidationException(String.Format("The {0} property value can not be null.", column.Name));
            }
        }

        /// <summary>
        /// Checks a value of a property agains know type-based constraints.
        /// </summary>
        /// <param name="attribute">An DbColumn instance od a property.</param>
        /// <param name="column">A PropertyInfo instance of a property.</param>
        /// <param name="columnValue">A value of the checked column.</param>
        public static void CheckTypeValues(DbColumnAttribute attribute, PropertyInfo column, object columnValue)
        {
            // Read only colums can contain any value, when they are inserted/saved.
            if (attribute.IsReadOnly)
            {
                return;
            }

            if (column.PropertyType == typeof(string))
            {
                // Get the property value.
                var value = columnValue as string;
                if (value == null)
                {
                    if (attribute.IsNullable == false)
                    {
                        // Strings are a value type, so we have to check for the null here too.
                        throw new ValidationException(String.Format("The {0} property value can not be null.", column.Name));
                    }

                    // The value is null and because this column is nullable, 
                    // we do not have to do anything with the Nonempty column option here.
                }
                else
                {
                    // String lengths are limited.
                    var stringColumnAttribute = attribute as DbStringColumnAttribute;
                    if (stringColumnAttribute != null)
                    {
                        if (stringColumnAttribute.IsNonempty && String.IsNullOrWhiteSpace(value))
                        {
                            // Nonempty strings should contain something.
                            throw new ValidationException(String.Format("The {0} property value can not be empty.", column.Name));
                        }

                        if (value.Length > stringColumnAttribute.MaxLength)
                        {
                            // The maximum length of a string column can be limited.
                            throw new ValidationException(
                                String.Format("The {0} property value '{1}' is too long. ({2}/{3})",
                                    column.Name,
                                    value,
                                    value.Length,
                                    stringColumnAttribute.MaxLength));
                        }
                    }
                }
            }
            else if (column.PropertyType == typeof(DateTime))
            {
                CheckDateTimeMinValue((DateTime)columnValue, column.Name);
            }
            else if (column.PropertyType == typeof(DateTime?))
            {
                var value = (DateTime?)columnValue;
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
        private static void CheckDateTimeMinValue(DateTime value, string columnName)
        {
            // SQL DateTime min value is January 1, 1753.
            if (value >= new DateTime(1753, 1, 1)) return;

            throw new ValidationException(String.Format("The {0} property value {1} is less than 1.1.1753.", columnName, value));
        }
    }
}
