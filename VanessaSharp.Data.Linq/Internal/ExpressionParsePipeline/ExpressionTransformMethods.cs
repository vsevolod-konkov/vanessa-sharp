using System;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Стандартная реализация <see cref="IExpressionTransformMethods"/>.
    /// Методов преобразования LINQ-выражений методов-запросов.
    /// </summary>
    internal sealed class ExpressionTransformMethods : IExpressionTransformMethods
    {
        /// <summary>Экземпляр по умолчанию.</summary>
        public static IExpressionTransformMethods Default
        {
            get { return _default; }
        }
        private static readonly IExpressionTransformMethods _default = new ExpressionTransformMethods();
        
        private ExpressionTransformMethods()
        {}
        
        /// <summary>Преобразование LINQ-выражения метода Select.</summary>
        /// <typeparam name="T">Тип элементов последовательности - результатов выборки.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="selectExpression">Преобразуемое выражение.</param>
        public SelectionPartParseProduct<T> TransformSelectExpression<T>(
            QueryParseContext context, Expression<Func<OneSDataRecord, T>> selectExpression)
        {
            return SelectExpressionTransformer.Transform(context, selectExpression);
        }

        /// <summary>Преобразование выражения в SQL-условие WHERE.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="filterExpression">Фильтрация выражения.</param>
        public SqlCondition TransformWhereExpression(
            QueryParseContext context, Expression<Func<OneSDataRecord, bool>> filterExpression)
        {
            return WhereExpressionTransformer.Transform(context, filterExpression);
        }
    }
}