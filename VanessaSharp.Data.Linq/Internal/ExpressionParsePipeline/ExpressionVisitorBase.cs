using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Базовый класс всех посетителей со вспомогательными 
    /// методами.
    /// </summary>
    internal abstract class ExpressionVisitorBase : ExpressionVisitor
    {
        /// <summary>Создание исключения, что выражение не поддерживается.</summary>
        /// <param name="expression">Неподдерживаемое выражение.</param>
        protected static Exception CreateExpressionNotSupportedException(Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            return new NotSupportedException(string.Format("Выражение \"{0}\" не поддерживается.", expression));
        }

        /// <summary>Получение константы.</summary>
        /// <typeparam name="T">Тип константы.</typeparam>
        /// <param name="expression">Выражение.</param>
        protected static T GetConstant<T>(Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            var constExpression = expression as ConstantExpression;
            if (constExpression == null)
                throw CreateExpressionNotSupportedException(expression);

            return (T)constExpression.Value;
        }
    }
}
