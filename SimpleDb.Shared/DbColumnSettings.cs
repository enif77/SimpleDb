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

namespace SimpleDb.Shared
{
    /// <summary>
    /// Database column settings generated from a DbColumnAttribute instance.
    /// </summary>
    public class DbColumnSettings
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public DbColumnSettings()
        {
            Name = null;
            Tag = null;
            Length = int.MaxValue;
            IsNullable = false;
            IsNonempty = false;
            IsId = false;
            IsReadOnly = false;
        }

        /// <summary>
        /// A DB column name.
        /// If null, the property name will be used.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A database column tag.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// A maximal allowed length of a string column.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// True, if this column allows the null value.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// True, if this column string value can not be empty.
        /// </summary>
        public bool IsNonempty { get; set; }

        /// <summary>
        /// True, if this column allows a DbNull value.
        /// </summary>
        public bool IsId { get; set; }

        /// <summary>
        /// True, if this column is read only.
        /// </summary>
        public bool IsReadOnly { get; set; }
    }
}
