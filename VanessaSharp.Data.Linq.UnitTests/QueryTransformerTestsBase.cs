using System;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>
    /// Базовый класс для тестирования <see cref="QueryTransformer"/>.
    /// </summary>
    public abstract class QueryTransformerTestsBase : TestsBase
    {
        internal static CustomDataTypeQuery<T> CreateQuery<T>(string source,
                                                              Expression<Func<OneSDataRecord, T>> selectExpression,
                                                              Expression<Func<OneSDataRecord, bool>> whereExpression = null)
        {
            return new CustomDataTypeQuery<T>(source, whereExpression, selectExpression);
        }

        internal static NoSideEffectItemReaderFactory<T> AssertAndCastNoSideEffectItemReaderFactory<T>(
            IItemReaderFactory<T> factory)
        {
            return AssertAndCast<NoSideEffectItemReaderFactory<T>>(factory);
        }
    }
}