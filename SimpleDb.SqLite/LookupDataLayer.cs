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

namespace SimpleDb.SqLite
{
    using System;
    using System.Linq;

    using SimpleDb.Shared;
    using SimpleDb.Sql;


    public class LookupDataLayer<T, TId> : ABaseIdDatalayer<T, TId> where T : AIdEntity<TId>, ILookup<TId>, new()
    {
        /// <summary>      
        /// Constructor.
        /// </summary>
        /// <param name="database">An initialized Database instance.</param>
        public LookupDataLayer(Database database)
            : base(database)
        {
        }

        /// <summary>
        /// Returns an ID of a lookup item by its name.
        /// </summary>
        /// <param name="name">A name of a lookup for which an Id is requested.</param>
        /// <returns>An Id of a lookup item or 0.</returns>
        public virtual TId GetIdByName(string name)
        {
            throw new NotImplementedException();

            //if (string.IsNullOrEmpty(name)) throw new ArgumentException("A name expected.", nameof(name));

            //OperationAllowed(DatabaseOperation.Select);

            //return Database.ExecuteScalarFunction<TId>(
            //    NamesProvider.GetGetIdByNameFunctionName(FunctionBaseName),
            //    new[]
            //    {
            //        Database.Provider.CreateDbParameter(NamePropertyDbColumnName, name)
            //    },
            //    null);
        }
    }
}
