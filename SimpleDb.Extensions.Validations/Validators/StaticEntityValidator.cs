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

namespace SimpleDb.Extensions.Validations.Validators
{
    using System;
    using System.Reflection;

    using SimpleDb.Core;


    /// <summary>
    /// Validates various per-entity constraints.
    /// </summary>
    public static class StaticEntityValidator
    {
        /// <summary>
        /// Checks, if a DbColumnAttribute was set to a property with a supported type.
        /// </summary>
        /// <param name="attribute">An DbColumn instance of a property.</param>
        /// <param name="column">A PropertyInfo instance of a property.</param>
        public static void CheckSupportedType(DbColumnAttribute attribute, PropertyInfo column)
        {
            if (attribute is DbStringColumnAttribute)
            {
                if (column.PropertyType != typeof(string))
                {
                    throw new ValidationException(string.Format("The {0} property is not a string type.", column.Name));
                }
            }

            // TODO: Add type validations for other DbColumnAttribute types.
        }

        /// <summary>
        /// Checks, if a nullable DbColumn is used on a nullable property type.
        /// </summary>
        /// <param name="attribute">An DbColumn instance of a property.</param>
        /// <param name="column">A PropertyInfo instance of a property.</param>
        public static void CheckNullableColumn(DbColumnAttribute attribute, PropertyInfo column)
        {
            if (attribute.IsNullable && IsNullableType(column.PropertyType) == false)
            {
                throw new ValidationException(string.Format("The {0} property is not a nullable type.", column.Name));
            }
        }

        /// <summary>
        /// Checks, if a null can be set to a certain property.
        /// </summary>
        /// <param name="attribute">A DbColumn extracted from a property.</param>
        /// <param name="column">A PropertyInfo describing a checked column.</param>
        public static bool CanBeNull(DbColumnAttribute attribute, PropertyInfo column)
        {
            return attribute.IsNullable && IsNullableType(column.PropertyType);
        }

        /// <summary>
        /// Checks, if the type of a column is nullable.
        /// </summary>
        /// <param name="type">A type.</param>
        /// <returns>True, if a type can accept the null value.</returns>
        public static bool IsNullableType(Type type)
        {
            return
                type.IsValueType == false ||   // A ref. type.
                (type.IsValueType && Nullable.GetUnderlyingType(type) != null);   // Nullable<T>
        }
    }
}
