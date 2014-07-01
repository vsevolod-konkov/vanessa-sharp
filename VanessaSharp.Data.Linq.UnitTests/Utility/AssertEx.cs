using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;

namespace VanessaSharp.Data.Linq.UnitTests.Utility
{
    /// <summary>
    /// Дополнительные методы для проверки результатов тестов.
    /// </summary>
    internal static class AssertEx
    {
        /// <summary>
        /// Проверка и приведение к типу <typeparamref name="T"/> значения <paramref name="value"/>.
        /// </summary>
        public static T IsInstanceAndCastOf<T>(object value)
        {
            Assert.IsInstanceOf<T>(value);

            return (T)value;
        }

        public static CollectionReadExpressionParseProduct<T> IsInstanceAndCastCollectionReadExpressionParseProduct<T>(
            Trait<T> trait, ExpressionParseProduct product)
        {
            return IsInstanceAndCastOf<CollectionReadExpressionParseProduct<T>>(product);
        }

        public static NoSideEffectItemReaderFactory<T> IsInstanceAndCastNoSideEffectItemReaderFactory<T>(
            IItemReaderFactory<T> itemReaderFactory)
        {
            return IsInstanceAndCastOf<NoSideEffectItemReaderFactory<T>>(itemReaderFactory);
        }

        public static IQuery<OneSDataRecord, T> IsInstanceAndCastDataRecordsQuery<T>(IQuery query)
        {
            return IsInstanceAndCastOf<IQuery<OneSDataRecord, T>>(query);
        }

        public static IQuery<OneSDataRecord, T> IsInstanceAndCastDataRecordsQuery<T>(Trait<T> trait, IQuery query)
        {
            return IsInstanceAndCastDataRecordsQuery<T>(query);
        }
    }
}
