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
    /// Validates various per-entity constraints.
    /// </summary>
    public static class StaticEntityValidator
    {
        /// <summary>
        /// Checks, if a nullable DbColumn is used on a nullable property type.
        /// </summary>
        /// <param name="attribute">An DbColumn instance of a property.</param>
        /// <param name="column">A PropertyInfo instance of a property.</param>
        public static void CheckNullableType(DbColumnAttribute attribute, PropertyInfo column)
        {
            if (attribute.IsNullable && column.PropertyType.IsValueType)  // A value type.
            {
                if (Nullable.GetUnderlyingType(column.PropertyType) == null)  // A value type that is NOT a Nullable<T>.)
                {
                    throw new ValidationException(string.Format("The {0} property is not a nullable type.", column.Name));
                }
            }
        }
    }
}
