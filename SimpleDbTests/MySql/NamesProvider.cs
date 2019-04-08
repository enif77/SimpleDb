﻿/* SimpleDb - (C) 2017 - 2019 Premysl Fara 
 
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

namespace SimpleDbTests.MySql
{
    using SimpleDb.Sql;


    /// <summary>
    /// Names provider for the MsSQL database implementation.
    /// </summary>
    public class NamesProvider : BaseNamesProvider, Shared.INamesProvider
    {
        /// <inheritdoc />
        public override string TranslateTableName(string tableName)
        {
            if (tableName == "LookupColumnNames")
            {
                return "lookup_column_names";
            }

            return tableName.ToLowerInvariant();
        }


        /// <inheritdoc />
        public override string TranslateColumnName(string columnName)
        {
            if (columnName == "RenamedName")
            {
                return "name_renamed";
            }

            return columnName.ToLowerInvariant();
        }


        public string GetDeleteAllStoredProcedureName(string storedProcedureBaseName)
        {
            return storedProcedureBaseName + "_delete_all";
        }
    }
}
