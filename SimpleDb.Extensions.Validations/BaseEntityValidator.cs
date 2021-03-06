﻿/* SimpleDb - (C) 2016 - 2019 Premysl Fara 
 
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

namespace SimpleDb.Extensions.Validations
{
    using System.Collections.Generic;

    using SimpleDb.Core;
    using SimpleDb.Extensions.Validations.Validators;
        

    /// <summary>
    /// An entity validator.
    /// </summary>
    public class BaseEntityValidator : IEntityValidator
    {
        /// <inheritdoc />
        public virtual void Validate(AEntity entity)
        {
            // Check all properties defined as database table columns.
            foreach (var column in entity.DatabaseColumns)
            {
                // Get the instance of this column attribute.
                var attribute = EntityReflector.GetDbColumnAttribute(column);

                // Check for non nullable type, when the a.IsNullable is true.
                StaticEntityValidator.CheckNullableColumn(attribute, column);

                var columnValue = column.GetValue(this);

                // Check for a null value when the a.IsNullable is false.
                EntityColumnValueValidator.CheckNullValue(attribute, column, columnValue);

                // Check know value limits of supported data types.
                EntityColumnValueValidator.CheckTypeValues(attribute, column, columnValue);
            }
        }

        /// <inheritdoc />
        public void ValidateAll(IEnumerable<AEntity> entities)
        {
            foreach (var entity in entities)
            {
                Validate(entity);
            }
        }
    }
}
