// Copyright (C) Premysl Fara. All rights reserved.
// SimpleDbTests is available under the zlib license.

namespace SimpleDbTests.Firebird.Datalayer
{
    using System;

    using Injektor;
    using SimpleDb.Sql;
    

    /// <summary>
    /// Global class for registering and initializing data layers.
    /// </summary>
    public static class Initializer
    {
        /// <summary>
        /// Initializes and registers all data layers.
        /// </summary>
        /// <param name="database"></param>
        public static void InitializeLayers(Database database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            Registry.RegisterInstance(new LookupDataLayer(database));
        }
    }
}
