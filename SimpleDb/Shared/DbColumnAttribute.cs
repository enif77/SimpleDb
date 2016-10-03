/* SimpleDb - (C) 2016 Premysl Fara 
 
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
    /// An attribute describing a DB column property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DbColumnAttribute : Attribute
    {
        /// <summary>
        /// Database column options.
        /// </summary>
        [Flags]
        public enum ColumnOptions
        {
            /// <summary>
            /// No flag set.
            /// </summary>
            None = 0,

            /// <summary>
            /// It is an ID column.
            /// </summary>
            Id = 1,          

            /// <summary>
            /// Can be null.
            /// </summary>
            Nullable = 2,

            /// <summary>
            /// Is read only. The value can not be changed.
            /// </summary>
            ReadOnly = 4,

            /// <summary>
            /// A string value, that can not be empty.
            /// A nonempty string can be null, if it the Nullable column option is used.
            /// See String.IsNullOrWhiteSpace().
            /// </summary>
            Nonempty = 8,

            /// <summary>
            /// Is Ignored. The value is never read from/saved to DB.
            /// </summary>
            Ignored = 128,
        }

        /// <summary>
        /// A DB column name.
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// A maximal allowed length of a string column.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Column options.
        /// </summary>
        public ColumnOptions Options { get; private set; }

        /// <summary>
        /// True, if this column allows the null value.
        /// </summary>
        public bool IsNullable
        {
            get
            {
                return (Options & ColumnOptions.Nullable) == ColumnOptions.Nullable;
            }
        }

        /// <summary>
        /// True, if this column string value can not be empty.
        /// </summary>
        public bool IsNonempty
        {
            get
            {
                return (Options & ColumnOptions.Nonempty) == ColumnOptions.Nonempty;
            }
        }

        /// <summary>
        /// True, if this column allows a DbNull value.
        /// </summary>
        public bool IsId
        {
            get
            {
                return (Options & ColumnOptions.Id) == ColumnOptions.Id;
            }
        }
        
        /// <summary>
        /// True, if this column is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return (Options & ColumnOptions.ReadOnly) == ColumnOptions.ReadOnly;
            }
        }

        /// <summary>
        /// True, if this column is ignored.
        /// </summary>
        public bool IsIgnored
        {
            get
            {
                return (Options & ColumnOptions.Ignored) == ColumnOptions.Ignored;
            }
        }


        /// <summary>
        /// A constructor.
        /// </summary>
        /// <param name="name">A DB column name.</param>
        /// <param name="options">Column options.</param>
        public DbColumnAttribute(string name, ColumnOptions options = ColumnOptions.None)
            : this(name, Int32.MaxValue, options)
        {
        }
        
        /// <summary>
        /// A constructor.
        /// </summary>
        /// <param name="name">A DB column name.</param>
        /// <param name="length">A maximal allowed length of a string column.</param>
        /// <param name="options">Column options.</param>
        public DbColumnAttribute(string name, int length, ColumnOptions options = ColumnOptions.None)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The name argument expected.");
            }

            Name = name;
            Length = length;
            Options = options;
        }
    }
}
