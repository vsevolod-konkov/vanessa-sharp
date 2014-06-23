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

        /// <summary>
        /// Проверка типа на корректность использования его в виде 
        /// типа записи данных из 1С.
        /// </summary>
        /// <param name="dataType">Тип данных.</param>
        void CheckDataType(Type dataType);
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

        void IExpressionParser.CheckDataType(Type dataType)
        {
            Contract.Requires<ArgumentNullException>(dataType != null);
        }
    }
}
