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

namespace SimpleDb.MySql
{
    using SimpleDb.Sql;


    /// <summary>
    /// Default names provider for the MySQL database implementation.
    /// </summary>
    public class NamesProvider : BaseNamesProvider
    {
        #region INamesProvider

        /// <inheritdoc />
        public override string GetTableName(string tableName)
        {
            return TranslateTableName(tableName);
        }

        /// <inheritdoc />
        public override string GetColumnName(string columnName)
        {
            return TranslateTableName(columnName);
        }

        /// <inheritdoc />
        public override string GetStoredProcedureBaseName(string baseName)
        {
            return "sp_" + TranslateTableName(baseName);
        }

        /// <inheritdoc />
        public override string GetFunctionBaseName(string baseName)
        {
            return "fn_" + TranslateTableName(baseName);
        }

        /// <inheritdoc />
        public override string GetSelectStoredProcedureName(string storedProcedureBaseName)
        {
            return storedProcedureBaseName + "_select_list";
        }

        /// <inheritdoc />
        public override string GetSelectDetailsStoredProcedureName(string storedProcedureBaseName)
        {
            return storedProcedureBaseName + "_select_details";
        }

        /// <inheritdoc />
        public override string GetUpdateStoredProcedureName(string storedProcedureBaseName)
        {
            return storedProcedureBaseName + "_update";
        }

        /// <inheritdoc />
        public override string GetInsertStoredProcedureName(string storedProcedureBaseName)
        {
            return storedProcedureBaseName + "_insert";
        }

        /// <inheritdoc />
        public override string GetDeleteStoredProcedureName(string storedProcedureBaseName)
        {
            return storedProcedureBaseName + "_delete";
        }

        /// <inheritdoc />
        public override string GetGetIdByNameFunctionName(string functionBaseName)
        {
            return functionBaseName + "_get_id_by_name";
        }

        ///// <inheritdoc />
        //public override string GetParameterName(string columnName, bool translateName = true)
        //{
        //    return "p_" + (translateName ? TranslateColumnName(columnName) : columnName);
        //}

        /// <inheritdoc />
        public override string GetStoredProcedureParameterName(string columnName, bool translateName = true)
        {
            return "p_" + (translateName ? TranslateColumnName(columnName) : columnName);
        }

        /// <inheritdoc />
        public override string TranslateTableName(string tableName)
        {
            return tableName.ToLowerInvariant();
        }

        /// <inheritdoc />
        public override string TranslateColumnName(string columnName)
        {
            return columnName.ToLowerInvariant();
        }

        #endregion
    }
}
