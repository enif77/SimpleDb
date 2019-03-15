﻿/* SimpleDb - (C) 2016 - 2019 Premysl Fara 
 
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

namespace SimpleDb.Sql.Expressions.Operators
{
    using System.Text;
    

    // SEE: https://www.w3schools.com/sql/sql_where.asp

    /// <summary>
    /// The base of all operators.
    /// </summary>
    public abstract class ABaseOperator : IOperator
    {
        /// <summary>
        /// The minimal expected number of operands this operator requires.
        /// The default is 2.
        /// </summary>
        public virtual int MinimimalExpectedOperandsCount { get { return 2; } }


        /// <summary>
        /// Generates a string representation of this operand a provided StringBuilder instance.
        /// </summary>
        /// <param name="to">An output StringBuilder instance.</param>
        public abstract void Generate(StringBuilder to);
    }
}
