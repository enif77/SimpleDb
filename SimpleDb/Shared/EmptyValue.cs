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
    using System.Collections.Concurrent;


    /// <summary>
    /// Helper class for creating empty values used in SingleSelect controls in WPF
    /// </summary>
    /// <typeparam name="T">Type of BusinessObject</typeparam>
    public static class EmptyValue<T> where T : class, ILookup, new()
    {
        #region fields

        /// <summary>
        /// Storage containers for required and optional values
        /// </summary>
        private static readonly ConcurrentDictionary<Type, object> RequiredValues = new ConcurrentDictionary<Type, object>();
        private static readonly ConcurrentDictionary<Type, object> OptionalValues = new ConcurrentDictionary<Type, object>();

        #endregion

        #region properties

        /// <summary>
        /// Represents empty instance of BusinessObject for Required item
        /// </summary>
        public static T Required
        {
            get
            {
                RequiredValues.TryAdd(typeof(T), new T { Name = "<choose>" });
                return (T)RequiredValues[typeof(T)];
            }
        }

        /// <summary>
        /// Represents empty instance of BusinessObject for Optional item
        /// </summary>
        public static T Optional
        {
            get
            {
                OptionalValues.TryAdd(typeof(T), new T { Name = "<none>" });
                return (T)OptionalValues[typeof(T)];
            }
        }

        #endregion
    }
}
