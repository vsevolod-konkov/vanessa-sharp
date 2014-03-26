using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>
    /// Интерфейс разборщика выражений.
    /// </summary>
    [ContractClass(typeof(IExpressionParserContract))]
    internal interface IExpressionParser
    {
        /// <summary>Разбор выражения.</summary>
        /// <param name="expression">Выражение.</param>
        ExpressionParseProduct Parse(Expression expression);
    }

    [ContractClassFor(typeof(IExpressionParser))]
    internal abstract class IExpressionParserContract : IExpressionParser
    {
        ExpressionParseProduct IExpressionParser.Parse(Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Ensures(Contract.Result<ExpressionParseProduct>() != null);

            return null;
        }
    }
}
