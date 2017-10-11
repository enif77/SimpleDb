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

namespace SimpleDb.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SimpleDb.Shared;


    public class LookupDataLayer<T> : ABaseDatalayer<T> where T : AIdDataObject, ILookup, new()
    {
        /// <summary>      
        /// Constructor.
        /// </summary>
        /// <param name="database">An initialized Database instance.</param>
        public LookupDataLayer(Database database)
            : base(database)
        {
            _lookupCacheLock = new object();
            _lookupCache = new Dictionary<string, int>();

            NamePropertyDbColumnName = ADataObject.GetDbColumnAttribute(TypeInstance.GetColumnsWithTag("Name").FirstOrDefault()).Name;
        }


        /// <summary>
        /// Returns an ID of a lookup item by its name.
        /// </summary>
        /// <returns>An Id of a lookup item or 0.</returns>
        public virtual int GetIdByName(string name, bool bypassCache = false)
        {
            lock (_lookupCacheLock)
            {
                if (String.IsNullOrEmpty(name)) throw new ArgumentException("A name expected.", "name");
                if (String.IsNullOrEmpty(NamePropertyDbColumnName)) throw new Exception("A Name column expected.");

                OperationAllowed(DatabaseOperation.Select);

                if (bypassCache == false && _lookupCache.ContainsKey(name))
                {
                    return _lookupCache[name];
                }

                var id = Database.ExecuteScalarFunction<int>(
                    FunctionBaseName + "_GetIdByName",
                    new[]
                    {
                        Database.DatabaseProvider.CreateDbParameter(GetParameterName(NamePropertyDbColumnName), name)
                    },
                    null);

                if (bypassCache == false)
                {
                    _lookupCache.Add(name, id);
                }

                return id;
            }
        }


        private readonly object _lookupCacheLock;
        private readonly Dictionary<string, int> _lookupCache;

        /// <summary>
        /// Returns a database column name of the Name column.
        /// </summary>
        private string NamePropertyDbColumnName { get; }
    }
}
