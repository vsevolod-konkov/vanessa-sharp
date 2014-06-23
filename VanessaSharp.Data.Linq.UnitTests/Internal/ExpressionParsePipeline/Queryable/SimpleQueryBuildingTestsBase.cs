using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Queryable
{
    /// <summary>
    /// Базовый класс для тестирования построяния <see cref="SimpleQuery"/>.
    /// </summary>
    public abstract class SimpleQueryBuildingTestsBase : TestsBase
    {
        /// <summary>Имя источника данных.</summary>
        protected const string SOURCE_NAME = "[source]";

        protected static Expression BuildQueryableExpression<T>(
            Func<IQueryable<OneSDataRecord>, IQueryable<T>> queryAction)
        {
            return TestHelperQueryProvider.BuildTestQueryExpression(SOURCE_NAME, queryAction);
        }

        internal static CustomDataTypeQuery<T> AssertAndCastCustomDataTypeQuery<T>(Trait<T> trait, ISimpleQuery query)
        {
            return AssertAndCast<CustomDataTypeQuery<T>>(query);
        }

        // TODO Упростить
        internal static void AssertSimpleQuery(ISimpleQuery testedQuery,
                                               Expression<Func<OneSDataRecord, bool>> expectedFilter = null,
                                               params SortExpression[] expectedSorters)
        {
            AssertSimpleQuery(
                AssertAndCast<SimpleQuery>(testedQuery),
                expectedFilter,
                expectedSorters
                );
        }

        private static void AssertSimpleQuery(SimpleQuery testedQuery,
                                              Expression<Func<OneSDataRecord, bool>> expectedFilter = null,
                                              params SortExpression[] expectedSorters)
        {
            Assert.AreEqual(SOURCE_NAME, testedQuery.Source);
            Assert.AreSame(expectedFilter, testedQuery.Filter);


            Assert.AreEqual(expectedSorters.Length, testedQuery.Sorters.Count);

            for (var index = 0; index < expectedSorters.Length; index++)
                AssertSortExpression(expectedSorters[index], testedQuery.Sorters[index]);
        }

        internal static void AssertSortExpression(SortExpression expectedSortExpression,
                                                 SortExpression actualSortExpression)
        {
            Assert.AreEqual(expectedSortExpression.Kind, actualSortExpression.Kind);
            Assert.AreEqual(expectedSortExpression.KeyExpression, actualSortExpression.KeyExpression);
        }

        // TODO Упростить
        internal static void AssertDataRecordsQuery(ISimpleQuery testedQuery,
                                                   Expression<Func<OneSDataRecord, bool>> expectedFilter = null,
                                                   params SortExpression[] expectedSorters)
        {
            AssertDataRecordsQuery(
                AssertAndCast<SimpleQuery>(testedQuery),
                expectedFilter,
                expectedSorters
                );
        }

        private static void AssertDataRecordsQuery(SimpleQuery testedQuery,
                                                   Expression<Func<OneSDataRecord, bool>> expectedFilter = null,
                                                   params SortExpression[] expectedSorters)
        {
            AssertSimpleQuery(testedQuery, expectedFilter, expectedSorters);
            Assert.IsInstanceOf<DataRecordsQuery>(testedQuery);
        }
    }
}
