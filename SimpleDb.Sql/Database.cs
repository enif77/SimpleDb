﻿/* SimpleDb - (C) 2016 - 2019 Premysl Fara 
 
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
    using System.Data;
    using System.Data.Common;
    using System.Text;

    using SimpleDb.Core;


    /// <summary>
    /// Low level database operations.
    /// </summary>
    public class Database
    {
        #region constants

        private const int DefaultCommandTimeoutSeconds = 60;
        private const string ReturnParameterName = "Result";

        #endregion


        #region properties

        /// <summary>
        /// A IDatabaseProvider instance.
        /// </summary>
        public IDatabaseProvider Provider { get; }

        /// <summary>
        /// A connection string used for accessing this instance database.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// A connection command timeout used for accessing this Database instance database.
        /// </summary>
        public int CommandTimeout { get; }

        /// <summary>
        /// A database name parsed from the Connection string.
        /// </summary>
        public string DatabaseName => Provider.GetDatabaseName(ConnectionString);

        #endregion


        #region ctor

        /// <summary>
        /// Creates new instance of a Database object for specified connection string.
        /// </summary>
        /// <param name="connectionString">Connection string to database</param>
        /// <param name="databaseProvider">A database provider instance.</param>
        /// <param name="commandTimeout">An optional command timeout.</param>
        public Database(string connectionString, IDatabaseProvider databaseProvider, int commandTimeout = DefaultCommandTimeoutSeconds)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("A connection string expected.", nameof(connectionString));

            ConnectionString = connectionString;
            Provider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
            CommandTimeout = (commandTimeout > 0)
                ? commandTimeout
                : DefaultCommandTimeoutSeconds;
        }

        #endregion


        #region delegates

        /// <summary>
        /// A method called during a SQL transaction.
        /// See the DoInTransaction() method.
        /// </summary>
        /// <param name="transaction">A SQL transaction, that operetions in this method can use to connect to the database.</param>
        public delegate void TransactionOperation(IDbTransaction transaction, object data);

        #endregion


        #region public methods

        /// <summary>
        /// Creates a new DatabaseConnection instance with current ConnectionString.
        /// </summary>
        /// <returns>A DatabaseConnection instance.</returns>
        public DatabaseConnection CreateConnection()
        {
            return new DatabaseConnection(ConnectionString, Provider);
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
        /// <param name="commandType">A command type.</param>
        /// <param name="sql">Name of stored procedure in database</param>
        /// <param name="parameters">Array of SQL parameters</param>
        /// <param name="transaction">SQL transaction object</param>
        /// <returns>Number of affected rows</returns>
        public int ExecuteNonQuery(CommandType commandType, string sql, IEnumerable<NamedDbParameter> parameters, IDbTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(sql)) throw new ArgumentException("A SQL query or a stored procedure name expected.", nameof(sql));

            if (transaction == null)
            {
                using (var connection = CreateConnection())
                {
                    using (var command = CreateCommand(commandType, sql, parameters, connection.Connection, null))
                    {
                        return command.ExecuteNonQuery();
                    }
                }
            }

            using (var command = CreateCommand(commandType, sql, parameters, transaction.Connection, transaction))
            {
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Reads data from a database.
        /// </summary>
        /// <param name="commandType">A command type.</param>
        /// <param name="sql">A SQL command or a stored procedure name.</param>
        /// <param name="parameters">Array of SQL parameters</param>
        /// <param name="transaction">SQL transaction object</param>
        /// <param name="dataConsumer">A data consumer.</param>
        public IEnumerable<IDataRecord> ExecuteReader(CommandType commandType, string sql, IEnumerable<NamedDbParameter> parameters, IDbTransaction transaction)
        {
            if (string.IsNullOrEmpty(sql)) throw new ArgumentException("A SQL query or a stored procedure name expected.", nameof(sql));

            if (transaction == null)
            {
                using (var connection = CreateConnection())
                {
                    using (var command = CreateCommand(commandType, sql, parameters, connection.Connection, null))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                yield return reader;
                            }
                        }
                    }
                }
            }
            else
            {
                using (var command = CreateCommand(commandType, sql, parameters, transaction.Connection, transaction))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return reader;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads data from a database.
        /// </summary>
        /// <param name="instanceFactory">An IInstanceFactory instance.</param>
        /// <param name="commandType">A command type.</param>
        /// <param name="sql">A SQL command or a stored procedure name.</param>
        /// <param name="parameters">Array of SQL parameters</param>
        /// <param name="transaction">SQL transaction object</param>
        /// <param name="dataConsumer">A data consumer.</param>
        public IEnumerable<T> ExecuteReader<T>(IInstanceFactory<T> instanceFactory, CommandType commandType, string sql, IEnumerable<NamedDbParameter> parameters, IDbTransaction transaction)
            where T : AEntity, new()
        {
            if (instanceFactory == null) throw new ArgumentNullException(nameof(instanceFactory));
            if (string.IsNullOrEmpty(sql)) throw new ArgumentException("A SQL query or a stored procedure name expected.", nameof(sql));

            if (transaction == null)
            {
                using (var connection = CreateConnection())
                {
                    using (var command = CreateCommand(commandType, sql, parameters, connection.Connection, null))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                yield return instanceFactory.CreateInstance(reader);
                            }
                        }
                    }
                }
            }
            else
            {
                using (var command = CreateCommand(commandType, sql, parameters, transaction.Connection, transaction))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return instanceFactory.CreateInstance(reader);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Executes a query or a stored procedure and returns a value of the first column of the first row in the result set returned by the query.
        /// Can be used for DB functions too.
        /// </summary>
        /// <typeparam name="T">The type of the returned value.</typeparam>
        /// <param name="instanceFactory">An IInstanceFactory instance.</param>
        /// <param name="commandType">A command type.</param>
        /// <param name="sql">A SQL command or a stored procedure name.</param>
        /// <param name="parameters">Array of SQL parameters.</param>
        /// <param name="transaction">SQL transaction object.</param>
        /// <returns>Scalar value returned from stored procedure.</returns>
        public T ExecuteScalar<T>(CommandType commandType, string sql, IEnumerable<NamedDbParameter> parameters, IDbTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(sql)) throw new ArgumentException("A SQL query or a stored procedure name expected.", nameof(sql));

            object result;
            if (transaction == null)
            {
                using (var connection = CreateConnection())
                {
                    using (var command = CreateCommand(commandType, sql, parameters, connection.Connection, null))
                    {
                        result = command.ExecuteScalar();
                    }
                }
            }
            else
            {
                using (var command = CreateCommand(commandType, sql, parameters, transaction.Connection, transaction))
                {
                    result = command.ExecuteScalar();
                }
            }

            return GetValue<T>(result);
        }

        /// <summary>
        /// Executes scalar function with parameters, that returns an INT value by the RETURN statement.
        /// </summary>
        /// <param name="function">Function name.</param>
        /// <param name="parameters">Array of parameters.</param>
        /// <param name="transaction">SQL transaction object.</param>
        /// <returns>Result of the function.</returns>
        public int ExecuteScalarFunction(string function, IEnumerable<NamedDbParameter> parameters, IDbTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(function)) throw new ArgumentException("A function name expected.", nameof(function));

            var returnParameter = Provider.CreateReturnIntDbParameter(ReturnParameterName);
            if (transaction == null)
            {
                using (var connection = CreateConnection())
                {
                    using (var command = CreateCommand(CommandType.StoredProcedure, function, parameters, connection.Connection, null))
                    {
                        command.Parameters.Add(returnParameter);

                        command.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using (var command = CreateCommand(CommandType.StoredProcedure, function, parameters, transaction.Connection, transaction))
                {
                    command.Parameters.Add(returnParameter);

                    command.ExecuteNonQuery();
                }
            }

            return (returnParameter.Value == null || Convert.IsDBNull(returnParameter.Value))
                ? default(int)
                : Convert.ToInt32(returnParameter.Value);
        }

        /// <summary>
        /// Returns a value converted to a type T.
        /// </summary>
        /// <typeparam name="T">The type ov the returned value.</typeparam>
        /// <param name="value">A value.</param>
        /// <returns>A value converted to a type T.</returns>
        public static T GetValue<T>(object value)
        {
            if (value == null || Convert.IsDBNull(value))
            {
                return default(T);
            }

            if (value is T)
            {
                return (T)value;
            }
            else
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }

        /// <summary>
        /// Static helper method for converting values from SqlDataReader
        /// </summary>
        /// <typeparam name="T">Desired type</typeparam>
        /// <param name="reader">SqlDataReader object representing one row in database</param>
        /// <param name="columnName">Name of column</param>
        /// <returns>Converted value</returns>
        public static T GetValue<T>(IDataReader reader, string columnName)
        {
            return GetValue<T>(reader[columnName]);
        }

        /// <summary>
        /// Sometimes bit from DB is not returned as bool...
        /// </summary>
        /// <param name="reader">A reader.</param>
        /// <param name="columnName">A column name.</param>
        /// <returns>True.</returns>
        public static bool GetBooleanValue(IDataReader reader, string columnName)
        {
            return (int)reader[columnName] != 0;
        }

        /// <summary>
        /// Convert parameters to string
        /// </summary>
        /// <param name="parameters">Procedure parameters</param>
        /// <returns>Parameters as string</returns>
        public static string ParamsToString(IEnumerable<DbParameter> parameters)
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
        public DbParameter NullableBooleanParameter(string name, bool? value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("A parameter name expected.", nameof(name));

            return Provider.CreateDbParameter(name, value);
        }

        /// <summary>
        /// Creates a SQL parameter from a nullable int value.
        /// </summary>
        /// <param name="name">A SQL parameter name.</param>
        /// <param name="value">A nullable value.</param>
        /// <returns>A SqlParameter instance.</returns>
        public DbParameter NullableInt32Parameter(string name, int? value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("A parameter name expected.", nameof(name));

            return Provider.CreateDbParameter(name, value);
        }

        /// <summary>
        /// Creates a SQL parameter from a nullable decimal value.
        /// </summary>
        /// <param name="name">A SQL parameter name.</param>
        /// <param name="value">A nullable value.</param>
        /// <returns>A SqlParameter instance.</returns>
        public DbParameter NullableDecimalParameter(string name, decimal? value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("A parameter name expected.", nameof(name));

            return Provider.CreateDbParameter(name, value);
        }

        /// <summary>
        /// Creates a SQL parameter from a nullable DateTime value.
        /// </summary>
        /// <param name="name">A SQL parameter name.</param>
        /// <param name="value">A nullable value.</param>
        /// <returns>A SqlParameter instance.</returns>
        public DbParameter NullableDateTimeParameter(string name, DateTime? value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("A parameter name expected.", nameof(name));

            return Provider.CreateDbParameter(name, value);
        }

        #endregion


        #region private methods

        /// <summary>
        /// Creates a SQL command.
        /// </summary>
        /// <param name="commandType">A command type.</param>
        /// <param name="sql">A SQL command or a stored procedure name.</param>
        /// <param name="parameters">An array of SQL parameters.</param>
        /// <param name="connection">An open database connection.</param>
        /// <param name="transaction">A transaction or null.</param>
        /// <returns>A SqlCommand instance.</returns>
        private IDbCommand CreateCommand(CommandType commandType, string sql, IEnumerable<NamedDbParameter> parameters, IDbConnection connection, IDbTransaction transaction)
        {
            return Provider.CreateDbCommand(commandType, CommandTimeout, sql, parameters, connection, transaction);
        }

        /// <summary>
        /// Reads scalar value from a database command.
        /// </summary>
        /// <param name="command">A SQL command instance.</param>
        private object ReadScalar(IDbCommand command)
        {
            return command.ExecuteScalar();
        }

        #endregion
    }
}
