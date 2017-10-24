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
    using System.Data;
    using System.Data.Common;

    using SimpleDb.Shared;


    /// <summary>
    /// Defines an interface for a specific database implementation
    /// </summary>
    public interface IDatabaseProvider: INamesProvider
    {
        /// <summary>
        /// Extracts a database name from a connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns>A database name extracted from a connection string.</returns>
        string GetDatabaseName(string connectionString);

        /// <summary>
        /// Returns a connection to a database.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns>A connection to a database.</returns>
        IDbConnection CreateDbConnection(string connectionString);

        /// <summary>
        /// Returns a database parameter instance.
        /// </summary>
        /// <param name="name">A parameter name.</param>
        /// <param name="value">A value or null.</param>
        /// <returns>A database parameter instance.</returns>
        DbParameter CreateDbParameter(string name, object value);

        /// <summary>
        /// Returns a named integer database return parameter instance.
        /// </summary>
        /// <param name="name">A parameter name.</param>
        /// <returns>A named integer database return parameter instance.</returns>
        DbParameter CreateReturnIntDbParameter(string name);

        /// <summary>
        /// Returns a database command instance.
        /// </summary>
        /// <param name="commandType">Specifies, how a command string is interpreted.</param>
        /// <param name="commandTimeout">A command timeout.</param>
        /// <param name="sql">A SQL command source. A stored procedure name or query.</param>
        /// <param name="parameters">Command parameters.</param>
        /// <param name="connection">A database connection.</param>
        /// <param name="transaction">A transaction.</param>
        /// <returns>A database command instance.</returns>
        IDbCommand CreateDbCommand(
            CommandType commandType,
            int commandTimeout,
            string sql,
            DbParameter[] parameters,
            IDbConnection connection,
            IDbTransaction transaction);
    }
}
