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

namespace SimpleDb.Sql.MsSql
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    

    /// <summary>
    /// Database provider for a MSSQL database.
    /// </summary>
    public class SqlDatabaseProvider : IDatabaseProvider
    {
        #region IDatabaseProvider

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
        public DbParameter CreateDbParameter(string name, object value)
        {
            return new SqlParameter(GetParameterName(name), value ?? DBNull.Value);
        }

        /// <inheritdoc />
        public DbParameter CreateReturnIntDbParameter(string name)
        {
            return new SqlParameter(name, SqlDbType.Int)
            {
                Direction = ParameterDirection.ReturnValue
            };
        }

        /// <inheritdoc />
        public IDbCommand CreateDbCommand(CommandType commandType, int commandTimeout, string sql, DbParameter[] parameters, IDbConnection connection, IDbTransaction transaction)
        {
            var command = new SqlCommand(sql, connection as SqlConnection)
            {
                CommandType = commandType,
                Transaction = transaction as SqlTransaction,
                CommandTimeout = Math.Max(commandTimeout, connection.ConnectionTimeout)
            };

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command;
        }

        #endregion


        #region INamesProvider

        /// <inheritdoc />
        public string TranslateTableName(string tableName)
        {
            return tableName;
        }

        /// <inheritdoc />
        public string TranslateColumnName(string columnName)
        {
            return columnName;
        }

        /// <inheritdoc />
        public string GetStoredProcedureBaseName(string baseName)
        {
            return "sp" + TranslateTableName(baseName);
        }

        /// <inheritdoc />
        public string GetFunctionBaseName(string baseName)
        {
            return "fn" + TranslateTableName(baseName);
        }

        /// <inheritdoc />
        public virtual string GetSelectStoredProcedureName(string storedProcedureBaseName)
        {
            return storedProcedureBaseName + "_SelectList";
        }

        /// <inheritdoc />
        public virtual string GetSelectDetailsStoredProcedureName(string storedProcedureBaseName)
        {
            return storedProcedureBaseName + "_SelectDetails";
        }

        /// <inheritdoc />
        public virtual string GetUpdateStoredProcedureName(string storedProcedureBaseName)
        {
            return storedProcedureBaseName + "_Update";
        }

        /// <inheritdoc />
        public virtual string GetInsertStoredProcedureName(string storedProcedureBaseName)
        {
            return storedProcedureBaseName + "_Insert";
        }

        /// <inheritdoc />
        public virtual string GetDeleteStoredProcedureName(string storedProcedureBaseName)
        {
            return storedProcedureBaseName + "_Delete";
        }
         
        public virtual string GetGetIdByNameFunctionName(string functionBaseName)
        {
            return functionBaseName + "_GetIdByName";
        }

        /// <inheritdoc />
        public string GetParameterName(string columnName)
        {
            return "@" + TranslateColumnName(columnName);
        }

        #endregion
    }
}
