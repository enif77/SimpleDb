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
    using System;


    /// <summary>
    /// An attribute describing a database column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DbColumnAttribute : Attribute
    {
        /// <summary>
        /// A DB column name.
        /// If null, the property name will be used.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A database column tag.
        /// Used by the LookupDataLayer to find the Name column. Users can use it freely.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// True, if this column allows the null value.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// True, if this column is an Id.
        /// </summary>
        public bool IsId { get; set; }

        /// <summary>
        /// True, if this column is read only. Id colums are read only allways.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// A constructor.
        /// </summary>
        /// <param name="name">A DB column name.</param>
        public DbColumnAttribute(string name = null)
        {
            Tag = null;
            IsNullable = false;
            IsId = false;
            IsReadOnly = false;

            Name = name;
        }
    }
}
