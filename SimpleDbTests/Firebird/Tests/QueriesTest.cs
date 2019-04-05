﻿/* SimpleDbTests - (C) 2016 - 2019 Premysl Fara 
 
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

namespace SimpleDbTests.Firebird.Tests
{
    using System;
    using System.Configuration;

    using SimpleDb.Sql;
    using SimpleDbTests.Shared.Datalayer;
    using SimpleDbTests.Shared.DataObjects;
    using System.Linq;
    using SimpleDb.Sql.Expressions;

    public class QueriesTest : ATest
    {
        private Database _database;
        private LookupDataLayer _lookupDataLayer;
        

        protected override void InitializeImplementation()
        {
            //_database = new Database(
            //    ConfigurationManager.ConnectionStrings["SIMPLEDB_FIREBIRD"].ConnectionString,
            //    new SimpleDb.Firebird.DatabaseProvider());

            //var connectionString = SimpleDb.Firebird.DatabaseProvider.CreateDatabase(null, 3050, @"U:\sql\firebird\test.fbd", "SYSDBA", null, 4096, true, true, true);
            var connectionString = SimpleDb.Firebird.DatabaseProvider.CreateEmbeddedDatabase(@"U:\sql\firebird\test.fbd", "NONE", true, 4096, true, true);

            _database = new Database(
                connectionString,
                new SimpleDb.Firebird.DatabaseProvider());

            _database.ExecuteNonQuery(System.Data.CommandType.Text, _createTableQuery, null, null);

            _lookupDataLayer = new LookupDataLayer(_database);
            _lookupDataLayer.UseQueries = true;
        }
        
        private string _createTableQuery = @"CREATE TABLE ""Lookup"" ( 
 ""Id"" INTEGER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
 ""Name"" VARCHAR(3) NOT NULL,
 ""Description"" BLOB SUB_TYPE TEXT NOT NULL,

 CONSTRAINT ""Lookup_Name_UN"" UNIQUE(""Name"")
);";

        public override void RunAllTests()
        {
            ShowTestSettings();

            RunTest(DeleteAllWithQueryFromLookup_Test);

            //RunTest(InsertDefaultEntityToLookup_Test);
            RunTest(InsertEntitiesToLookup_Test);

            RunTest(GetAllFromLookup_Test);
            RunTest(GetIdByNameFromLookup_Test);
            RunTest(UpdateByNameFromLookup_Test);
            RunTest(DeleteselectedFromLookup_Test);

            Console.WriteLine(Stats);
        }

        #region tests

        /// <summary>
        /// Deletes all from the Lookup table and reseed the auto-increase Id column counter.
        /// </summary>
        public void DeleteAllWithQueryFromLookup_Test()
        {
            _lookupDataLayer.Database.ExecuteNonQuery(System.Data.CommandType.Text, "DELETE FROM \"Lookup\"", null);
            _lookupDataLayer.Database.ExecuteNonQuery(System.Data.CommandType.Text, "ALTER TABLE \"Lookup\" ALTER COLUMN \"Id\" RESTART", null);
        }

        ///// <summary>
        ///// Inserts the default entity to the lookup table. 
        ///// </summary>
        //public void InsertDefaultEntityToLookup_Test()
        //{
        //    var entity = new Lookup()
        //    {
        //        Name = "-",
        //        Description = string.Empty
        //    };

        //    _lookupDataLayer.Save(entity);

        //    Console.WriteLine("Default entity saved: {0}", FormatEntity(entity));

        //    AssertEqual(0, entity.Id);
        //}

        /// <summary>
        /// Inserts all Vx entities to the lookup table. 
        /// </summary>
        public void InsertEntitiesToLookup_Test()
        {
            for (var i = 1; i <= 10; i++)
            {
                var entity = new Lookup()
                {
                    Name = "V" + i,
                    Description = "Entity NO. " + i
                };

                _lookupDataLayer.Save(entity);

                Console.WriteLine("Entity saved: {0}", FormatEntity(entity));

                AssertEqual(i, entity.Id);
            }
        }

        /// <summary>
        /// Gets all entities from the Lookup table.
        /// </summary>
        public void GetAllFromLookup_Test()
        {
            var entities = _lookupDataLayer.GetAll();
            foreach (var entity in entities)
            {
                Console.WriteLine(FormatEntity(entity));
            }

            AssertEqual(10, entities.Count());
        }

        /// <summary>
        /// Gets an Id of an entity identified by its name from the Lookup table.
        /// </summary>
        public void GetIdByNameFromLookup_Test()
        {
            var name = "V2";
            var id = _lookupDataLayer.GetIdByName(name);

            Console.WriteLine("The '{0}' Id is: {1}", name, id);

            AssertEqual(2, id);
        }


        /// <summary>
        /// Updates an entity identified by its name from the Lookup table.
        /// </summary>
        public void UpdateByNameFromLookup_Test()
        {
            var name = "V3";
            var id = _lookupDataLayer.GetIdByName(name);

            Console.WriteLine("The '{0}' Id is: {1}", name, id);

            var entity = _lookupDataLayer.Get(id);

            AssertEqual(entity.Id, id);

            entity.Description += " (UPDATED)";

            var newId = _lookupDataLayer.Save(entity);

            Console.WriteLine("The '{0}' new Id is: {1}", name, newId);

            AssertEqual(entity.Id, id);
        }
        
        /// <summary>
        /// Deletes selected entities from the Lookup.
        /// </summary>
        public void DeleteselectedFromLookup_Test()
        {
            _lookupDataLayer.Delete(
                null, 
                Expression.Or(
                    Expression.Equal(
                        Expression.QuotedName("Name"),
                        Expression.Value("V2")),
                    Expression.Equal(
                        Expression.QuotedName("Name"),
                        Expression.Value("V4"))
                ) as Expression,
                null);

            var entities = _lookupDataLayer.GetAll();
            foreach (var entity in entities)
            {
                Console.WriteLine(FormatEntity(entity));
            }

            AssertEqual(8, entities.Count());
        }


        public void MethodTest()
        {
            // https://www.youtube.com/watch?v=kInFI2x7yLY
            // https://www.youtube.com/watch?v=HYrXogLj7vg

            // arrange
            // setup...

            // act
            // tested operation..

            // assert
            // results...
        }

        #endregion


        #region private

        protected void ShowTestSettings()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("Test: Queries Test");
            Console.WriteLine("Database: Firebird");
            Console.WriteLine("Initialized: {0}", Initialized ? "yes" : "no");
            Console.WriteLine("Connection string: {0}", Initialized ? _database.ConnectionString : string.Empty);
            Console.WriteLine("========================================");
            Console.WriteLine();
        }
               
        
        private string FormatEntity(Lookup entity)
        {
            return string.Format("Id: {0}, Name: '{1}', Description: '{2}'", entity.Id, entity.Name, entity.Description);
        }

        #endregion
    }
}
