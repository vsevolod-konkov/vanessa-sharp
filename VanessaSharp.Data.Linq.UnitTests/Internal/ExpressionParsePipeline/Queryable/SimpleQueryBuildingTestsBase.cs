using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Queryable
{
    /// <summary>
    /// Базовый класс для тестирования построяния <see cref="Query"/>.
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

        internal static IQuery<OneSDataRecord, T> AssertAndCastCustomDataTypeQuery<T>(Trait<T> trait, IQuery query)
        {
            return AssertAndCast<IQuery<OneSDataRecord, T>>(query);
        }

        // TODO Упростить
        internal static IQuery<OneSDataRecord, T> AssertDataRecordQuery<T>(Trait<T> trait, IQuery testedQuery,
                                               string expectedSourceName,
                                               Expression<Func<OneSDataRecord, bool>> expectedFilter = null,
                                               params SortExpression[] expectedSorters)
        {
            var typedQuery = AssertAndCast<IQuery<OneSDataRecord, T>>(testedQuery);

            AssertDataRecordQuery(
                typedQuery,
                expectedSourceName,
                expectedFilter,
                expectedSorters
                );

            return typedQuery;
        }

        internal static void AssertDataRecordQuery(IQuery testedQuery,
                                                           string expectedSourceName,
                                                           Expression<Func<OneSDataRecord, bool>> expectedFilter = null,
                                                           params SortExpression[] expectedSorters)
        {
            var typedQuery = AssertAndCast<IQuery<OneSDataRecord, OneSDataRecord>>(testedQuery);
            AssertDataRecordQuery(typedQuery, expectedSourceName, expectedFilter, expectedSorters);

        }

        private static void AssertDataRecordQuery<TOutput>(IQuery<OneSDataRecord, TOutput> testedQuery,
                                                           string expectedSourceName,
                                                           Expression<Func<OneSDataRecord, bool>> expectedFilter = null,
                                                           params SortExpression[] expectedSorters)
        {
            var explicitSource = AssertAndCast<ExplicitSourceDescription>(testedQuery.Source);
            Assert.AreEqual(expectedSourceName, explicitSource.SourceName);

            AssertQuery(testedQuery, expectedFilter, expectedSorters);
        }

        private static void AssertQuery<TInput, TOutput>(IQuery<TInput, TOutput> testedQuery,
                                              Expression<Func<TInput, bool>> expectedFilter = null,
                                              params SortExpression[] expectedSorters)
        {
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
    }
}
