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
    /// A base class for a business object.
    /// </summary>
    public abstract class ALookupDataObject<T> : AIdDataObject, ILookup, ICloneable, IUpdatable<T> where T : class, ILookup, new()
    {
        #region public fields

        public static readonly T RequiredValue = EmptyValue<T>.Required;
        public static readonly T OptionalValue = EmptyValue<T>.Optional;

        #endregion


        #region fields

        private string _name;
        private string _description;

        #endregion


        #region properties

        /// <summary>
        /// The Name DB column.
        /// User can change this column name by overriding this property in his own ALookupDataObject implementation.
        /// The DbColumnTag is required by the LookupDataLayer. User should not change it.
        /// </summary>
        [DbColumn("Name", Int32.MaxValue, DbColumnAttribute.ColumnOptions.Nonempty)]
        [DbColumnTag("Name")]
        public virtual string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// The Description DB column.
        /// User can change this column name by overriding this property in his own ALookupDataObject implementation.
        /// The DbColumnTag is not required by the LookupDataLayer. User can change it.
        /// </summary>
        [DbColumn("Description", Int32.MaxValue)]
        [DbColumnTag("Description")]
        public virtual string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        #endregion


        #region public methods

        public override string ToString()
        {
            return Name;
        }


        public virtual object Clone()
        {
            return new T()
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description
            };
        }

        #endregion


        #region IUpdatable<T>

        public virtual bool NeedsUpdate(T source)
        {
            if (Id != source.Id) return true;
            if (Name != source.Name) return true;
            if (Description != source.Description) return true;

            return false;
        }


        public virtual void Update(T source)
        {
            Id = source.Id;
            Name = source.Name;
            Description = source.Description;
        }

        #endregion
    }
}
