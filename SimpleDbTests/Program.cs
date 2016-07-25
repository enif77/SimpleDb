/* SimpleDbTests - (C) 2016 Premysl Fara 
 
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

namespace SimpleDbTests
{
    using System;
    using System.Configuration;

    using Injektor;
    using SimpleDb.Files;
    using SimpleDb.MsSql;

    using SimpleDbTests.Files.Datalayer;
    using SimpleDbTests.MsSql.Datalayer;


    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                FilesTests();
                MsSqlTests();
            }
            finally 
            {
                Console.WriteLine();
                Console.WriteLine("DONE");
                Console.ReadLine();
            }
        }


        #region FILES

        private static void FilesTests()
        {
            Files.Datalayer.Initializer.InitializeLayers(new SimpleDb.Files.Database(@"D:\Devel\Projects\Personal\CS\git\SimpleDb\SimpleDbTests\Files\Data"));

            FilesLookupDataLayerTest();
            //MsSqlLookupColumnNamesDatalayerTest();
        }


        private static void FilesLookupDataLayerTest()
        {
            var dal = Registry.Get<Files.Datalayer.LookupDataLayer>();
            var lookups = dal.GetAll();
            foreach (var lookup in lookups)
            {
                Console.WriteLine("Id: {0}, Name: '{1}', Description: '{2}'", lookup.Id, lookup.Name, lookup.Description);
            }

            var name = "V1";
            var id = dal.GetIdByName(name);
            Console.WriteLine("The '{0}' Id is: {1}", name, id);
        }

        #endregion


        #region MSSQL

        private static void MsSqlTests()
        {
            MsSql.Datalayer.Initializer.InitializeLayers(new SimpleDb.MsSql.Database(ConfigurationManager.ConnectionStrings["SIMPLEDB"].ConnectionString));

            MsSqlLookupDataLayerTest();
            MsSqlLookupColumnNamesDatalayerTest();
        }


        private static void MsSqlLookupDataLayerTest()
        {
            var dal = Registry.Get<MsSql.Datalayer.LookupDataLayer>();
            var lookups = dal.GetAll();
            foreach (var lookup in lookups)
            {
                Console.WriteLine("Id: {0}, Name: '{1}', Description: '{2}'", lookup.Id, lookup.Name, lookup.Description);
            }

            var name = "V1";
            var id = dal.GetIdByName(name);
            Console.WriteLine("The '{0}' Id is: {1}", name, id);
        }


        private static void MsSqlLookupColumnNamesDatalayerTest()
        {
            var dal = Registry.Get<MsSql.Datalayer.LookupColumnNamesDataLayer>();
            var lookups = dal.GetAll();
            foreach (var lookup in lookups)
            {
                Console.WriteLine("Id: {0}, Name: '{1}', Description: '{2}'", lookup.Id, lookup.Name, lookup.Description);
            }
        }

        #endregion
    }
}
