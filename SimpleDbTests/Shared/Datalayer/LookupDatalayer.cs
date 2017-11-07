/* SimpleDbTests - (C) 2016 - 2017 Premysl Fara 
 
SimpleDbTests is available under the zlib license:

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

namespace SimpleDbTests.Shared.Datalayer
{
    using System.Data.SqlClient;

    using SimpleDb.Sql;

    using SimpleDbTests.Shared.DataObjects;


    public class LookupDataLayer : LookupDataLayer<Lookup, int>
    {
        public LookupDataLayer(Database database)
            : base(database)
        {
        }


        /// <summary>
        /// Deletes all rows in DB.
        /// </summary>
        /// <param name="transaction">A SqlTransaction instance or null.</param>
        /// <returns>Number of affected rows.</returns>
        public int DeleteAll(SqlTransaction transaction = null)
        {
            OperationAllowed(DatabaseOperation.Delete);

            return Database.ExecuteNonQuery(((Shared.INamesProvider)NamesProvider).GetDeleteAllStoredProcedureName(StoredProcedureBaseName), null, transaction);
        }
    }
}
