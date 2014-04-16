using System;
using System.Linq;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;

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
            return GetQueryFromQueryableExpression(expression)
                .Transform();
        }

        /// <summary>
        /// Создание объекта запроса из выражения генерируемого <see cref="Queryable"/>.
        /// </summary>
        /// <param name="expression">Выражение, которое необходимо преобразовать в запрос.</param>
        internal static SimpleQuery GetQueryFromQueryableExpression(Expression expression)
        {
            var handler = new SimpleQueryBuilder();
            var visitor = new QueryableExpressionVisitor(handler);

            try
            {
                handler.HandleStart();
                visitor.Visit(expression);
                handler.HandleEnd();

                return handler.BuiltQuery;
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException(string.Format(
                    "Выражение \"{0}\" содержит недопустимую операцию. Подробности: {1}",
                    expression, e.Message), e);
            }
        }
    }
}
