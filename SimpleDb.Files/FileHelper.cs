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

namespace SimpleDb.Files
{
    using System;
    using System.IO;


    /// <summary>
    /// File operations.
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Loads a file into an array of strings. Each line as a separate string.
        /// </summary>
        /// <param name="fileName">A file name.</param>
        /// <param name="trimNonEmpty">Trims all nonempty lines if true.</param>
        /// <returns>An array of strings or exception.</returns>
        public static string[] LoadDataAsArrayOfStrings(string fileName, bool trimNonEmpty = false)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("A file name expected.", nameof(fileName));

            var data = File.ReadAllLines(fileName);

            // Trim all nonempty lines.
            if (trimNonEmpty)
            {
                for (var i = 0; i < data.Length; i++)
                {
                    if (string.IsNullOrEmpty(data[i]) == false)
                    {
                        data[i] = data[i].Trim();
                    }
                }
            }

            return data;
        }
    }
}
