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

namespace SimpleDb.Shared
{
    using System;


    /// <summary>
    /// Base interface for an audited entity.
    /// </summary>
    /// <typeparam name="TId">A type of the entity Id (int, long, GUID, ...).</typeparam>
    public interface IAuditedEntity<TId> : IEntity<TId>
    {
        /// <summary>
        /// The last update date.
        /// </summary>
        DateTime UpdateDate { get; set; }

        /// <summary>
        /// The last use, who updated this object.
        /// </summary>
        string UpdatedBy { get; set; }

        /// <summary>
        /// When was this object created.
        /// </summary>
        DateTime CreateDate { get; set; }

        /// <summary>
        /// Who created this object.
        /// </summary>
        string CreatedBy { get; set; }
    }
}
