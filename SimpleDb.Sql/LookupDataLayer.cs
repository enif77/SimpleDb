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

namespace SimpleDb.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SimpleDb.Shared;


    public class LookupDataLayer<T> : ABaseDatalayer<T> where T : AIdDataObject, ILookup, new()
    {
        /// <summary>
        /// If true, lookups are cached in the memory.
        /// False by default.
        /// </summary>
        public bool UseCache { get; set; }


        /// <summary>      
        /// Constructor.
        /// </summary>
        /// <param name="database">An initialized Database instance.</param>
        public LookupDataLayer(Database database)
            : base(database)
        {
            UseCache = false;
            NamePropertyDbColumnName = ADataObject.GetDbColumnAttribute(TypeInstance.GetColumnsWithTag("Name").FirstOrDefault()).Name;
        }


        /// <summary>
        /// Checks, if a lookup is in the internal cache.
        /// </summary>
        /// <param name="name">A lookup Name.</param>
        /// <returns>True, if a lookup with a specified name is in the local cache.</returns>
        protected bool HasCachedLookup(string name)
        {
            lock (_lookupCacheLock)
            {
                if (UseCache)
                {
                    return _lookupCache.ContainsKey(name);

                }

                return false;
            }
        }

        /// <summary>
        /// Clears the internal lookup cache.
        /// </summary>
        public void ClearCache()
        {
            lock (_lookupCacheLock)
            {
                _lookupCache.Clear();
            }
        }
        
        /// <summary>
        /// Returns an ID of a lookup item by its name.
        /// </summary>
        /// <returns>An Id of a lookup item or 0.</returns>
        public virtual int GetIdByName(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("A name expected.", nameof(name));
            if (string.IsNullOrEmpty(NamePropertyDbColumnName)) throw new Exception("A Name column expected.");

            OperationAllowed(DatabaseOperation.Select);

            lock (_lookupCacheLock)
            {
                if (HasCachedLookup(name))
                {
                    return _lookupCache[name];
                }

                var id = Database.ExecuteScalarFunction<int>(
                    Database.Provider.GetGetIdByNameFunctionName(FunctionBaseName),
                    new[]
                    {
                        Database.Provider.CreateDbParameter(NamePropertyDbColumnName, name)
                    },
                    null);

                if (UseCache)
                {
                    _lookupCache.Add(name, id);
                }

                return id;
            }
        }


        private readonly object _lookupCacheLock = new object();
        private readonly Dictionary<string, int> _lookupCache = new Dictionary<string, int>();


        /// <summary>
        /// Returns a database column name of the Name column.
        /// </summary>
        protected string NamePropertyDbColumnName { get; }
    }
}
