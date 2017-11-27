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

namespace SimpleDbTests.MsSql.Tests
{
    using System;
    using System.Configuration;

    using SimpleDb.Sql;
    using SimpleDbTests.Shared.Datalayer;


    public class QueriesTest
    {
        private Database _database;
        private LookupDataLayer _lookupDataLayer;
        private bool _initialized = false;


        public void Initialize()
        {
            if (_initialized) throw new InvalidOperationException("Already initialized.");

            _database = new Database(
                ConfigurationManager.ConnectionStrings["SIMPLEDB_MSSQL"].ConnectionString,
                new SimpleDb.MsSql.DatabaseProvider(new SimpleDbTests.MsSql.NamesProvider()));

            _lookupDataLayer = new LookupDataLayer(_database);
            _lookupDataLayer.UseQueries = true;

            _initialized = true;
        }


        public void ShowTestSettings()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("Test: Queries Test");
            Console.WriteLine("Database: MSSQL");
            Console.WriteLine("Initialized: {0}", _initialized ? "yes" : "no");
            Console.WriteLine("Connection string: {0}", _initialized ? _database.ConnectionString : string.Empty);
            Console.WriteLine("========================================");

        }


        public void GetAllTest()
        {
            CheckInitialized();

            foreach (var lookup in _lookupDataLayer.GetAll())
            {
                Console.WriteLine("Id: {0}, Name: '{1}', Description: '{2}'", lookup.Id, lookup.Name, lookup.Description);
            }
        }


        public void GetIdByNameTest()
        {
            CheckInitialized();

            var name = "V2";
            var id = _lookupDataLayer.GetIdByName(name);

            Console.WriteLine("The '{0}' Id is: {1}", name, id);
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


        #region private

        private void CheckInitialized()
        {
            if (_initialized == false) throw new InvalidOperationException("Not initialized.");
        }

        #endregion
    }
}
