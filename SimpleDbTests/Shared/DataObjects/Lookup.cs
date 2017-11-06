﻿/* SimpleDbTests - (C) 2016 Premysl Fara 
 
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

namespace SimpleDbTests.Shared.DataObjects
{
    using System;

    using SimpleDb.Shared;

    /// <summary>
    /// A simple lookup with default colums and column names.
    /// </summary>
    [DbTable("Lookup")]
    public sealed class Lookup : ALookupEntity<Lookup, int>
    {
        #region ctor

        public Lookup()
        {
            // The description is not required. 
            // Because it is not nullable by default, we set some non-null value here. 
            Description = String.Empty;
        }

        #endregion


        #region properties

        /// <summary>
        /// The Name column with limited lenght of data. (3 chars only, see the Lookup.sql file.)
        /// </summary>
        [DbColumn("Name", 3)]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        #endregion
    }
}
