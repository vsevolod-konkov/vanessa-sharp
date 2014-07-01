using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Queryable
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
                                              IList<SortExpression> expectedSorters)
        {
            Assert.AreSame(expectedSelector, testedQuery.Selector);
            Assert.AreSame(expectedFilter, testedQuery.Filter);
            AssertSortExpressions(expectedSorters, testedQuery.Sorters);
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
                                                           IList<SortExpression> expectedSorters)
        {
            var explicitSource = AssertEx.IsInstanceAndCastOf<ExplicitSourceDescription>(testedQuery.Source);
            Assert.AreEqual(expectedSourceName, explicitSource.SourceName);

            AssertQuery(testedQuery, expectedSelector, expectedFilter, expectedSorters);
        }
        
        internal static void AssertDataRecordsQuery(
            IQuery testedQuery, 
            string expectedSourceName,
            Expression<Func<OneSDataRecord, bool>> expectedFilter = null,
            params SortExpression[] expectedSorters)
        {
            AssertDataRecordsQuery<OneSDataRecord>(
                testedQuery,
                expectedSourceName,
                null,
                expectedFilter,
                expectedSorters);
        }

        internal static void AssertDataRecordsQuery<T>(
            IQuery testedQuery,
            string expectedSourceName,
            Expression<Func<OneSDataRecord, T>> expectedSelector, 
            Expression<Func<OneSDataRecord, bool>> expectedFilter = null,
            params SortExpression[] expectedSorters)
        {

            var typedQuery = AssertEx.IsInstanceAndCastOf<IQuery<OneSDataRecord, T>>(testedQuery);
            
            AssertDataRecordsQuery(typedQuery, expectedSourceName, expectedSelector, expectedFilter, (IList<SortExpression>)expectedSorters);
        }

        private static void AssertTypedRecordsQuery<TInput, TOutput>(
            IQuery<TInput, TOutput> testedQuery,
            Expression<Func<TInput, TOutput>> expectedSelector,
            Expression<Func<TInput, bool>> expectedFilter,
            IList<SortExpression> expectedSorters)
        {
            Assert.IsInstanceOf<SourceDescriptionByType<TInput>>(testedQuery.Source);
            AssertQuery(testedQuery, expectedSelector, expectedFilter, expectedSorters);
        }

        internal static void AssertTypedRecordsQuery<TInput, TOutput>(
            IQuery testedQuery,
            Expression<Func<TInput, TOutput>> expectedSelector,
            Expression<Func<TInput, bool>> expectedFilter = null,
            params SortExpression[] expectedSorters)
        {
            var typedQuery = AssertEx.IsInstanceAndCastOf<IQuery<TInput, TOutput>>(testedQuery);
            AssertTypedRecordsQuery(typedQuery, expectedSelector, expectedFilter, (IList<SortExpression>)expectedSorters);
        }

        internal static void AssertTypedRecordsQuery<T>(
            IQuery testedQuery,
            Expression<Func<T, bool>> expectedFilter = null,
            params SortExpression[] expectedSorters)
        {
            AssertTypedRecordsQuery<T, T>(testedQuery, null, expectedFilter, expectedSorters);
        }
    }
}