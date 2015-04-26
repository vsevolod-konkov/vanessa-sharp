using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Базовый класс для тестирования построения <see cref="IQuery{TInput, TOutput}"/>.
    /// </summary>
    public abstract class QueryBuildingTestsBase
    {
        /// <summary>Имя источника данных.</summary>
        protected const string SOURCE_NAME = "[source]";

        private static void AssertQuery<TInput, TOutput>(IQuery<TInput, TOutput> testedQuery,
                                              Expression<Func<TInput, TOutput>> expectedSelector,
                                              Expression<Func<TInput, bool>> expectedFilter,
                                              IList<SortExpression> expectedSorters,
                                              bool exceptedIsDistinct)
        {
            Assert.AreSame(expectedSelector, testedQuery.Selector);
            Assert.AreSame(expectedFilter, testedQuery.Filter);
            AssertSortExpressions(expectedSorters, testedQuery.Sorters);

            Assert.AreEqual(exceptedIsDistinct, testedQuery.IsDistinct);
        }

        private static void AssertSortExpressions(IList<SortExpression> expectedExpressions,
                                                   IList<SortExpression> actualExpressions)
        {
            Assert.AreEqual(expectedExpressions.Count, actualExpressions.Count);

            for (var index = 0; index < expectedExpressions.Count; index++)
                AssertSortExpression(expectedExpressions[index], actualExpressions[index]);
        }

        private static void AssertSortExpression(SortExpression expectedSortExpression,
                                                 SortExpression actualSortExpression)
        {
            Assert.AreEqual(expectedSortExpression.Kind, actualSortExpression.Kind);
            Assert.AreSame(expectedSortExpression.KeyExpression, actualSortExpression.KeyExpression);
        }

        private static void AssertDataRecordsQuery<TOutput>(IQuery<OneSDataRecord, TOutput> testedQuery,
                                                           string expectedSourceName,
                                                           Expression<Func<OneSDataRecord, TOutput>> expectedSelector,
                                                           Expression<Func<OneSDataRecord, bool>> expectedFilter,
                                                           IList<SortExpression> expectedSorters,
                                                           bool exceptedIsDistinct)
        {
            var explicitSource = AssertEx.IsInstanceAndCastOf<ExplicitSourceDescription>(testedQuery.Source);
            Assert.AreEqual(expectedSourceName, explicitSource.SourceName);

            AssertQuery(testedQuery, expectedSelector, expectedFilter, expectedSorters, exceptedIsDistinct);
        }
        
        internal static void AssertDataRecordsQuery(
            IQuery testedQuery, 
            string expectedSourceName,
            bool expectedIsDistinct = false,
            Expression<Func<OneSDataRecord, bool>> expectedFilter = null,
            params SortExpression[] expectedSorters)
        {
            AssertDataRecordsQuery<OneSDataRecord>(
                testedQuery,
                expectedSourceName,
                null,
                expectedIsDistinct,
                expectedFilter,
                expectedSorters);
        }

        internal static void AssertDataRecordsQuery<T>(
            IQuery testedQuery,
            string expectedSourceName,
            Expression<Func<OneSDataRecord, T>> expectedSelector,
            bool expectedIsDistinct = false,
            Expression<Func<OneSDataRecord, bool>> expectedFilter = null,
            params SortExpression[] expectedSorters)
        {

            var typedQuery = AssertEx.IsInstanceAndCastOf<QueryBase<OneSDataRecord, T>>(testedQuery);
            
            AssertDataRecordsQuery(typedQuery, expectedSourceName, expectedSelector, expectedFilter, (IList<SortExpression>)expectedSorters, expectedIsDistinct);
        }

        private static void AssertTypedRecordsQuery<TInput, TOutput>(
            IQuery<TInput, TOutput> testedQuery,
            Expression<Func<TInput, TOutput>> expectedSelector,
            Expression<Func<TInput, bool>> expectedFilter,
            IList<SortExpression> expectedSorters,
            bool expectedIsDistinct)
        {
            Assert.IsInstanceOf<SourceDescriptionByType<TInput>>(testedQuery.Source);
            AssertQuery(testedQuery, expectedSelector, expectedFilter, expectedSorters, expectedIsDistinct);
        }

        internal static void AssertTypedRecordsQuery<TInput, TOutput>(
            IQuery testedQuery,
            Expression<Func<TInput, TOutput>> expectedSelector,
            bool expectedIsDistinct = false,
            Expression<Func<TInput, bool>> expectedFilter = null,
            params SortExpression[] expectedSorters)
        {
            var typedQuery = AssertEx.IsInstanceAndCastOf<QueryBase<TInput, TOutput>>(testedQuery);
            AssertTypedRecordsQuery(typedQuery, expectedSelector, expectedFilter, (IList<SortExpression>)expectedSorters, expectedIsDistinct);
        }

        internal static void AssertTypedRecordsQuery<T>(
            IQuery testedQuery,
            bool expectedIsDistinct = false,
            Expression<Func<T, bool>> expectedFilter = null,
            params SortExpression[] expectedSorters)
        {
            AssertTypedRecordsQuery<T, T>(testedQuery, null, expectedIsDistinct, expectedFilter, expectedSorters);
        }
    }
}