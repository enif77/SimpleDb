/* SimpleDb - (C) 2016 - 2019 Premysl Fara 
 
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

namespace SimpleDb.Sql
{
    using System.Data.Common;


    /// <summary>
    /// A databese parameter with names.
    /// </summary>
    public class NamedDbParameter
    {
        /// <summary>
        /// The original, untranslated, parameter name.
        /// </summary>
        public string BaseName { get; set; }

        /// <summary>
        /// The translated parameter name.
        /// </summary>
        public string Name { get; set; }
         
        /// <summary>
        /// True, if this parameter is an Id.
        /// </summary>
        public bool IsId { get; set; }

        /// <summary>
        /// The DbParameter instance.
        /// </summary>
        public DbParameter DbParameter { get; set; }
    }
}
