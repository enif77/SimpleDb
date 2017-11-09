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

namespace SimpleDbTests.Shared.DataObjects
{
    using System;

    using SimpleDb.Shared;

    /// <summary>
    /// This lookup demonstrates, how to rename a column from a default name.
    /// </summary>
    [DbTable]
    public sealed class LookupColumnNames : ALookupEntity<LookupColumnNames, int>
    {
        #region ctor

        // The description is not required. 
        // Because it is not nullable by default, we set some non-null value here.
        public LookupColumnNames()
        {
            Description = String.Empty;
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
        /// The renamed Name column and limited length of data. (3 chars only, see the Lookup.sql file.)
        /// </summary>
        [DbStringColumn("RenamedName", IsNonempty = true, MaxLength = 3, Tag = NameColumnTagName)]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        #endregion
    }
}
