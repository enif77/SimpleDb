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
    /// <summary>
    /// Base class for names providers.
    /// </summary>
    public class BaseNamesProvider : INamesProvider
    {
        #region INamesProvider

        /// <inheritdoc />
        public virtual string GetTableName(string tableName)
        {
            return string.Format("\"{0}\"", TranslateTableName(tableName));
        }

        /// <inheritdoc />
        public virtual string GetColumnName(string columnName)
        {
            return string.Format("\"{0}\"", TranslateTableName(columnName));
        }

        /// <inheritdoc />
        public virtual string GetStoredProcedureBaseName(string baseName)
        {
            return "sp" + TranslateTableName(baseName);
        }

        /// <inheritdoc />
        public virtual string GetFunctionBaseName(string baseName)
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

        /// <inheritdoc />
        public virtual string GetGetIdByNameFunctionName(string functionBaseName)
        {
            return functionBaseName + "_GetIdByName";
        }

        /// <inheritdoc />
        public virtual string GetParameterName(string columnName, bool translateName = true)
        {
            return string.Format( "@{0}", translateName ? TranslateColumnName(columnName) : columnName);
        }

        /// <inheritdoc />
        public virtual string GetStoredProcedureParameterName(string columnName, bool translateName = true)
        {
            return GetParameterName(columnName, translateName);
        }

        /// <inheritdoc />
        public virtual string TranslateTableName(string tableName)
        {
            return tableName;
        }

        /// <inheritdoc />
        public virtual string TranslateColumnName(string columnName)
        {
            return columnName;
        }

        #endregion
    }
}
