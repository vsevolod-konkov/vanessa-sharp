using System;
using System.Linq.Expressions;
using Moq;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Базовый класс для тестирования <see cref="QueryTransformer"/>.
    /// </summary>
    public abstract class QueryTransformerTestsBase
    {
        /// <summary>Создание объекта запроса.</summary>
        /// <typeparam name="TInput">
        /// Тип элементов входной последовательности.
        /// </typeparam>
        /// <typeparam name="TOutput">
        /// Тип элементов выходной последовательности.
        /// </typeparam>
        /// <param name="source">Описание источника.</param>
        /// <param name="selector">Выражение выборки.</param>
        /// <param name="filter">Выражение фильтрации.</param>
        /// <param name="sorters">Выражения сортировки.</param>
        internal static IQuery<TInput, TOutput> CreateQuery<TInput, TOutput>(
            ISourceDescription source,
            Expression<Func<TInput, TOutput>> selector = null,
            Expression<Func<TInput, bool>> filter = null,
            params SortExpression[] sorters)
        {
            var queryMock = new Mock<IQuery<TInput, TOutput>>(MockBehavior.Strict);
            queryMock
                .SetupGet(q => q.Source)
                .Returns(source);
            queryMock
                .SetupGet(q => q.Selector)
                .Returns(selector);
            queryMock
                .SetupGet(q => q.Filter)
                .Returns(filter);
            queryMock
                .SetupGet(q => q.Sorters)
                .Returns(sorters.ToReadOnly());

            return queryMock.Object;
        }

        /// <summary>
        /// Тестовая типизированная запись.
        /// </summary>
        public sealed class SomeData
        {
            public int Id;

            public string Name;

            public decimal Price;
        }
    }
}