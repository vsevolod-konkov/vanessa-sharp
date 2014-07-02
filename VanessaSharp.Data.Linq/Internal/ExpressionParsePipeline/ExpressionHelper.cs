using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Вспомогетельные методы для выражений.
    /// </summary>
    internal static class ExpressionHelper
    {
        /// <summary>Создание исключения, что выражение не поддерживается.</summary>
        /// <param name="expression">Неподдерживаемое выражение.</param>
        public static Exception CreateExpressionNotSupportedException(this Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            return new NotSupportedException(string.Format(
                "Выражение \"{0}\" не поддерживается.", expression));
        }

        /// <summary>Получение константы.</summary>
        /// <typeparam name="T">Тип константы.</typeparam>
        /// <param name="expression">Выражение.</param>
        public static T GetConstant<T>(this Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            var constExpression = expression as ConstantExpression;
            if (constExpression == null)
                throw expression.CreateExpressionNotSupportedException();

            return (T)constExpression.Value;
        }
    }
}
