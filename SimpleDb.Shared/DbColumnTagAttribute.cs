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
    /// An attribute that marks a property with a specific tag.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DbColumnTagAttribute : Attribute
    {
        /// <summary>
        /// A database column tag.
        /// </summary>
        public string Tag { get; }


        /// <summary>
        /// A constructor.
        /// </summary>
        /// <param name="tag">A database column tag.</param>
        public DbColumnTagAttribute(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                throw new ArgumentException("A database table column tag expected.", nameof(tag));
            }

            Tag = tag;
        }
    }
}
