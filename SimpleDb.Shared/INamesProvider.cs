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

namespace SimpleDb.Shared
{
    /// <summary>
    /// Provides data table, column and other names translation.
    /// </summary>
    public interface INamesProvider
    {
        /// <summary>
        /// Creates a stored procedure base name from a base name.
        /// </summary>
        /// <param name="baseName">A base for a stored procedure name. Ex.: An untranslated table name.</param>
        /// <returns>A stored procedure base name from a base name.</returns>
        string GetStoredProcedureBaseName(string baseName);

        /// <summary>
        /// Creates a function base name from a base name.
        /// </summary>
        /// <param name="baseName">A base for a function name. Ex.: An untranslated table name.</param>
        /// <returns>A function base name from a base name.</returns>
        string GetFunctionBaseName(string baseName);

        /// <summary>
        /// Returns a stored procedure name for an SELECT operation.
        /// </summary>
        /// <param name="storedProcedureBaseName">A stored procedure base name returned from the GetStoredProcedureBaseName() method.</param>
        /// <returns>A name of a stored procedure.</returns>
        string GetSelectStoredProcedureName(string storedProcedureBaseName);

        /// <summary>
        /// Returns A stored procedure name for an SELECT by ID operation.
        /// </summary>
        /// <param name="storedProcedureBaseName">A stored procedure base name returned from the GetStoredProcedureBaseName() method.</param>
        /// <returns>A name of a stored procedure.</returns>
        string GetSelectDetailsStoredProcedureName(string storedProcedureBaseName);

        /// <summary>
        /// Returns a stored procedure name for an UPDATE operation.
        /// </summary>
        /// <param name="storedProcedureBaseName">A stored procedure base name returned from the GetStoredProcedureBaseName() method.</param>
        /// <returns>A name of a stored procedure.</returns>
        string GetUpdateStoredProcedureName(string storedProcedureBaseName);

        /// <summary>
        /// Returns a stored procedure name for an INSERT operation.
        /// </summary>
        /// <param name="storedProcedureBaseName">A stored procedure base name returned from the GetStoredProcedureBaseName() method.</param>
        /// <returns>A name of a stored procedure.</returns>
        string GetInsertStoredProcedureName(string storedProcedureBaseName);

        /// <summary>
        /// Returns stored procedure name for the DELETE operation.
        /// </summary>
        /// <param name="storedProcedureBaseName">A stored procedure base name returned from the GetStoredProcedureBaseName() method.</param>
        /// <returns>A name of a stored procedure.</returns>
        string GetDeleteStoredProcedureName(string storedProcedureBaseName);

        /// <summary>
        /// Returns a scalar function name for the GET_ID_BY_NAME operation.
        /// </summary>
        /// <param name="functionBaseName">A function base name returned from the GetFunctionBaseName() method.</param>
        /// <returns>A scalar function name for the GET_ID_BY_NAME operation.</returns>
        string GetGetIdByNameFunctionName(string functionBaseName);

        /// <summary>
        /// Creates a parameter name from a column name.
        /// </summary>
        /// <param name="columnName">An untranslated column name.</param>
        /// <returns>A parameter name.</returns>
        string GetParameterName(string columnName);

        /// <summary>
        /// Translates a table name to the format required by the database.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <returns>A translated table name.</returns>
        string TranslateTableName(string tableName);

        /// <summary>
        /// Translates a column name to the format required by the database.
        /// </summary>
        /// <param name="columnName">A column name.</param>
        /// <returns>A translated column name.</returns>
        string TranslateColumnName(string columnName);
    }
}
