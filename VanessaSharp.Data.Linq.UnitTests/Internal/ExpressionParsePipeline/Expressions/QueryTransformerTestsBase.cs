using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
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
        internal static CustomDataTypeQuery<T> CreateQuery<T>(string source,
                                                              Expression<Func<OneSDataRecord, T>> selectExpression,
                                                              Expression<Func<OneSDataRecord, bool>> whereExpression = null,
                                                              IList<SortExpression> orderbyExpressions = null)
        {
            return new CustomDataTypeQuery<T>(source, whereExpression, 
                new ReadOnlyCollection<SortExpression>(orderbyExpressions ?? new SortExpression[0]), selectExpression);
        }

        internal static NoSideEffectItemReaderFactory<T> AssertAndCastNoSideEffectItemReaderFactory<T>(
            IItemReaderFactory<T> factory)
        {
            return AssertAndCast<NoSideEffectItemReaderFactory<T>>(factory);
        }

        internal static TupleQuery<AnyData, T> CreateTupleQuery<T>(Expression<Func<AnyData, T>> selectExpression = null,
                                                                  Expression<Func<AnyData, bool>> filterExpression = null,
                                                                  ReadOnlyCollection<SortExpression> sorters = null)
        {
            if (sorters == null)
                sorters = new ReadOnlyCollection<SortExpression>(new SortExpression[0]);

            return new TupleQuery<AnyData, T>(selectExpression, filterExpression, sorters);
        }

        public sealed class AnyData
        {
            public int Id;

            public string Name;

            public decimal Price;
        }
    }
}