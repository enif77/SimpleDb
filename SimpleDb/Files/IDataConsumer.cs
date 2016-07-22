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

namespace SimpleDb.Files
{
    using System.Collections.Generic;

    using SimpleDb.Shared;


    public interface IDataConsumer<T> where T : ADataObject, new()
    {
        /// <summary>
        /// A list of T instances.
        /// </summary>
        ICollection<T> Instances { get; }

        /// <summary>
        /// Creates an object instance from a data entity and strores it in the Instances collection.
        /// </summary>
        /// <param name="entity">An entity instance to be processed.</param>
        /// <returns>True on succes.</returns>
        bool CreateInstance(DataEntity entity);

        /// <summary>
        /// Recreates an object instance from an entity.
        /// </summary>
        /// <param name="entity">A data entity to be processed.</param>
        /// <returns>True on succes.</returns>
        bool RecreateInstance(DataEntity entity);
    }
}
