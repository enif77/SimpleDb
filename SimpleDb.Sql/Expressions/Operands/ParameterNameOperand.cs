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

namespace SimpleDb.Sql.Expressions.Operands
{
    using System;
    using System.Text;


    /// <summary>
    /// A WHERE clause operand representing a query parameter name.
    /// </summary>
    public class ParameterNameOperand : ABaseOperand
    {
        /// <summary>
        /// A query parameter this operand represents.
        /// </summary>
        public NamedDbParameter Parameter { get; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameter">A query parameter.</param>
        public ParameterNameOperand(NamedDbParameter parameter)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));

            Parameter = parameter;
        }


        /// <summary>
        /// Genarates this operand into an output StringBuilder instance.
        /// </summary>
        /// <param name="to">An output StringBuilder instance.</param>
        public override void Generate(StringBuilder to)
        {
            if (to == null) throw new ArgumentException();

            to.Append(Parameter.Name);
            to.Append(" = ");
            to.Append(Parameter.DbParameter.ParameterName);
        }
    }
}
