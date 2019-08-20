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

namespace SimpleDb.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;

    using SimpleDb.Core;


    /// <summary>
    /// Consumes data from a IDataReader and construts instances of type T from it.
    /// </summary>
    /// <typeparam name="T">The data type of constructed instances.</typeparam>
    public class InstanceFactory<T> : IInstanceFactory<T> where T : AEntity, new()
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">A INamesProvider instance.</param>
        /// <param name="instances">A collection, where created instances are stored.</param>
        public InstanceFactory(INamesProvider provider, IEnumerable<PropertyInfo> databaseColumns)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            DatabaseColumns = databaseColumns ?? throw new ArgumentNullException(nameof(databaseColumns));
        }


        /// <inheritdoc />
        public virtual T CreateInstance(IDataRecord record, T instance = null)
        {
            var i = instance ?? new T();

            // For all DB columns...
            foreach (var column in DatabaseColumns)
            {
                // A column name can be specified by the name of a property itself.
                var columnData = record[Provider.TranslateColumnName(EntityReflector.GetDbColumnName(column))]; // Can throw IndexOutOfRangeException.
                var columnType = column.PropertyType;

                // Get a value from the reader object and convert it it to a property type.
                column.SetValue(i, (columnData is DBNull)
                    ? (columnType.IsValueType ? Activator.CreateInstance(columnType) : null)
                    : columnData);
            }

            return i;
        }


        /// <summary>
        /// A names provider instance.
        /// </summary>
        private INamesProvider Provider { get; }

        /// <summary>
        /// The list of database columns we should get from the reader.
        /// </summary>
        private IEnumerable<PropertyInfo> DatabaseColumns { get; }
    }
}
