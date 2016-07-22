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

    using SimpleDb.Files;


    /// <summary>
    /// A base class for a business object.
    /// </summary>
    public abstract class AFileDataObject : AIdDataObject
    {
        /// <summary>
        /// Creates a new DataEntity instance from this ADataObject instance.
        /// </summary>
        /// <returns>A new DataEntity instance.</returns>
        public DataEntity CreateDataEntity()
        {
            var entity = new DataEntity();

            // For all DB columns...
            foreach (var column in DatabaseColumns)
            {
                // Get the instance of this column attribute.
                var attribute = ADataObject.GetDbColumnAttribute(column);

                // Skip ignored columns.
                if (attribute.IsIgnored) continue;

                var columnType = column.PropertyType;
                switch (columnType.Name)
                {
                    // TODO: Nullable types?

                    case "Int32": entity.SetValue(attribute.Name, (int)column.GetValue(this)); break;
                    case "Boolean": entity.SetValue(attribute.Name, (bool)column.GetValue(this)); break;
                    case "Decimal": entity.SetValue(attribute.Name, (decimal)column.GetValue(this)); break;
                    case "DateTime": entity.SetValue(attribute.Name, (DateTime)column.GetValue(this)); break;
                    case "String": entity.SetValue(attribute.Name, (string)column.GetValue(this)); break;
                }
            }

            return entity;
        }
    }
}
