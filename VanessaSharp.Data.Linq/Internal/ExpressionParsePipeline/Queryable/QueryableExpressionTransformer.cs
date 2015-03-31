using System;
using System.Linq;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable
{
    /// <summary>
    /// Преобразователь linq-выражения <see cref="IQueryable{T}"/>
    /// в <see cref="IQuery"/>.
    /// </summary>
    internal sealed class QueryableExpressionTransformer : IQueryableExpressionTransformer
    {
        /// <summary>Экземпляр по умолчанию.</summary>
        public static QueryableExpressionTransformer Default
        {
            get { return _default; }
        }
        private static readonly QueryableExpressionTransformer _default = new QueryableExpressionTransformer();

        /// <summary>Преобразование linq-выражения <see cref="IQueryable{T}"/>
        /// в <see cref="IQuery"/>.
        /// </summary>
        /// <param name="expression">Выражение.</param>
        public IQuery Transform(Expression expression)
        {
            var builder = new QueryBuilder();
            IQueryableExpressionHandler handler = new QueryableExpressionPreEvaluator(builder);
            var visitor = new QueryableExpressionVisitor(handler);

            try
            {
                handler.HandleStart();
                visitor.Visit(expression);
                handler.HandleEnd();

                return builder.BuiltQuery;
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
