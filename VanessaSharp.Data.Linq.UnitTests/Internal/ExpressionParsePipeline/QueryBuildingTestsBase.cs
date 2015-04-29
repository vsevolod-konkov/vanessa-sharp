using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
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

        private static void TestTransformQuery<TInput, TOutput>(IQuery<TInput, TOutput> testedQuery)
        {
            var expressionParseProduct = new CollectionReadExpressionParseProduct<TOutput>(
                _sqlCommand,
                new Mock<IItemReaderFactory<TOutput>>(MockBehavior.Strict).Object); 

            var transformerMock = new Mock<IQueryTransformer>(MockBehavior.Strict);
            transformerMock
                .Setup(t => t.Transform(testedQuery))
                .Returns(expressionParseProduct);

            TestTransform(testedQuery, transformerMock.Object, expressionParseProduct);

            transformerMock.Verify(t => t.Transform(testedQuery), Times.Once());
        }

        private static void TestTransformScalarQuery<TInput, TOutput, TResult>(IScalarQuery<TInput, TOutput, TResult> testedQuery)
        {
            var expressionParseProduct = new ScalarReadExpressionParseProduct<TResult>(_sqlCommand, (c, o) => (TResult)o);

            var transformerMock = new Mock<IQueryTransformer>(MockBehavior.Strict);
            transformerMock
                .Setup(t => t.TransformScalar(testedQuery))
                .Returns(expressionParseProduct);

            TestTransform(testedQuery, transformerMock.Object, expressionParseProduct);

            transformerMock.Verify(t => t.TransformScalar(testedQuery), Times.Once());
        }

        private static void TestTransform<TInput, TOutput>(IQuery<TInput, TOutput> testedQuery, IQueryTransformer transformer,
                                                    ExpressionParseProduct expectedResult)
        {
            var transformServiceMock = new Mock<IQueryTransformService>(MockBehavior.Strict);
            transformServiceMock
                .Setup(s => s.CreateTransformer())
                .Returns(transformer);

            // Act
            var actualResult = testedQuery.Transform(transformServiceMock.Object);

            // Assert
            Assert.AreSame(expectedResult, actualResult);

            transformServiceMock.Verify(s => s.CreateTransformer(), Times.Once());
        }

        private static readonly SqlCommand _sqlCommand = new SqlCommand("SQL", Empty.ReadOnly<SqlParameter>());

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
            TestTransformQuery(typedQuery);
        }

        private static void AssertTypedRecordsQueryAndTestTransform<TInput, TOutput>(
            IQuery<TInput, TOutput> testedQuery,
            Expression<Func<TInput, TOutput>> expectedSelector,
            Expression<Func<TInput, bool>> expectedFilter,
            IList<SortExpression> expectedSorters,
            bool expectedIsDistinct)
        {
            AssertTypedRecordsQuery(testedQuery, expectedSelector, expectedFilter, expectedSorters, expectedIsDistinct);

            TestTransformQuery(testedQuery);
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

        internal static void AssertTypedRecordsQueryAndTestTransform<TInput, TOutput>(
            IQuery testedQuery,
            Expression<Func<TInput, TOutput>> expectedSelector,
            bool expectedIsDistinct = false,
            Expression<Func<TInput, bool>> expectedFilter = null,
            params SortExpression[] expectedSorters)
        {
            var typedQuery = AssertEx.IsInstanceAndCastOf<QueryBase<TInput, TOutput>>(testedQuery);
            AssertTypedRecordsQueryAndTestTransform(typedQuery, expectedSelector, expectedFilter, (IList<SortExpression>)expectedSorters, expectedIsDistinct);
        }

        internal static void AssertTypedRecordsQueryAndTestTransform<T>(
            IQuery testedQuery,
            bool expectedIsDistinct = false,
            Expression<Func<T, bool>> expectedFilter = null,
            params SortExpression[] expectedSorters)
        {
            AssertTypedRecordsQueryAndTestTransform<T, T>(testedQuery, null, expectedIsDistinct, expectedFilter, expectedSorters);
        }

        internal static void AssertDataRecordsScalarQuery<TOutput, TResult>(
            IQuery testedQuery,
            string expectedSourceName,
            Expression<Func<OneSDataRecord, TOutput>> expectedSelector,
            AggregateFunction aggregateFunction,
            bool expectedIsDistinct = false,
            Expression<Func<OneSDataRecord, bool>> expectedFilter = null,
            params SortExpression[] expectedSorters)
        {

            var typedScalarQuery = AssertEx.IsInstanceAndCastOf<IScalarQuery<OneSDataRecord, TOutput, TResult>>(testedQuery);

            IQuery<OneSDataRecord, TOutput> typedQuery = typedScalarQuery;
            AssertDataRecordsQuery(typedQuery, expectedSourceName, expectedSelector, expectedFilter, (IList<SortExpression>)expectedSorters, expectedIsDistinct);
            Assert.AreEqual(aggregateFunction, typedScalarQuery.AggregateFunction);

            TestTransformScalarQuery(typedScalarQuery);
        }

        internal static void AssertTypedRecordsScalarQuery<TInput, TOutput, TResult>(
            IQuery testedQuery,
            Expression<Func<TInput, TOutput>> expectedSelector,
            AggregateFunction aggregateFunction,
            bool expectedIsDistinct = false,
            Expression<Func<TInput, bool>> expectedFilter = null,
            params SortExpression[] expectedSorters)
        {

            var typedScalarQuery = AssertEx.IsInstanceAndCastOf<IScalarQuery<TInput, TOutput, TResult>>(testedQuery);

            IQuery<TInput, TOutput> typedQuery = typedScalarQuery;
            AssertTypedRecordsQuery(typedQuery, expectedSelector, expectedFilter, expectedSorters, expectedIsDistinct);
            Assert.AreEqual(aggregateFunction, typedScalarQuery.AggregateFunction);

            TestTransformScalarQuery(typedScalarQuery);
        }
    }
}