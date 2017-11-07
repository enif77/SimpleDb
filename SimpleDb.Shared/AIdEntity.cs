﻿/* SimpleDb - (C) 2016 - 2017 Premysl Fara 
 
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
    /// <summary>
    /// A base class for an entity with an Id column.
    /// </summary>
    /// <typeparam name="TId">A type of the entity Id column (int, long, GUID, ...).</typeparam>
    public abstract class AIdEntity<TId> : AEntity, IEntity<TId>
    {
        #region properties

        /// <summary>
        /// A unique entity identifier.
        /// </summary>
        [DbColumn(IsId = true)]
        [DbColumnTag("Id")]
        public virtual TId Id { get; set; }

        // TODO: Validate, that the Id column is not nullable.

        #endregion
    }
}