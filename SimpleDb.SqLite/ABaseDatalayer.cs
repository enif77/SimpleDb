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

namespace SimpleDb.SqLite
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Text;

    using SimpleDb.Shared;
    using SimpleDb.Sql;
    

    /// <summary>
    /// The base of all datalayers.
    /// </summary>
    /// <typeparam name="T">An ABusinessObject type.</typeparam>
    public abstract class ABaseDatalayer<T> : SimpleDb.Sql.ABaseDatalayer<T> where T : AEntity, new()
    {

        #region ctor

        /// <inheritdoc />
        protected ABaseDatalayer(Database database) : base(database)
        {
        }

        #endregion


        #region public methods

        /// <inheritdoc />
        public override IEnumerable<T> GetAll(IEnumerable<NamedDbParameter> parameters = null, IDataConsumer<T> dataConsumer = null, IDbTransaction transaction = null)
        {
            OperationAllowed(DatabaseOperation.Select);

            var res = new List<T>();

            var consumer = dataConsumer ?? new DataConsumer<T>(NamesProvider, DatabaseColumns, res);

            // TODO: Replace "*" with the list of DbColumns.
            // TODO: Generate the WHERE clausule using parameters.
            Database.ExecuteReaderQuery("SELECT * FROM " + TypeInstance.DataTableName, parameters, dataConsumer.CreateInstance, transaction);

            return res;
        }

        /// <inheritdoc />
        public override void Save(T entity, IDbTransaction transaction = null)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            OperationAllowed(DatabaseOperation.Insert);

            var parameters = CreateInsertParameters(entity);

            // INSERT INTO Lookup (Name, Description) VALUES (@Name, @Description)
            var sb = new StringBuilder("INSERT INTO ");
            
            sb.Append(TypeInstance.DataTableName);

            sb.Append(" (");

            var count = parameters.Count();
            var pos = count;
            foreach (var parameter in parameters)
            {
                sb.Append(parameter.Name);

                pos--;
                if (pos < 1)
                {
                    break;
                }

                sb.Append(",");
            }

            sb.Append(") VALUES (");

            pos = count;
            foreach (var parameter in parameters)
            {
                sb.Append(parameter.DbParameter.ParameterName);

                pos--;
                if (pos < 1)
                {
                    break;
                }

                sb.Append(",");
            }

            sb.Append(")");

            Database.ExecuteNonReaderQuery(sb.ToString(), parameters, transaction);
        }
                
        #endregion


        #region non-public methods

        // Nothing here yet... :-)

        #endregion
    }
}

/*
 
https://stackoverflow.com/questions/2662999/system-data-sqlite-parameterized-queries-with-multiple-values

-------------------

pendingDeletions = new SQLiteCommand(@"DELETE FROM [centres] WHERE [name] = $name", conn);
foreach (string name in selected)
{
    pendingDeletions.Parameters.AddWithValue("$name", centre.Name);
    pendingDeletions.ExecuteNonQuery();
}   

------------------- 

SqlConnection tConn = new SqlConnection("ConnectionString");

SqlCommand tCommand = new SqlCommand();

tCommand.Connection = tConn;
tCommand.CommandText = "UPDATE players SET name = @name, score = @score, active = @active WHERE jerseyNum = @jerseyNum";

tCommand.Parameters.Add(new SqlParameter("@name", System.Data.SqlDbType.VarChar).Value = "Smith, Steve");
tCommand.Parameters.Add(new SqlParameter("@score", System.Data.SqlDbType.Int).Value = "42");
tCommand.Parameters.Add(new SqlParameter("@active", System.Data.SqlDbType.Bit).Value = true);
tCommand.Parameters.Add(new SqlParameter("@jerseyNum", System.Data.SqlDbType.Int).Value = "99");

tCommand.ExecuteNonQuery();
    
*/
