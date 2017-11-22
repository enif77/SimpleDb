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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    /// <summary>
    /// Helper class for generating SQL queries.
    /// Expects all names in target database format.
    /// </summary>
    public static class QueryGenerator
    {
        /// <summary>
        /// Generates a parametrized SELECT query.
        /// </summary>
        /// <param name="dataTableName">A data table name.</param>
        /// <param name="columnNames">An optional list of columns we wanna to get.</param>
        /// <param name="parameters">An optional list of WHERE parameters.</param>
        /// <returns>A parametrized SELECT query.</returns>
        public static string GenerateSelectQuery(string dataTableName, IEnumerable<NamedDbParameter> columnNames, IEnumerable<NamedDbParameter> parameters)
        {
            var sb = new StringBuilder("SELECT ");

            // If we have a non empty list of columns.
            var count = columnNames.Count();
            if (count > 0)
            {
                foreach (var column in columnNames)
                {
                    sb.Append(column.Name);

                    count--;
                    if (count < 1)
                    {
                        break;
                    }

                    sb.Append(",");
                }
            }
            else
            {
                // Get all columns.
                sb.Append("*");
            }

            sb.Append(" FROM ");
            sb.Append(dataTableName);

            // Generate the WHERE clausule using parameters.
            // WHERE param1 = @param1 AND param2 = @param2 ...
            if (parameters != null && parameters.Any())
            {
                GenerateWhereClausule(parameters, sb);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a parametrized INSERT query.
        /// </summary>
        /// <param name="dataTableName">A data table name.</param>
        /// <param name="insertParameters">A list of INSERT parameters.</param>
        /// <returns>A parametrized INSERT query.</returns>
        public static string GenerateInsertQuery(string dataTableName, IEnumerable<NamedDbParameter> insertParameters)
        {
            var columsStringBuilder = new StringBuilder();
            var valuesStringBuilder = new StringBuilder();

            var count = insertParameters.Count();
            foreach (var parameter in insertParameters)
            {
                columsStringBuilder.Append(parameter.Name);
                valuesStringBuilder.Append(parameter.DbParameter.ParameterName);

                count--;
                if (count < 1)
                {
                    break;
                }

                columsStringBuilder.Append(",");
                valuesStringBuilder.Append(",");
            }

            // INSERT INTO Lookup (Name, Description) VALUES (@Name, @Description); SELECT SCOPE_IDENTITY() Id;
            return string.Format("INSERT INTO {0} ({1}) VALUES ({2}); SELECT SCOPE_IDENTITY() Id", dataTableName, columsStringBuilder.ToString(), valuesStringBuilder.ToString());
        }

        /// <summary>
        /// Generates a parametrized UPDATE query.
        /// </summary>
        /// <param name="dataTableName">A data table name.</param>
        /// <param name="updateParameters">A list of UPDATE parameters.</param>
        /// <returns>A parametrized UPDATE query.</returns>
        public static string GenerateUpdateQuery(string dataTableName, IEnumerable<NamedDbParameter> updateParameters)
        {
            // UPDATE table_name SET column1 = value1, column2 = value2, ... WHERE condition; 
            var sb = new StringBuilder("UPDATE ");

            sb.Append(dataTableName);
            sb.Append(" SET ");

            var count = updateParameters.Count();
            foreach (var parameter in updateParameters)
            {
                if (parameter.IsId)
                {
                    // Id parameters are read only.
                    continue;
                }

                sb.Append(parameter.Name);
                sb.Append("=");
                sb.Append(parameter.DbParameter.ParameterName);

                count--;
                if (count < 1)
                {
                    break;
                }

                sb.Append(",");
            }

            GenerateWhereClausule(updateParameters.Where(p => p.IsId), sb);

            return sb.ToString();
        }

        /// <summary>
        /// Generates a DELETE query.
        /// </summary>
        /// <param name="dataTableName">A data table name.</param>
        /// <param name="idParameters">An optional list of parameters used to generate the WHERE clausule.</param>
        /// <returns>A DELETE query.</returns>
        public static string GenerateDeleteQuery(string dataTableName, IEnumerable<NamedDbParameter> parameters)
        {
            // DELETE FROM table WHERE ...
            var sb = new StringBuilder("DELETE FROM ");

            sb.Append(dataTableName);

            // Generate the WHERE clausule using parameters.
            // WHERE param1 = @param1 AND param2 = @param2 ...
            if (parameters != null && parameters.Any())
            {
                GenerateWhereClausule(parameters, sb);
            }

            return sb.ToString();
        }
       
        /// <summary>
        /// Generates a WHERE clausule from a nonempty list of parameters.
        /// </summary>
        /// <param name="parameters">A list of parameters.</param>
        /// <param name="sb">An output StringBuilder instance.</param> 
        public static void GenerateWhereClausule(IEnumerable<NamedDbParameter> parameters, StringBuilder sb)
        {
            sb.Append(" WHERE ");

            var count = parameters.Count();
            foreach (var parameter in parameters)
            {
                sb.Append(parameter.Name);
                sb.Append("=");
                sb.Append(parameter.DbParameter.ParameterName);

                count--;
                if (count < 1)
                {
                    break;
                }

                sb.Append(" AND ");
            }
        }
    }
}
