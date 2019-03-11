// Copyright (C) Premysl Fara. All rights reserved.
// SimpleDbTests is available under the zlib license.

namespace SimpleDbTests.Firebird.DataObjects
{
    using SimpleDb.Extensions.Lookups;
    using SimpleDb.Shared;


    /// <summary>
    /// A simple lookup with default colums and column names.
    /// SQLITE supports Int64 only and not Int32.
    /// </summary>
    [DbTable]
    public class Lookup : ALookupEntity<Lookup, int>
    {
        #region ctor

        public Lookup()
        {
            // The description is not required. 
            // Because it is not nullable by default, we set some non-null value here. 
            Description = string.Empty;
        }

        #endregion


        #region properties

        /// <inheritdoc />
        public override bool IsNew
        {
            get
            {
                return Id == 0;
            }
        }

        /// <summary>
        /// The Name column with limited lenght of data. (3 chars only, see the Lookup.sql file.)
        /// </summary>
        [DbStringColumn(IsNonempty = true, MaxLength = 3, Tag = NameColumnTagName)]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }
        
        #endregion
    }
}
