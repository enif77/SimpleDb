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

namespace SimpleDb.Firebird
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;

    using FirebirdSql.Data.FirebirdClient;
    using SimpleDb.Sql;

    
    /// <summary>
    /// Database provider for a PostgreSQL database.
    /// </summary>
    public class DatabaseProvider : IDatabaseProvider
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="namesProvider">An INamesProvider instance or null.</param>
        /// <param name="queryGenerator">An IQueryGenerator instance or null.</param>
        public DatabaseProvider(INamesProvider namesProvider = null, IQueryGenerator queryGenerator = null)
        {
            NamesProvider = namesProvider ?? new BaseNamesProvider();
            QueryGenerator = queryGenerator ?? new BaseQueryGenerator();
        }


        #region IDatabaseProvider

        /// <inheritdoc />
        public INamesProvider NamesProvider { get; }

        /// <inheritdoc />
        public IQueryGenerator QueryGenerator { get; }

        /// <inheritdoc />
        public string GetDatabaseName(string connectionString)
        {
            var builder = new FbConnectionStringBuilder(connectionString);

            return string.Format("{0}\\{1}", builder.DataSource, builder.Database);
        }

        /// <inheritdoc />
        public IDbConnection CreateDbConnection(string connectionString)
        {
            return new FbConnection(connectionString);
        }

        /// <inheritdoc />
        public DbParameter CreateDbParameter(string name, object value, bool translateName = true)
        {
            return new FbParameter(NamesProvider.GetParameterName(name, translateName), value ?? DBNull.Value);
        }

        /// <inheritdoc />
        public DbParameter CreateStoredProcedureDbParameter(string name, object value, bool translateName = true)
        {
            return new FbParameter(NamesProvider.GetStoredProcedureParameterName(name, translateName), value ?? DBNull.Value);
        }

        /// <inheritdoc />
        public DbParameter CreateReturnIntDbParameter(string name, bool translateName = true)
        {
            return new FbParameter(NamesProvider.GetParameterName(name, translateName), DbType.Int32)
            {
                Direction = ParameterDirection.ReturnValue
            };
        }

        /// <inheritdoc />
        public IDbCommand CreateDbCommand(CommandType commandType, int commandTimeout, string sql, IEnumerable<NamedDbParameter> parameters, IDbConnection connection, IDbTransaction transaction)
        {
            var command = new FbCommand(sql, connection as FbConnection)
            {
                CommandType = commandType,
                Transaction = transaction as FbTransaction,
                CommandTimeout = Math.Max(commandTimeout, connection.ConnectionTimeout)
            };

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter.DbParameter);
                }
            }

            return command;
        }

        #endregion


        #region helpers

        // https://stackoverflow.com/questions/16354712/how-to-programmatically-create-firebird-database
        // https://www.tabsoverspaces.com/7968-creating-firebird-database-programatically-c-net
        // https://github.com/cincuranet/FirebirdSql.Data.FirebirdClient/blob/master/Provider/src/FirebirdSql.Data.FirebirdClient/FirebirdClient/FbConnectionStringBuilder.cs

        /// <summary>
        /// Creates a new Firebird database.
        /// </summary>
        /// <param name="host">The name/IP address of the Firebird server to which to connect. Ignored for ebedded databases.</param>
        /// <param name="fileName">The name of the actual database or the database to be used when a connection is open. It is normally the path to an .FDB file or an alias.</param>
        /// <param name="user">Indicates the User ID to be used when connecting to the data source.</param>
        /// <param name="password">Indicates the password to be used when connecting to the data source.</param>
        /// <param name="pageSize">The size of the database page size. 4096 (the default and the minimum) or 8192 or 16384.</param>
        /// <param name="forcedWrites">If true, data are written to the database synchronously on COMMIT (the safe way). Asynchronously otherwise.</param>
        /// <param name="overwrite">If true, the new database replaces an existing database if exists.</param>
        /// <param name="embeddedDb">If true, an embedded database will be created.</param>
        public static void CreateDatabase(string host, string fileName, string user, string password, int pageSize, bool forcedWrites, bool overwrite, bool embeddedDb)
        {
            var csb = new FbConnectionStringBuilder
            {
                Database = fileName,
                DataSource = host,
                UserID = user,
                Password = password,
                ServerType = embeddedDb ? FbServerType.Embedded : FbServerType.Default
            };

            FbConnection.CreateDatabase(csb.ConnectionString, pageSize, forcedWrites, overwrite);
        }

        #endregion
    }
}
