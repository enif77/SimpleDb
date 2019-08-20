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
    using System.Data;

    using SimpleDb.Core;


    /// <summary>
    /// Defines an instance factory for creating instances of the T type from a IDataRecord instance.
    /// </summary>
    /// <typeparam name="T">A type of data, that will be read from the database.</typeparam>
    public interface IInstanceFactory<T> where T : AEntity, new()
    {
        /// <summary>
        /// Creates a T instance from an IDataRecord instance.
        /// </summary>
        /// <param name="record">An IDataRecord instance.</param>
        /// <param name="instance">An optional instance to which will be data stored instead of creating a new instance of T.</param>
        /// <returns>True on succes.</returns>
        T CreateInstance(IDataRecord record, T instance = null);
    }
}
