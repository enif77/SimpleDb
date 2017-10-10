/* SimpleDbMsSql - (C) 2016 Premysl Fara 
 
SimpleDbMsSql is available under the zlib license:

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
    using System.Data.SqlClient;
    using System.Text;

    using SimpleDb.Shared;


    /// <summary>
    /// Reperesents a database.
    /// </summary>
    public class Database 
    {
        #region constants

        private const int DefaultCommandTimeoutSeconds = 60;
        private const string ReturnParameterName = "@Result";

        #endregion


        #region properties

        /// <summary>
        /// Returns a connection string used for accessing this instances database.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Returns a connection command timeout used for accessing this instances database.
        /// </summary>
        public int CommandTimeout { get; }

        /// <summary>
        /// Returns database name parsed from connection string
        /// </summary>
        public string DatabaseName
        {
            get
            {
                var builder = new SqlConnectionStringBuilder(ConnectionString);

                return string.Format("{0}\\{1}", builder.DataSource, builder.InitialCatalog);
            }
        }

        #endregion


        #region ctor

        /// <summary>
        /// Creates new instance of Database object for specified connection string
        /// </summary>
        /// <param name="connectionString">Connection string to database</param>
        /// <param name="commandTimeout">An optional command timeout.</param>
        public Database(string connectionString, int commandTimeout = DefaultCommandTimeoutSeconds)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            ConnectionString = connectionString;
            CommandTimeout = (commandTimeout > 0) 
                ? commandTimeout 
                : DefaultCommandTimeoutSeconds;
        }

        #endregion


        #region delegates

        /// <summary>
        /// A method called for each database row returned by a database query or call.
        /// Data consumer should never call reader.Read() method.
        /// </summary>
        /// <param name="reader">A SQL reader instance with a database row ready to be processed.</param>
        /// <returns>True on success.</returns>
        public delegate bool DataConsumer(SqlDataReader reader);

        /// <summary>
        /// A method called during a SQL transaction.
        /// See the DoInTransaction() method.
        /// </summary>
        /// <param name="transaction">A SQL transaction, that operetions in this method can use to connect to the database.</param>
        public delegate void TransactionOperation(SqlTransaction transaction, object data);

        #endregion


        #region public methods

        /// <summary>
        /// Creates a new DatabaseConnection instance with current ConnectionString.
        /// </summary>
        /// <returns>A DatabaseConnection instance.</returns>
        public DatabaseConnection CreateConnection()
        {
            return new DatabaseConnection(ConnectionString);
        }

        /// <summary>
        /// Returns a value indicating whether instance is able to connect to database
        /// </summary>
        /// <returns>Returns true, if it is possible to connect to a database.</returns>
        public bool TryConnect()
        {
            try
            {
                using (CreateConnection())
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Creates a SQL transaction and runs an operation in it.
        /// </summary>
        public void DoInTransaction(TransactionOperation operation, object data = null)
        {
            using (var connection = CreateConnection())
            {
                using (var transaction = connection.Connection.BeginTransaction())
                {
                    operation(transaction, data);

                    transaction.Commit();
                }
            }
        }
        
        /// <summary>
        /// Executes non-query SQL stored procedure and returns number of affected rows.
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure in database</param>
        /// <param name="parameters">Array of SQL parameters</param>
        /// <param name="transaction">SQL transaction object</param>
        /// <returns>Number of affected rows</returns>
        public int ExecuteNonQuery(string storedProcedure, SqlParameter[] parameters, SqlTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(storedProcedure)) throw new ArgumentException("A stored procedure name expected.", "storedProcedure");

            try
            {
                SqlParameter errorCodeParameter;
                if (transaction == null)
                {
                    using (var connection = CreateConnection())
                    {
                        using (var command = CreateCommand(connection.Connection, storedProcedure, parameters, null, out errorCodeParameter))
                        {
                            return command.ExecuteNonQuery();
                        }
                    }
                }

                using (var command = CreateCommand(transaction.Connection, storedProcedure, parameters, transaction, out errorCodeParameter))
                {
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }


        /// <summary>
        /// Executes SQL reader to database
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure in database</param>
        /// <param name="parameters">Array of SQL parameters</param>
        /// <param name="transaction">SQL transaction object</param>
        /// <param name="dataConsumer">A data consumer.</param>
        public void ExecuteReader(string storedProcedure, SqlParameter[] parameters, DataConsumer dataConsumer, SqlTransaction transaction)
        {
            if (string.IsNullOrEmpty(storedProcedure)) throw new ArgumentException("A stored procedure name expected.", "storedProcedure");
            if (dataConsumer == null) throw new ArgumentNullException("dataConsumer");

            try
            {
                SqlParameter errorCodeParameter;
                if (transaction == null)
                {
                    using (var connection = CreateConnection())
                    {
                        using (var command = CreateCommand(connection.Connection, storedProcedure, parameters, null, out errorCodeParameter))
                        {
                            ReadData(command, dataConsumer);
                        }
                    }
                }
                else
                {
                    using (var command = CreateCommand(transaction.Connection, storedProcedure, parameters, transaction, out errorCodeParameter))
                    {
                        ReadData(command, dataConsumer);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        /// <summary>
        ///  Executes SQL command returning a data.
        /// </summary>
        /// <param name="query">A SQL query.</param>
        /// <param name="dataConsumer">A method consuming data from a SqlDataReader reader.</param>
        /// <param name="transaction">A SQL transaction or null.</param>
        public void ExecuteReaderQuery(string query, DataConsumer dataConsumer, SqlTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentException("A SQL query expected.", "query");
            if (dataConsumer == null) throw new ArgumentNullException("dataConsumer");
            
            try
            {
                if (transaction == null)
                {
                    using (var connection = CreateConnection())
                    {
                        using (var command = CreateTextCommand(query, null, connection.Connection, null))
                        {
                            ReadData(command, dataConsumer);
                        }
                    }
                }
                else
                {
                    using (var command = CreateTextCommand(query, null, transaction.Connection, transaction))
                    {
                        ReadData(command, dataConsumer);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }
        
        /// <summary>
        /// Executes SQL reader to database
        /// </summary>
        /// <param name="function">Name of stored procedure in database</param>
        /// <param name="parameters">Array of SQL parameters</param>
        /// <param name="transaction">SQL transaction object</param>
        /// <param name="dataConsumer">A data consumer.</param>
        public void ExecuteReaderFunction(string function, SqlParameter[] parameters, DataConsumer dataConsumer, SqlTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(function)) throw new ArgumentException("A function name expected.", "function");
            if (dataConsumer == null) throw new ArgumentNullException("dataConsumer");

            try
            {
                // SELECT * FROM <functionName> ( <parameters> )
                var query = new StringBuilder();

                query.Append("SELECT * FROM ");
                query.Append(function);
                query.Append(" (");

                var count = 0;
                foreach (var sqlParameter in parameters)
                {
                    query.Append(sqlParameter.ParameterName);

                    if (count < parameters.Length - 1)
                    {
                        query.Append(",");
                    }

                    count++;
                }

                query.Append(")");
                
                if (transaction == null)
                {
                    using (var connection = CreateConnection())
                    {
                        using (var command = CreateTextCommand(query.ToString(), parameters, connection.Connection, null))
                        {
                            ReadData(command, dataConsumer);    
                        }
                    }
                }
                else
                {
                    using (var command = CreateTextCommand(query.ToString(), parameters, transaction.Connection, transaction))
                    {
                        ReadData(command, dataConsumer);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Executes scalar stored procedure which returns integer value
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure in database</param>
        /// <param name="parameters">Array of SQL parameters</param>
        /// <param name="transaction">SQL transaction object</param>
        /// <returns>Scalar value returned from stored procedure</returns>
        public int ExecuteScalar(string storedProcedure, SqlParameter[] parameters, SqlTransaction transaction = null)
        {
            var result = ExecuteScalarObject(storedProcedure, parameters, transaction);

            if (result == null || result.ToString() == "")
            {
                return 0;
            }

            return int.Parse(result.ToString());
        }

        /// <summary>
        /// Executes scalar stored procedure which returns a value
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure in database</param>
        /// <param name="parameters">Array of SQL parameters</param>
        /// <param name="transaction">SQL transaction object</param>
        /// <returns>Scalar value returned from stored procedure</returns>
        public object ExecuteScalarObject(string storedProcedure, SqlParameter[] parameters, SqlTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(storedProcedure)) throw new ArgumentException("A stored procedure name expected.", "storedProcedure");

            try
            {
                SqlParameter errorCodeParameter;
                if (transaction == null)
                {
                    using (var connection = CreateConnection())
                    {
                        using (var command = CreateCommand(connection.Connection, storedProcedure, parameters, null, out errorCodeParameter))
                        {
                            return command.ExecuteScalar();
                        }
                    }
                }

                using (var command = CreateCommand(transaction.Connection, storedProcedure, parameters, transaction, out errorCodeParameter))
                {
                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }
        
        /// <summary>
        /// Executes scalar function with parameters
        /// </summary>
        /// <typeparam name="T">Type of result</typeparam>
        /// <param name="function">Function name</param>
        /// <param name="parameters">Array of parameters</param>
        /// <param name="transaction">SQL transaction object</param>
        /// <returns>Result of the function</returns>
        public T ExecuteScalarFunction<T>(string function, SqlParameter[] parameters, SqlTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(function)) throw new ArgumentException("A function name expected.", "function");

            try
            {
                SqlParameter returnParameter;
                if (transaction == null)
                {
                    using (var connection = CreateConnection())
                    {
                        using (var command = CreateCommand(connection.Connection, function, parameters, null, out returnParameter))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    using (var command = CreateCommand(transaction.Connection, function, parameters, transaction, out returnParameter))
                    {
                        command.ExecuteNonQuery();
                    }    
                }
                
                return (returnParameter.Value == DBNull.Value)
                    ? default(T)
                    : (T)returnParameter.Value;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }
        

        /// <summary>
        /// Static helper method for converting values from SqlDataReader
        /// </summary>
        /// <typeparam name="T">Desired type</typeparam>
        /// <param name="reader">SqlDataReader object representing one row in database</param>
        /// <param name="columnName">Name of column</param>
        /// <returns>Converted value</returns>
        public static T GetValue<T>(SqlDataReader reader, string columnName)
        {
            return (reader[columnName] is DBNull) 
                ? default(T) 
                : (T)reader[columnName];
        }

        /// <summary>
        /// Sometimes bit from DB is not returned as bool...
        /// </summary>
        /// <param name="reader">A reader.</param>
        /// <param name="columnName">A column name.</param>
        /// <returns>True.</returns>
        public static bool GetBooleanValue(SqlDataReader reader, string columnName)
        {
            return (int)reader[columnName] != 0;
        }

        /// <summary>
        /// Convert parameters to string
        /// </summary>
        /// <param name="parameters">Procedure parameters</param>
        /// <returns>Parameters as string</returns>
        public static string ParamsToString(IEnumerable<SqlParameter> parameters)
        {
            var result = new StringBuilder();

            foreach (var param in parameters)
            {
                result.AppendFormat("{0} = {1}{2}", param.ParameterName, param.Value, Environment.NewLine);
            }

            return result.ToString();
        }


        /// <summary>
        /// Creates a SQL parameter from a nullable bool value.
        /// </summary>
        /// <param name="name">A SQL parameter name.</param>
        /// <param name="value">A nullable value.</param>
        /// <returns>A SqlParameter instance.</returns>
        public SqlParameter NullableBooleanParameter(string name, bool? value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("A parameter name expected.", "name");

            return value.HasValue
                ? new SqlParameter(name, value)
                : new SqlParameter(name, DBNull.Value);
        }

        /// <summary>
        /// Creates a SQL parameter from a nullable int value.
        /// </summary>
        /// <param name="name">A SQL parameter name.</param>
        /// <param name="value">A nullable value.</param>
        /// <returns>A SqlParameter instance.</returns>
        public SqlParameter NullableInt32Parameter(string name, int? value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("A parameter name expected.", "name");

            return value.HasValue
                ? new SqlParameter(name, value)
                : new SqlParameter(name, DBNull.Value);
        }

        /// <summary>
        /// Creates a SQL parameter from a nullable decimal value.
        /// </summary>
        /// <param name="name">A SQL parameter name.</param>
        /// <param name="value">A nullable value.</param>
        /// <returns>A SqlParameter instance.</returns>
        public SqlParameter NullableDecimalParameter(string name, decimal? value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("A parameter name expected.", "name");

            return value.HasValue 
                ? new SqlParameter(name, value) 
                : new SqlParameter(name, DBNull.Value);
        }

        /// <summary>
        /// Creates a SQL parameter from a nullable DateTime value.
        /// </summary>
        /// <param name="name">A SQL parameter name.</param>
        /// <param name="value">A nullable value.</param>
        /// <returns>A SqlParameter instance.</returns>
        public SqlParameter NullableDateTimeParameter(string name, DateTime? value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("A parameter name expected.", "name");

            return value.HasValue
                ? new SqlParameter(name, value)
                : new SqlParameter(name, DBNull.Value);
        }

        #endregion


        #region private methods

        /// <summary>
        /// Creates SQL command for database.
        /// </summary>
        /// <param name="connection">SqlConnection instance</param>
        /// <param name="storedProcedure">Name of stored procedure</param>
        /// <param name="parameters">Array of SQL parameters</param>
        /// <param name="transaction">SqlTransaction instance</param>
        /// <param name="errorCodeParameter">Out parameter representing error code</param>
        /// <returns>Instance of created SqlCommand</returns>
        private SqlCommand CreateCommand(SqlConnection connection, string storedProcedure, SqlParameter[] parameters, SqlTransaction transaction, out SqlParameter errorCodeParameter)
        {
            var command = CreateStoredProcedureCommand(storedProcedure, parameters, connection, transaction);

            errorCodeParameter = new SqlParameter(ReturnParameterName, SqlDbType.Int)
            {
                Direction = ParameterDirection.ReturnValue
            };

            command.Parameters.Add(errorCodeParameter);

            return command;
        }


        /// <summary>
        /// Creates a text SQL command.
        /// </summary>
        /// <param name="sql">A SQL command.</param>
        /// <param name="parameters">An array of SQL parameters.</param>
        /// <param name="connection">An open database connection.</param>
        /// <param name="transaction">A transaction or null.</param>
        /// <returns>A SqlCommand instance.</returns>
        private SqlCommand CreateTextCommand(string sql, SqlParameter[] parameters, SqlConnection connection, SqlTransaction transaction)
        {
            return CreateSqlCommand(CommandType.Text, sql, parameters, connection, transaction);
        }

        /// <summary>
        /// Creates a stored procedure command.
        /// </summary>
        /// <param name="storedProcedure">A stored procedure name.</param>
        /// <param name="parameters">An array of SQL parameters.</param>
        /// <param name="connection">An open database connection.</param>
        /// <param name="transaction">A transaction or null.</param>
        /// <returns>A SqlCommand instance.</returns>
        private SqlCommand CreateStoredProcedureCommand(string storedProcedure, SqlParameter[] parameters, SqlConnection connection, SqlTransaction transaction)
        {
            return CreateSqlCommand(CommandType.StoredProcedure, storedProcedure, parameters, connection, transaction);
        }

        /// <summary>
        /// Creates a SQL command.
        /// </summary>
        /// <param name="sql">A SQL command or stored procedure name.</param>
        /// <param name="commandType">A SQL command type.</param>
        /// <param name="parameters">An array of SQL parameters.</param>
        /// <param name="connection">An open database connaction.</param>
        /// <param name="transaction">A transaction or null.</param>
        /// <returns>A SqlCommand instance.</returns>
        private SqlCommand CreateSqlCommand(CommandType commandType, string sql, SqlParameter[] parameters, SqlConnection connection, SqlTransaction transaction)
        {
            var command = new SqlCommand(sql, connection)
            {
                CommandType = commandType,
                Transaction = transaction,
                CommandTimeout = Math.Max(CommandTimeout, connection.ConnectionTimeout)
            };

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command;
        }
        
        
        /// <summary>
        /// Reads data from a SQL command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="dataConsumer"></param>
        private void ReadData(SqlCommand command, DataConsumer dataConsumer)
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (dataConsumer(reader) == false)
                    {
                        break; // Something was wrong. Stop reading.
                    }
                }
            }
        }
        
        /// <summary>
        /// Reads scalar value from a SQL command.
        /// </summary>
        /// <param name="command">A SQL command instance.</param>
        private object ReadScalar(SqlCommand command)
        {
            return command.ExecuteScalar();
        }

        ///// <summary>
        ///// Gets a result returned from a scalar function.
        ///// </summary>
        ///// <typeparam name="T">A type of the returned value.</typeparam>
        ///// <param name="command">A SQL command instance.</param>
        ///// <returns>A value returned from a scalar function.</returns>
        //private T GetScalarFunctionResult<T>(SqlCommand command)
        //{
        //    var returnParameter = new SqlParameter(ReturnParameterName, SqlDbType.Int)
        //    {
        //        Direction = ParameterDirection.ReturnValue
        //    };

        //    command.Parameters.Add(returnParameter);

        //    command.ExecuteNonQuery();

        //    return (returnParameter.Value == null || returnParameter.Value == DBNull.Value)
        //        ? default(T)
        //        : (T)returnParameter.Value;
        //}

        #endregion
    }
}
