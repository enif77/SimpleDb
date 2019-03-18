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

namespace SimpleDb.Sql.Expressions.Operands
{
    using System;
    using System.Text;


    /// <summary>
    /// A WHERE clause operand representing a value.
    /// </summary>
    public class ValueOperand<T> : ABaseOperand
    {
        /// <summary>
        /// A value this operand represents.
        /// </summary>
        public T Value { get; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="v">A value.</param>
        public ValueOperand(T v)
        {
            Value = v;
        }


        /// <summary>
        /// Genarates this operand into an output StringBuilder instance.
        /// </summary>
        /// <param name="to">An output StringBuilder instance.</param>
        public override void Generate(StringBuilder to)
        {
            if (to == null) throw new ArgumentException();

            if (typeof(T) == typeof(string))
            {
                FormatStringValue(to);
            }
            else if (typeof(T) == typeof(bool))
            {
                FormatBooleanValue(to);
            }
            else
            {
                to.Append(Value);
            }
            
        }


        private void FormatStringValue(StringBuilder to)
        {
            to.AppendFormat("'{0}'", Value);
        }


        private void FormatBooleanValue(StringBuilder to)
        {
            to.Append(Value.ToString().ToUpperInvariant());
        }
    }
}
