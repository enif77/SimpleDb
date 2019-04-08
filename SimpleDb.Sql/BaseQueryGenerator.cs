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

namespace SimpleDb.Sql
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using SimpleDb.Sql.Expressions;
    using SimpleDb.Sql.Expressions.Operators;
       

    /// <summary>
    /// Helper class for generating SQL queries.
    /// Expects all names in target database format.
    /// </summary>
    public class BaseQueryGenerator : IQueryGenerator
    {
        /// <summary>
        /// Generates a parametrized SELECT query.
        /// </summary>
        /// <param name="dataTableName">A data table name.</param>
        /// <param name="columnNames">An optional list of columns we wanna to get.</param>
        /// <param name="expression">An optional WHERE clause expression.</param>
        /// <returns>A parametrized SELECT query.</returns>
        public virtual string GenerateSelectQuery(string dataTableName, IEnumerable<NamedDbParameter> columnNames, Expression expression)
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

            // WHERE expression ...
            if (expression != null)
            {
                GenerateWhereClause(expression, sb);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a parametrized INSERT query.
        /// </summary>
        /// <param name="dataTableName">A data table name.</param>
        /// <param name="insertParameters">A list of INSERT parameters.</param>
        /// <param name="withGeneratedIdentity">If true, the generated insert query will return an Id of the inserted row.</param>
        /// <returns>A parametrized INSERT query.</returns>
        public virtual string GenerateInsertQuery(string dataTableName, IEnumerable<NamedDbParameter> insertParameters, bool withGeneratedIdentity)
        {
            var columnsStringBuilder = new StringBuilder();
            var valuesStringBuilder = new StringBuilder();

            var count = insertParameters.Count();
            foreach (var parameter in insertParameters)
            {
                columnsStringBuilder.Append(parameter.Name);
                valuesStringBuilder.Append(parameter.DbParameter.ParameterName);
                
                count--;
                if (count < 1)
                {
                    break;
                }

                columnsStringBuilder.Append(",");
                valuesStringBuilder.Append(",");
            }

            if (withGeneratedIdentity)
            {
                // INSERT INTO Lookup (Name, Description) VALUES (@Name, @Description); SELECT SCOPE_IDENTITY() Id;
                return string.Format("INSERT INTO {0} ({1}) VALUES ({2}){3}",
                    dataTableName,
                    columnsStringBuilder.ToString(),
                    valuesStringBuilder.ToString(),
                    GenerateReturnIdentityForInsertQuery());
            }
            else
            {
                // INSERT INTO Lookup (Name, Description) VALUES (@Name, @Description)
                return string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                    dataTableName,
                    columnsStringBuilder.ToString(),
                    valuesStringBuilder.ToString());
            }
        }

        /// <summary>
        /// Generates the return-indentity for an INSERT query.
        /// By default it is the MSSQL's SELECT SCOPE_IDENTITY() Id.
        /// </summary>
        /// <returns>The return-indentity for an INSERT query.</returns>
        protected virtual string GenerateReturnIdentityForInsertQuery()
        {
            return "; SELECT SCOPE_IDENTITY() \"Id\"";
        }

        /// <summary>
        /// Generates a parametrized UPDATE query.
        /// </summary>
        /// <param name="dataTableName">A data table name.</param>
        /// <param name="updateParameters">A list of UPDATE parameters.</param>
        /// <returns>A parametrized UPDATE query.</returns>
        public virtual string GenerateUpdateQuery(string dataTableName, IEnumerable<NamedDbParameter> updateParameters)
        {
            // UPDATE table_name SET column1 = value1, column2 = value2, ... WHERE condition; 
            var sb = new StringBuilder("UPDATE ");

            sb.Append(dataTableName);
            sb.Append(" SET ");

            var filteredParameters = updateParameters.Where(p => p.IsId == false);
            var count = filteredParameters.Count();
            foreach (var parameter in filteredParameters)
            {
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

            var idParamExpressions = new List<Expression>();
            foreach (var idParam in updateParameters.Where(p => p.IsId))
            {
                idParamExpressions.Add(Expression.ParameterExpression(new EqualOperator(), idParam));
            }

            Expression whereExpression;
            if (idParamExpressions.Count > 1)
            {
                whereExpression = (Expression)Expression.And(idParamExpressions);
            }
            else
            {
                whereExpression = idParamExpressions.First();
            }

            GenerateWhereClause(whereExpression, sb);

            return sb.ToString();
        }

        /// <summary>
        /// Generates a DELETE query.
        /// </summary>
        /// <param name="dataTableName">A data table name.</param>
        /// <param name="expression">An optional WHERE clause expression.</param>
        /// <returns>A DELETE query.</returns>
        public virtual string GenerateDeleteQuery(string dataTableName, Expression expression = null)
        {
            // DELETE FROM table [ WHERE ... ]
            var sb = new StringBuilder("DELETE FROM ");

            sb.Append(dataTableName);

            // WHERE expression ...
            if (expression != null)
            {
                GenerateWhereClause(expression, sb);
            }

            return sb.ToString();
        }
       
        ///// <summary>
        ///// Generates a WHERE clausule from a nonempty list of parameters.
        ///// </summary>
        ///// <param name="parameters">A list of parameters.</param>
        ///// <param name="sb">An output StringBuilder instance.</param> 
        //public virtual void GenerateWhereClause(IEnumerable<NamedDbParameter> parameters, StringBuilder sb)
        //{
        //    sb.Append(" WHERE ");

        //    var count = parameters.Count();
        //    foreach (var parameter in parameters)
        //    {
        //        sb.Append(parameter.Name);
        //        sb.Append("=");
        //        sb.Append(parameter.DbParameter.ParameterName);

        //        count--;
        //        if (count < 1)
        //        {
        //            break;
        //        }

        //        sb.Append(" AND ");
        //    }
        //}


        /// <summary>
        /// Generates a WHERE clause from a nonempty list of parameters.
        /// </summary>
        /// <param name="expression">An expression.</param>
        /// <param name="sb">An output StringBuilder instance.</param> 
        public void GenerateWhereClause(Expression expression, StringBuilder sb)
        {
            sb.Append(" WHERE ");
            expression.Generate(sb);
        }
    }
}
