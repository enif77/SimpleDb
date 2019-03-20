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
    using System.Text;

    using SimpleDb.Sql.Expressions;


    /// <summary>
    /// A SQL query generator interface.
    /// </summary>
    public interface IQueryGenerator
    {
        /// <summary>
        /// Generates a parametrized SELECT query.
        /// </summary>
        /// <param name="dataTableName">A data table name.</param>
        /// <param name="columnNames">An optional list of columns we wanna to get.</param>
        /// <param name="expression">An optional WHERE clause expression.</param>
        /// <returns>A parametrized SELECT query.</returns>
        string GenerateSelectQuery(string dataTableName, IEnumerable<NamedDbParameter> columnNames, Expression expression);

        /// <summary>
        /// Generates a parametrized INSERT query.
        /// </summary>
        /// <param name="dataTableName">A data table name.</param>
        /// <param name="insertParameters">A list of INSERT parameters.</param>
        /// <param name="withGeneratedIdentity">If true, the generated insert query will return an Id of the inserted row.</param>
        /// <returns>A parametrized INSERT query.</returns>
        string GenerateInsertQuery(string dataTableName, IEnumerable<NamedDbParameter> insertParameters, bool withGeneratedIdentity);

        /// <summary>
        /// Generates a parametrized UPDATE query.
        /// </summary>
        /// <param name="dataTableName">A data table name.</param>
        /// <param name="updateParameters">A list of UPDATE parameters.</param>
        /// <returns>A parametrized UPDATE query.</returns>
        string GenerateUpdateQuery(string dataTableName, IEnumerable<NamedDbParameter> updateParameters);

        /// <summary>
        /// Generates a DELETE query.
        /// </summary>
        /// <param name="dataTableName">A data table name.</param>
        /// <param name="expression">An optional WHERE clause expression.</param>
        /// <returns>A DELETE query.</returns>
        string GenerateDeleteQuery(string dataTableName, Expression expression);

        ///// <summary>
        ///// Generates a WHERE clause from a nonempty list of parameters.
        ///// </summary>
        ///// <param name="parameters">A list of parameters.</param>
        ///// <param name="sb">An output StringBuilder instance.</param> 
        //void GenerateWhereClause(IEnumerable<NamedDbParameter> parameters, StringBuilder sb);

        /// <summary>
        /// Generates a WHERE clause from a nonempty list of parameters.
        /// </summary>
        /// <param name="expression">An expression.</param>
        /// <param name="sb">An output StringBuilder instance.</param> 
        void GenerateWhereClause(Expression expression, StringBuilder sb);
    }
}
