/* SimpleDbTests - (C) 2016 - 2019 Premysl Fara 
 
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
    using SimpleDb.Extensions.Lookups;
    using SimpleDb.Files;
    using SimpleDb.Sql;

    using SimpleDbTests.Files.Datalayer;
    using SimpleDb.MsSql;
    using SimpleDb.MySql;
    using SimpleDb.Sql.Expressions;
    using SimpleDb.Sql.Expressions.Operators;
    using SimpleDb.Sql.Expressions.Operands;
    using System.Text;
    using System.Collections.Generic;

    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //ExpressionsTests();

                //FilesTests();
                //MsSqlTests();
                //MySqlTests();
                //PgSqlTests();
                //SqLiteTests();
                FirebirdTests();
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("DONE");
                Console.ReadLine();
            }
        }


        #region Expressions

        private static void ExpressionsTests()
        {

            /*
             
            ParameterExpression param = (ParameterExpression)exprTree.Parameters[0];  
            BinaryExpression operation = (BinaryExpression)exprTree.Body;  
            ParameterExpression left = (ParameterExpression)operation.Left;  
            ConstantExpression right = (ConstantExpression)operation.Right;  
             
             */


            // ("a op" < 5 OR C > D OR E = 'magor') AND NOT (E <> FALSE)
            var exp = Expression.And(
                Expression.Priority(
                    Expression.Or(
                        new List<IOperand>()
                        {
                            Expression.LessThan(Expression.QuotedName("a op"), Expression.Value(5)),
                            Expression.GreaterThan(Expression.Name("c"), Expression.Name("d")),
                            Expression.Equal(Expression.Name("e"), Expression.Value("magor"))
                        }
                    )
                ),
                Expression.Not(
                    Expression.Priority(
                        Expression.NotEqual(Expression.Name("e"), Expression.Value(false))
                    )
                )
            );

            var sb = new StringBuilder();

            exp.Generate(sb);

            Console.WriteLine(sb.ToString());
        }

        #endregion


        #region FILES

        private static void FilesTests()
        {
            Files.Datalayer.Initializer.InitializeLayers(new SimpleDb.Files.Database(@"W:\Devel\Projects\CS\git\SimpleDb\SimpleDbTests\Files\Data"));

            FilesLookupDataLayerTest();
            //MsSqlLookupColumnNamesDatalayerTest();
        }


        private static void FilesLookupDataLayerTest()
        {
            var dal = Registry.Get<Files.Datalayer.LookupDataLayer>();
            foreach (var lookup in dal.GetAll())
            {
                Console.WriteLine("Id: {0}, Name: '{1}', Description: '{2}'", lookup.Id, lookup.Name, lookup.Description);
            }

            var name = "V1";
            var id = dal.GetIdByName(name);
            Console.WriteLine("The '{0}' Id is: {1}", name, id);
        }

        #endregion


        #region MsSQL
        
        private static void MsSqlTests()
        {
            var test = new MsSql.Tests.QueriesTest();

            test.Initialize();
            test.RunAllTests();
        }
        

        //private static void MsSqlLookupColumnNamesDatalayerTest()
        //{
        //    var dal = Registry.Get<Shared.Datalayer.LookupColumnNamesDataLayer>();

        //    dal.UseQueries = true;

        //    foreach (var lookup in dal.GetAll())
        //    {
        //        Console.WriteLine("Id: {0}, Name: '{1}', Description: '{2}'", lookup.Id, lookup.Name, lookup.Description);
        //    }
        //}

        #endregion


        #region MySQL

        private static void MySqlTests()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("MySQL");
            Console.WriteLine("========================================");

            Shared.Datalayer.Initializer.InitializeLayers(
                new SimpleDb.Sql.Database(
                    ConfigurationManager.ConnectionStrings["SIMPLEDB_MYSQL"].ConnectionString,
                    new SimpleDb.MySql.DatabaseProvider(new SimpleDbTests.MySql.NamesProvider())));

            MySqlLookupDataLayerTest();
            MySqlLookupColumnNamesDatalayerTest();
        }


        private static void MySqlLookupDataLayerTest()
        {
            var dal = Registry.Get<Shared.Datalayer.LookupDataLayer>();

            dal.UseQueries = true;

            foreach (var lookup in dal.GetAll())
            {
                Console.WriteLine("Id: {0}, Name: '{1}', Description: '{2}'", lookup.Id, lookup.Name, lookup.Description);
            }

            var name = "V2";
            var id = dal.GetIdByName(name);
            Console.WriteLine("The '{0}' Id is: {1}", name, id);
        }


        private static void MySqlLookupColumnNamesDatalayerTest()
        {
            var dal = Registry.Get<Shared.Datalayer.LookupColumnNamesDataLayer>();
            foreach (var lookup in dal.GetAll())
            {
                Console.WriteLine("Id: {0}, Name: '{1}', Description: '{2}'", lookup.Id, lookup.Name, lookup.Description);
            }
        }

        #endregion


        #region PgSQL

        private static void PgSqlTests()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("PgSQL");
            Console.WriteLine("========================================");

            Shared.Datalayer.Initializer.InitializeLayers(
                new SimpleDb.Sql.Database(
                    ConfigurationManager.ConnectionStrings["SIMPLEDB_PGSQL"].ConnectionString,
                    new SimpleDb.PgSql.DatabaseProvider()));

            PgSqlLookupDataLayerTest();
            //PgSqlLookupColumnNamesDatalayerTest();
        }


        private static void PgSqlLookupDataLayerTest()
        {
            var dal = Registry.Get<Shared.Datalayer.LookupDataLayer>();

            dal.UseQueries = true;

            foreach (var lookup in dal.GetAll())
            {
                Console.WriteLine("Id: {0}, Name: '{1}', Description: '{2}'", lookup.Id, lookup.Name, lookup.Description);
            }

            var name = "V2";
            var id = dal.GetIdByName(name);
            Console.WriteLine("The '{0}' Id is: {1}", name, id);
        }


        //private static void PgSqlLookupColumnNamesDatalayerTest()
        //{
        //    var dal = Registry.Get<Shared.Datalayer.LookupColumnNamesDataLayer>();
        //    foreach (var lookup in dal.GetAll())
        //    {
        //        Console.WriteLine("Id: {0}, Name: '{1}', Description: '{2}'", lookup.Id, lookup.Name, lookup.Description);
        //    }
        //}

        #endregion


        #region SqLite

        private static void SqLiteTests()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("SQLITE");
            Console.WriteLine("========================================");

            SqLite.Datalayer.Initializer.InitializeLayers(
                new SimpleDb.Sql.Database(
                    ConfigurationManager.ConnectionStrings["SIMPLEDB_SQLITE"].ConnectionString,
                    new SimpleDb.SqLite.DatabaseProvider()));

            SqLiteLookupDataLayerTest();
            //SqLiteLookupColumnNamesDatalayerTest();
        }


        private static void SqLiteLookupDataLayerTest()
        {
            var dal = Registry.Get<SqLite.Datalayer.LookupDataLayer>();
            foreach (var lookup in dal.GetAll())
            {
                Console.WriteLine("Id: {0}, Name: '{1}', Description: '{2}'", lookup.Id, lookup.Name, lookup.Description);
            }

            var name = "V2";
            var id = dal.GetIdByName(name);
            Console.WriteLine("The '{0}' Id is: {1}", name, id);
        }


        //private static void SqLiteLookupColumnNamesDatalayerTest()
        //{
        //    var dal = Registry.Get<Shared.Datalayer.LookupColumnNamesDataLayer>();
        //    foreach (var lookup in dal.GetAll())
        //    {
        //        Console.WriteLine("Id: {0}, Name: '{1}', Description: '{2}'", lookup.Id, lookup.Name, lookup.Description);
        //    }
        //}

        #endregion


        #region Firebird

        // https://csharp.hotexamples.com/examples/FirebirdSql.Data.FirebirdClient/FbConnectionStringBuilder/-/php-fbconnectionstringbuilder-class-examples.html

        private static void FirebirdTests()
        {
            var test = new Firebird.Tests.QueriesTest();

            test.Initialize();
            test.RunAllTests();
        }

        //private static void FirebirdTests()
        //{
        //    Console.WriteLine("========================================");
        //    Console.WriteLine("FIREBIRD");
        //    Console.WriteLine("========================================");

        //    Firebird.Datalayer.Initializer.InitializeLayers(
        //        new SimpleDb.Sql.Database(
        //            ConfigurationManager.ConnectionStrings["SIMPLEDB_FIREBIRD"].ConnectionString,
        //            new SimpleDb.Firebird.DatabaseProvider()));

        //    FirebirdLookupDataLayerTest();
        //}


        //private static void FirebirdLookupDataLayerTest()
        //{
        //    var dal = Registry.Get<Firebird.Datalayer.LookupDataLayer>();
        //    foreach (var lookup in dal.GetAll())
        //    {
        //        Console.WriteLine("Id: {0}, Name: '{1}', Description: '{2}'", lookup.Id, lookup.Name, lookup.Description);
        //    }

        //    var name = "V2";
        //    var id = dal.GetIdByName(name);
        //    Console.WriteLine("The '{0}' Id is: {1}", name, id);
        //}

        #endregion
    }
}

/*

set term #;  
  
INSERT INTO "Lookup" ("Id", "Name", "Description") VALUES (1, 'V1', 'The first value')#
INSERT INTO "Lookup" ("Id", "Name", "Description") VALUES (2, 'V2', 'The second value')#
INSERT INTO "Lookup" ("Id", "Name", "Description") VALUES (3, 'V3', 'The third value')#
INSERT INTO "Lookup" ("Id", "Name", "Description") VALUES (4, 'V4', 'The fourth value')#
INSERT INTO "Lookup" ("Id", "Name", "Description") VALUES (5, 'V5', 'The fifth value')#


CREATE TABLE "Lookup"
(
   "Name" varchar(3) NOT NULL,
   "Id" integer generated by default as identity PRIMARY KEY NOT NULL,
   "Description" blob sub_type text NOT NULL
)
#
CREATE UNIQUE INDEX "Lookup_Name_UN" ON "Lookup"("Name")
#
--CREATE UNIQUE INDEX "Lookup_Id_PK" ON "Lookup"("Id")
--#

set term ;#

*/
