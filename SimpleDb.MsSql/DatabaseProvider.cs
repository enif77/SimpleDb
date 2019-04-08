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

namespace SimpleDb.MsSql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;

    using SimpleDb.Sql;


    /// <summary>
    /// Database provider for a MSSQL database.
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
            var builder = new SqlConnectionStringBuilder(connectionString);

            return string.Format("{0}\\{1}", builder.DataSource, builder.InitialCatalog);
        }

        /// <inheritdoc />
        public IDbConnection CreateDbConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        /// <inheritdoc />
        public DbParameter CreateDbParameter(string name, object value, bool translateName = true)
        {
            return new SqlParameter(NamesProvider.GetParameterName(name, translateName), value ?? DBNull.Value);
        }

        /// <inheritdoc />
        public DbParameter CreateStoredProcedureDbParameter(string name, object value, bool translateName = true)
        {
            return new SqlParameter(NamesProvider.GetStoredProcedureParameterName(name, translateName), value ?? DBNull.Value);
        }

        /// <inheritdoc />
        public DbParameter CreateReturnIntDbParameter(string name, bool translateName = true)
        {
            return new SqlParameter(NamesProvider.GetParameterName(name, translateName), SqlDbType.Int)
            {
                Direction = ParameterDirection.ReturnValue
            };
        }

        /// <inheritdoc />
        public IDbCommand CreateDbCommand(CommandType commandType, int commandTimeout, string sql, IEnumerable<NamedDbParameter> parameters, IDbConnection connection, IDbTransaction transaction)
        {
            var command = new SqlCommand(sql, connection as SqlConnection)
            {
                CommandType = commandType,
                Transaction = transaction as SqlTransaction,
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
    }
}
