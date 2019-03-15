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

namespace SimpleDb.Sql.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using SimpleDb.Sql.Expressions.Operands;
    using SimpleDb.Sql.Expressions.Operators;


    /// <summary>
    /// Represents a WHERE clause expression.
    /// </summary>
    public class Expression : IExpressionBuilder
    {
        #region properties

        /// <summary>
        /// An operator used in this expression.
        /// </summary>
        public IOperator Operator { get; private set; }

        /// <summary>
        /// The list of operands used by this expression.
        /// </summary>
        public IEnumerable<IOperand> Operands
        {
            get
            {
                return new List<IOperand>(_operands);
            }

            private set
            {
                _operands = value;
            }
        }
        
        #endregion


        #region ctor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="op">An oerator.</param>
        /// <param name="firstOperand">The first operand.</param>
        /// <param name="secondOperand">The second operand.</param>
        public Expression(IOperator op, IOperand firstOperand, IOperand secondOperand)
        {
            if (firstOperand == null) throw new ArgumentNullException(nameof(firstOperand));
            if (secondOperand == null) throw new ArgumentNullException(nameof(secondOperand));

            SetupAndValidate(op, new List<IOperand>()
            {
                firstOperand,
                secondOperand
            });
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="op">An operator.</param>
        /// <param name="operands">Operands. At least op.MinimimalExpectedOperandsCount of operands.</param>
        public Expression(IOperator op, IReadOnlyList<IOperand> operands)
        {
            SetupAndValidate(op, operands);
        }

        #endregion


        #region public methods

        /// <summary>
        /// Generates a string representation of this expression into a provided StringBuilder instance.
        /// </summary>
        /// <param name="to">An output StringBuilder instance.</param>
        public void Generate(StringBuilder to)
        {
            if (to == null) throw new ArgumentNullException(nameof(to));

            if (Operator.MinimimalExpectedOperandsCount == 0)
            {
                // NOT
                GenerateSeparator(to);
                Operator.Generate(to);
            }
            else if (Operator.MinimimalExpectedOperandsCount < 2)
            {
                // NOT a
                GenerateSeparator(to);
                Operator.Generate(to);
                GenerateSeparator(to);
                Operands.First().Generate(to);
            }
            else
            {
                // a AND b AND c
                var count = ((List<IOperand>)Operands).Count;
                foreach (var operand in Operands)
                {
                    GenerateSeparator(to);
                    operand.Generate(to);

                    count--;
                    if (count > 0)
                    {
                        GenerateSeparator(to);
                        Operator.Generate(to);
                    }
                }
            }
        }

        #endregion


        #region factory methods

        /// <summary>
        /// Creates a new parametrized expression "parameter.Name operand parameter.ParameterName".
        /// Example: Name = :Name
        /// </summary>
        /// <param name="op">An operator.</param>
        /// <param name="parameter">A DB parameter.</param>
        /// <returns>A new expression.</returns>
        public static Expression CreateParametrizedExpression(IOperator op, NamedDbParameter parameter)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (parameter == null) throw new ArgumentNullException(nameof(op));

            return new Expression(
                op,
                new NameOperand(parameter.Name),
                new NameOperand(parameter.DbParameter.ParameterName));
        }

        #endregion


        #region private

        private IEnumerable<IOperand> _operands;
        

        /// <summary>
        /// Validates and sets up this expression.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="operands"></param>
        private void SetupAndValidate(IOperator op, IReadOnlyList<IOperand> operands)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (operands == null) throw new ArgumentNullException(nameof(operands));

            if (operands.Count < op.MinimimalExpectedOperandsCount)
            {
                throw new ExpressionException("Not enough expression operands provided.");
            }

            Operator = op;
            Operands = new List<IOperand>(operands);
        }

        /// <summary>
        /// Generates an WHERE expression separator.
        /// </summary>
        /// <param name="to">An output StringBuilder instance.</param>
        private void GenerateSeparator(StringBuilder to)
        {
            to.Append(" ");
        }

        #endregion
    }
}
