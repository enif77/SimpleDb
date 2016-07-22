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

namespace SimpleDb.MsSql
{
    using System.Collections.Generic;
    using System.Data.SqlClient;

    using SimpleDb.Shared;


    public interface IDataConsumer<T> where T : ADataObject, new()
    {
        /// <summary>
        /// A list of T instances.
        /// </summary>
        ICollection<T> Instances { get; }

        /// <summary>
        /// Creates an object instance from the actual SQL reader state and strores it in the Instances collection.
        /// Data consumer should never call reader.Read() method.
        /// </summary>
        /// <param name="reader">A SQL reader instance with a database row ready to be processed.</param>
        /// <returns>True on succes.</returns>
        bool CreateInstance(SqlDataReader reader);

        /// <summary>
        /// Recreates an object instance from the actual SQL reader state.
        /// Data consumer should never call reader.Read() method.
        /// </summary>
        /// <param name="reader">A SQL reader instance with a database row ready to be processed.</param>
        /// <returns>True on succes.</returns>
        bool RecreateInstance(SqlDataReader reader);
    }
}
