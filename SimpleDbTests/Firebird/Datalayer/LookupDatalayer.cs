// Copyright (C) Premysl Fara. All rights reserved.
// SimpleDbTests is available under the zlib license.

namespace SimpleDbTests.Firebird.Datalayer
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

            // For Firebird we "have to" use SQL queries for now.
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
