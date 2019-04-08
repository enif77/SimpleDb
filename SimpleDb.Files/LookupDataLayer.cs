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

namespace SimpleDb.Files
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SimpleDb.Core;
    using SimpleDb.Extensions.Lookups;
    
                                      
    public class LookupDataLayer<T> : ABaseDatalayer<T> where T : AIdEntity<int>, ILookup<int>, new()
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

            // TODO: Replace "Name" with the ALookupEntity<T, TId>.NameColumnTagName constant.
            NamePropertyDbColumnName = EntityReflector.GetDbColumnAttribute(EntityReflector.GetColumnsWithTag("Name", TypeInstance).FirstOrDefault()).Name;
        }


        /// <summary>
        /// Returns an ID of a lookup item by its name.
        /// </summary>
        /// <returns>An Id of a lookup item or 0.</returns>
        public virtual int GetIdByName(string name, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("A name expected.", "name");
            if (string.IsNullOrEmpty(NamePropertyDbColumnName)) throw new Exception("A Name column expected.");

            lock (_lookupCacheLock)
            {
                if (bypassCache == false && _lookupCache.ContainsKey(name))
                {
                    return _lookupCache[name];
                }

                foreach (var dataObject in DataObjects.Values)
                {
                    if (dataObject.Name == name)
                    {
                        if (bypassCache == false)
                        {
                            _lookupCache.Add(name, dataObject.Id);
                        }

                        return dataObject.Id;
                    }
                }
                
                return 0;
            }
        }


        private readonly object _lookupCacheLock;
        private readonly Dictionary<string, int> _lookupCache;

        /// <summary>
        /// Returns a Db column name of a Name column.
        /// </summary>
        private string NamePropertyDbColumnName { get; set; }
    }
}
