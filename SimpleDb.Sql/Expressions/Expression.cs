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
    public class Expression : IOperand
    {
        #region properties

        /// <summary>
        /// If true, encapsules this expression with parenthesis.
        /// </summary>
        public bool WithPriority { get; private set; }

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
        /// <param name="operand">The operand.</param>
        public Expression(IOperand operand, bool withPriority = false)
        {
            if (operand == null) throw new ArgumentNullException(nameof(operand));

            SetupAndValidate(
                new NopOperator(), 
                new List<IOperand>()
                    {
                        operand
                    },
                withPriority);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="op">An oerator.</param>
        /// <param name="operand">The operand.</param>
        public Expression(IOperator op, IOperand operand, bool withPriority = false)
        {
            if (operand == null) throw new ArgumentNullException(nameof(operand));

            SetupAndValidate(
                op,
                new List<IOperand>()
                    {
                        operand
                    },
                withPriority);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="op">An oerator.</param>
        /// <param name="firstOperand">The first operand.</param>
        /// <param name="secondOperand">The second operand.</param>
        public Expression(IOperator op, IOperand firstOperand, IOperand secondOperand, bool withPriority = false)
        {
            if (firstOperand == null) throw new ArgumentNullException(nameof(firstOperand));
            if (secondOperand == null) throw new ArgumentNullException(nameof(secondOperand));

            SetupAndValidate(
                op, 
                new List<IOperand>()
                    {
                        firstOperand,
                        secondOperand
                    },
                withPriority);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="op">An operator.</param>
        /// <param name="operands">Operands. At least op.MinimimalExpectedOperandsCount of operands.</param>
        public Expression(IOperator op, IReadOnlyList<IOperand> operands, bool withPriority = false)
        {
            SetupAndValidate(op, operands, withPriority);
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
            else if (Operator.MinimimalExpectedOperandsCount == 1)
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

                if (WithPriority) to.Append("(");

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

                if (WithPriority) to.Append(")");
            }
        }

        #endregion


        #region factory methods

        ///// <summary>
        ///// Creates a new expression "expression AND expression".
        ///// Example: Name = :Name
        ///// </summary>
        ///// <param name="firstOperandExpression">An expression.</param>
        ///// <param name="secondOperandExpression">An expression.</param>
        ///// <returns>A new expression.</returns>
        //public static Expression CreateExpression(IOperator op, Expression firstOperandExpression, Expression secondOperandExpression)
        //{
        //    if (op == null) throw new ArgumentNullException(nameof(op));
        //    if (firstOperandExpression == null) throw new ArgumentNullException(nameof(firstOperandExpression));
        //    if (secondOperandExpression == null) throw new ArgumentNullException(nameof(secondOperandExpression));

        //    return new Expression(
        //        op,
        //        firstOperandExpression,
        //        secondOperandExpression);
        //}

        /// <summary>
        /// Creates a new parametrized expression "parameter.Name operator parameter.ParameterName".
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
        private void SetupAndValidate(IOperator op, IReadOnlyList<IOperand> operands, bool withPriority)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (operands == null) throw new ArgumentNullException(nameof(operands));

            if (operands.Count < op.MinimimalExpectedOperandsCount)
            {
                throw new ExpressionException("Not enough expression operands provided.");
            }

            Operator = op;
            Operands = new List<IOperand>(operands);
            WithPriority = withPriority;
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
