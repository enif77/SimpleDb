/* SimpleDbTests - (C) 2016 - 2017 Premysl Fara 
 
SimpleDbTests is available under the zlib license:

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

namespace SimpleDbTests.SqLite.Datalayer
{
    using System.Data;

    using SimpleDb.Extensions.Lookups;
    using SimpleDb.Shared;
    using SimpleDb.Sql;

    using SimpleDbTests.SqLite.DataObjects;
    using System;
    using System.Linq;
    using System.Collections.Generic;


    public class LookupDataLayer : ABaseIdDatalayer<Lookup, long>
    {
        /// <summary>      
        /// Constructor.
        /// </summary>
        /// <param name="database">An initialized Database instance.</param>
        public LookupDataLayer(Database database)
            : base(database)
        {
            // We need a property tagged as a Name database table column.
            var columnWithNameTag = EntityReflector.GetColumnsWithTag(ALookupEntity<Lookup, long>.NameColumnTagName, TypeInstance).FirstOrDefault();
            if (columnWithNameTag == null)
            {
                throw new InvalidOperationException("A column with the " + ALookupEntity<Lookup, long>.NameColumnTagName + " tag expected.");
            }

            // Such a property should be a DbColumn and can have a DbColumn.Name set.
            NamePropertyDbColumnName = EntityReflector.GetDbColumnName(columnWithNameTag);

            // For SQLITE we have to use SQL queries.
            UseQueries = true;
        }

        /// <summary>
        /// Returns an ID of a lookup item by its name.
        /// </summary>
        /// <param name="name">A name of a lookup for which an Id is requested.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        /// <returns>An Id of a lookup item or 0.</returns>
        public virtual long GetIdByName(string name, IDbTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("A name expected.", nameof(name));

            if (UseQueries)
            {
                var res = GetAll(GetDbParameterForName(name), null, new BaseDataConsumer<Lookup>(NamesProvider, IdDatabaseColumns, new List<Lookup>()), transaction);

                return (res.Any())
                    ? res.First().Id
                    : default(long);
            }
            else
            {
                throw new InvalidOperationException("Not supported.");
            }
        }
        

        /// <summary>
        /// Returns a database column name of the Name column.
        /// </summary>
        protected string NamePropertyDbColumnName { get; }


        /// <summary>
        /// Returns a database parameter for a given name.
        /// </summary>
        /// <param name="name">A name.</param>
        /// <returns>A database parameter for a given name.</returns>
        protected IEnumerable<NamedDbParameter> GetDbParameterForName(string name)
        {
            var paramList = new List<NamedDbParameter>();

            // Add parameter to the list of parameters.
            var baseName = NamePropertyDbColumnName;
            paramList.Add(new NamedDbParameter()
            {
                BaseName = baseName,
                Name = NamesProvider.GetColumnName(baseName),
                DbParameter = Database.Provider.CreateDbParameter(baseName, name)
            });

            return paramList;
        }
    }
}
