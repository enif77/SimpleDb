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
            QueryGenerator = queryGenerator ?? new QueryGenerator();
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
        // https://stackoverflow.com/questions/41980813/firebird-net-provider-and-embedded-server-3
        // http://www.ibphoenix.com/files/Embedded_fb3.pdf
        // https://www.firebirdsql.org/pdfmanual/html/ufb-cs-embedded.html

        /// <summary>
        /// Creates a new Firebird database.
        /// </summary>
        /// <param name="host">The name/IP address of the Firebird server to which to connect. Ignored for ebedded databases.</param>
        /// <param name="port">The port. Ignored for ebedded databases.</param>
        /// <param name="fileName">The name of the actual database or the database to be used when a connection is open. It is normally the path to an .FDB file or an alias.</param>
        /// <param name="user">Indicates the User ID to be used when connecting to the data source.</param>
        /// <param name="password">Indicates the password to be used when connecting to the data source.</param>
        /// <param name="pageSize">The size of the database page size. 4096 (the default and the minimum) or 8192 or 16384.</param>
        /// <param name="forcedWrites">If true, data are written to the database synchronously on COMMIT (the safe way). Asynchronously otherwise.</param>
        /// <param name="overwrite">If true, the new database replaces an existing database if exists.</param>
        /// <param name="embeddedDb">If true, an embedded database will be created.</param>
        /// <returns>A connection string for connecting to the created database.</returns>
        public static string CreateDatabase(string host, int port, string fileName, string user, string password, int pageSize, bool forcedWrites, bool overwrite, bool embeddedDb)
        {
            // User=SYSDBA;Password=dba;
            // Database =W:\Devel\db\Firebird\DB.FDB;
            // DataSource =localhost;
            // Port =3050;Dialect=3;Charset=NONE;Role=;
            // Connection lifetime=15;
            // Pooling =true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;

            var csb = new FbConnectionStringBuilder
            {
                Database = fileName,
                DataSource = host,
                Port = port,
                Dialect = 3,
                Charset = "NONE",
                ConnectionLifeTime = 15,
                Pooling = true,
                MinPoolSize = 0,
                MaxPoolSize = 50,
                PacketSize = 8192,
                UserID = user,
                Password = password,
                ServerType = embeddedDb ? FbServerType.Embedded : FbServerType.Default,
                ClientLibrary = "fbclient.dll"
            };

            FbConnection.CreateDatabase(csb.ConnectionString, pageSize, forcedWrites, overwrite);

            return csb.ConnectionString;
        }

        /// <summary>
        /// Creates a connection string for an embedded database.
        /// </summary>
        /// <param name="fileName">A path to the database file.</param>
        /// <param name="charSet">A charset for database strings. NONE by default.</param>
        /// <param name="withPooling">If true, the connection pooling is udsed.</param>
        /// <returns>A connection string for an ebedded database.</returns>
        public static string CreateEmbeddedDatabaseConnectionString(string fileName, string charSet = "NONE", bool withPooling = true)
        {
            return new FbConnectionStringBuilder
            {
                Database = fileName,
                DataSource = "localhost",
                Port = 3050,
                Dialect = 3,
                Charset = charSet,
                ConnectionLifeTime = 15,
                Pooling = withPooling,
                MinPoolSize = 0,
                MaxPoolSize = 50,
                PacketSize = 8192,
                UserID = "SYDBA",
                Password = null,
                ServerType = FbServerType.Embedded,
                ClientLibrary = @"W:\sql\firebird\Firebird-3.0.4.33054-0_Win32\fbclient.dll"
            }.ConnectionString;
        }

        /// <summary>
        /// Creates an embedded database.
        /// </summary>
        /// <param name="fileName">A path to the database file.</param>
        /// <param name="charSet">A charset for database strings. NONE by default.</param>
        /// <param name="withPooling">If true, the connection pooling is udsed.</param>
        /// <param name="pageSize">The size of the database page size. 4096 (the default and the minimum) or 8192 or 16384.</param>
        /// <param name="forcedWrites">If true, data are written to the database synchronously on COMMIT (the safe way). Asynchronously otherwise.</param>
        /// <param name="overwrite">If true, the new database replaces an existing database if exists.</param>
        /// <returns></returns>
        public static string CreateEmbeddedDatabase(string fileName, string charSet = "NONE", bool withPooling = true, int pageSize = 4096, bool forcedWrites = true, bool overwrite = false)
        {
            var csb = CreateEmbeddedDatabaseConnectionString(fileName, charSet, withPooling);

            CreateDatabase(csb, pageSize, forcedWrites, overwrite);

            return csb;
        }

        /// <summary>
        /// Creates a new database.
        /// </summary>
        /// <param name="connectionString">A database connection string.</param>
        /// <param name="pageSize">A database page size. 4096 by default.</param>
        /// <param name="forcedWrites">If true (defaut) all writes to database are immediate.</param>
        /// <param name="overwrite">If a database already exists, it is overwritten.</param>
        public static void CreateDatabase(string connectionString, int pageSize = 4096, bool forcedWrites = true, bool overwrite = false)
        {
            FbConnection.CreateDatabase(connectionString, pageSize, forcedWrites, overwrite);
        }

        /// <summary>
        /// Drops a database.
        /// </summary>
        /// <param name="connectionString">A database connection string.</param>
        public static void DropDatabase(string connectionString)
        {
            FbConnection.DropDatabase(connectionString);
        }


        public static void ClearAllPools()
        {
            FbConnection.ClearAllPools();
        }


        public static void ClearPool(FbConnection connection)
        {
            FbConnection.ClearPool(connection);
        }

        #endregion
    }
}
