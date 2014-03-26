using System;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Стандартная реализация <see cref="IExpressionParser"/>.</summary>
    internal sealed class ExpressionParser : IExpressionParser
    {
        /// <summary>Экземпляр по умолчанию.</summary>
        public static ExpressionParser Default
        {
            get { return _default; }
        }
        private static readonly ExpressionParser _default = new ExpressionParser();
        
        /// <summary>Разбор выражения.</summary>
        /// <param name="expression">Выражение.</param>
        public ExpressionParseProduct Parse(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}
