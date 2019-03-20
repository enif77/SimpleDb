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


    // https://docs.microsoft.com/cs-cz/dotnet/csharp/programming-guide/concepts/expression-trees/


    /// <summary>
    /// Represents a WHERE clause expression.
    /// </summary>
    public class Expression : IOperand
    {
        #region properties

        /// <summary>
        /// An operator used in this expression.
        /// </summary>
        public IOperator Operator { get; private set; }

        /// <summary>
        /// The list of operands used by this expression.
        /// </summary>
        public IList<IOperand> Operands
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
        /// <param name="operand">The operand.</param>
        public Expression(IOperator op, IOperand operand)
        {
            if (operand == null) throw new ArgumentNullException(nameof(operand));

            SetupAndValidate(
                op,
                new List<IOperand>()
                    {
                        operand
                    });
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="op">An oerator.</param>
        /// <param name="firstOperand">The first operand.</param>
        public Expression(IOperator op, IOperand firstOperand, IOperand secondOperand)
        {
            if (firstOperand == null) throw new ArgumentNullException(nameof(firstOperand));
            if (secondOperand == null) throw new ArgumentNullException(nameof(secondOperand));

            SetupAndValidate(
                op,
                new List<IOperand>()
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
                throw new NotImplementedException();
            }
            else
            if (Operator.MinimimalExpectedOperandsCount == 1)
            {
                // NOT and ( ... )
                var withPriority = Operator is PriorityOperator;
                if (withPriority)
                {
                    to.Append("(");
                }
                else
                {
                    Operator.Generate(to);
                    GenerateSeparator(to);
                }
                                                
                Operands.First().Generate(to);

                if (withPriority)
                {
                    to.Append(")");
                }
            }
            else
            {
                // a AND b AND c
                var expStart = true;
                var remainingOpsCount = Operands.Count;
                foreach (var operand in Operands)
                {
                    if (expStart)
                    {
                        operand.Generate(to);
                        expStart = false;
                    }
                    else
                    {
                        GenerateSeparator(to);
                        operand.Generate(to);
                    }

                    remainingOpsCount--;
                    if (remainingOpsCount > 0)
                    {
                        GenerateSeparator(to);
                        Operator.Generate(to);
                    }
                }
            }
        }

        #endregion


        #region factory methods

        #region operand expressions

        /// <summary>
        /// Creates a new operand for a name.
        /// </summary>
        /// <param name="name">A name.</param>
        /// <returns>An IOperand instance representing a name.</returns>
        public static IOperand Name(string name)
        {
            return new NameOperand(name);
        }

        /// <summary>
        /// Creates a new operand for a quoted name.
        /// </summary>
        /// <param name="name">A name.</param>
        /// <returns>An IOperand instance representing a quoted name.</returns>
        public static IOperand QuotedName(string name)
        {
            return new QuotedNameOperand(name);
        }

        /// <summary>
        /// Creates a new operand for a value.
        /// </summary>
        /// <param name="v">A value.</param>
        /// <returns>An IOperand instance representing a value.</returns>
        public static IOperand Value<T>(T v)
        {
            return new ValueOperand<T>(v);
        }

        #endregion


        #region operator expressions

        /// <summary>
        /// Creates a new expression for the AND operator.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand And(IOperand a, IOperand b)
        {
            return new Expression(new AndOperator(), a, b);
        }

        /// <summary>
        /// Creates a new expression for the AND operator.
        /// </summary>
        /// <param name="operands">The list of operands. At least two.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand And(IReadOnlyList<IOperand> operands)
        {
            return new Expression(new AndOperator(), operands);
        }

        /// <summary>
        /// Creates a new expression for the '=' operator.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand Equal(IOperand a, IOperand b)
        {
            return new Expression(new EqualOperator(), a, b);
        }

        /// <summary>
        /// Creates a new expression for the '=' operator.
        /// </summary>
        /// <param name="operands">The list of operands. At least two.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand Equal(IReadOnlyList<IOperand> operands)
        {
            return new Expression(new EqualOperator(), operands);
        }

        /// <summary>
        /// Creates a new expression for the '>' operator.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand GreaterThan(IOperand a, IOperand b)
        {
            return new Expression(new GreaterThanOperator(), a, b);
        }

        /// <summary>
        /// Creates a new expression for the '>' operator.
        /// </summary>
        /// <param name="operands">The list of operands. At least two.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand GreaterThan(IReadOnlyList<IOperand> operands)
        {
            return new Expression(new GreaterThanOperator(), operands);
        }

        /// <summary>
        /// Creates a new expression for the '>=' operator.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand GreaterThanOrEqual(IOperand a, IOperand b)
        {
            return new Expression(new GreaterThanOrEqualOperator(), a, b);
        }

        /// <summary>
        /// Creates a new expression for the '>=' operator.
        /// </summary>
        /// <param name="operands">The list of operands. At least two.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand GreaterThanOrEqual(IReadOnlyList<IOperand> operands)
        {
            return new Expression(new GreaterThanOrEqualOperator(), operands);
        }

        /// <summary>
        /// Creates a new expression for the '<' operator.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand LessThan(IOperand a, IOperand b)
        {
            return new Expression(new LessThanOperator(), a, b);
        }

        /// <summary>
        /// Creates a new expression for the '<' operator.
        /// </summary>
        /// <param name="operands">The list of operands. At least two.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand LessThan(IReadOnlyList<IOperand> operands)
        {
            return new Expression(new LessThanOperator(), operands);
        }

        /// <summary>
        /// Creates a new expression for the NOT operator.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand Not(IOperand a)
        {
            return new Expression(new NotOperator(), a);
        }

        /// <summary>
        /// Creates a new expression for the '<>' operator.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand NotEqual(IOperand a, IOperand b)
        {
            return new Expression(new NotEqualOperator(), a, b);
        }

        /// <summary>
        /// Creates a new expression for the '<>' operator.
        /// </summary>
        /// <param name="operands">The list of operands. At least two.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand NotEqual(IReadOnlyList<IOperand> operands)
        {
            return new Expression(new NotEqualOperator(), operands);
        }
        
        /// <summary>
        /// Creates a new expression for the OR operator.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand Or(IOperand a, IOperand b)
        {
            return new Expression(new OrOperator(), a, b);
        }

        /// <summary>
        /// Creates a new expression for the OR operator.
        /// </summary>
        /// <param name="operands">The list of operands. At least two.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand Or(IReadOnlyList<IOperand> operands)
        {
            return new Expression(new OrOperator(), operands);
        }
        
        /// <summary>
        /// Creates a new expression for the ( ... ) operator.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <returns>An IOperand instance representing the new expression.</returns>
        public static IOperand Priority(IOperand a)
        {
            return new Expression(new PriorityOperator(), a);
        }

        #endregion


        /// <summary>
        /// Creates a new expression for a database parameter "parameter.Name operator parameter.ParameterName".
        /// Example: Name = :Name
        /// </summary>
        /// <param name="op">An operator.</param>
        /// <param name="parameter">A database parameter instance.</param>
        /// <returns>A new expression.</returns>
        public static Expression ParameterExpression(IOperator op, NamedDbParameter parameter)
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
