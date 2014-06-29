using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Moq;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Базовый класс для тестирования <see cref="QueryTransformer"/>.
    /// </summary>
    public abstract class QueryTransformerTestsBase : TestsBase
    {
        internal static IQuery<OneSDataRecord, OneSDataRecord> CreateQuery(string source,
                                                                 Expression<Func<OneSDataRecord, bool>> whereExpression
                                                                     = null,
                                                                 params SortExpression[] orderbyExpressions)
        {
            return CreateQuery<OneSDataRecord>(source, null, whereExpression, orderbyExpressions);
        }

        internal static IQuery<OneSDataRecord, T> CreateQuery<T>(string source,
                                                              Expression<Func<OneSDataRecord, T>> selectExpression,
                                                              Expression<Func<OneSDataRecord, bool>> whereExpression = null,
                                                              params SortExpression[] orderbyExpressions)
        {
            var sourceDescriptionMock = new Mock<ISourceDescription>(MockBehavior.Strict);
            sourceDescriptionMock
                .Setup(sd => sd.GetSourceName(It.IsAny<ISourceResolver>()))
                .Returns(source);

            var queryMock = new Mock<IQuery<OneSDataRecord, T>>(MockBehavior.Strict);
            queryMock
                .SetupGet(q => q.Source)
                .Returns(sourceDescriptionMock.Object);
            queryMock
                .SetupGet(q => q.Selector)
                .Returns(selectExpression);
            queryMock
                .SetupGet(q => q.Filter)
                .Returns(whereExpression);
            queryMock
                .SetupGet(q => q.Sorters)
                .Returns(new ReadOnlyCollection<SortExpression>(orderbyExpressions));

            return queryMock.Object;
        }

        internal static NoSideEffectItemReaderFactory<T> AssertAndCastNoSideEffectItemReaderFactory<T>(
            IItemReaderFactory<T> factory)
        {
            return AssertAndCast<NoSideEffectItemReaderFactory<T>>(factory);
        }

        internal static IQuery<AnyData, AnyData> CreateTupleQuery(
            Expression<Func<AnyData, bool>> filterExpression = null,
            ReadOnlyCollection<SortExpression> sorters = null)
        {
            return CreateTupleQuery<AnyData>(null, filterExpression, sorters);
        }

        internal static IQuery<AnyData, T> CreateTupleQuery<T>(Expression<Func<AnyData, T>> selectExpression,
                                                                  Expression<Func<AnyData, bool>> filterExpression = null,
                                                                  ReadOnlyCollection<SortExpression> sorters = null)
        {
            if (sorters == null)
                sorters = new ReadOnlyCollection<SortExpression>(new SortExpression[0]);

            var queryMock = new Mock<IQuery<AnyData, T>>(MockBehavior.Strict);
            queryMock
                .SetupGet(q => q.Source)
                .Returns(SourceDescriptionByType<AnyData>.Instance);
            queryMock
                .SetupGet(q => q.Selector)
                .Returns(selectExpression);
            queryMock
                .SetupGet(q => q.Filter)
                .Returns(filterExpression);
            queryMock
                .SetupGet(q => q.Sorters)
                .Returns(sorters);

            return queryMock.Object;
        }

        public sealed class AnyData
        {
            public int Id;

            public string Name;

            public decimal Price;
        }
    }
}