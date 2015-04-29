using System;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Queryable
{
    /// <summary>Тестирование <see cref="QueryBuilder"/>.</summary>
    [TestFixture]
    public sealed class QueryBuilderTests : QueryBuildingTestsBase
    {
        private QueryBuilder _testedInstance;

        [SetUp]
        public void SetUp()
        {
            _testedInstance = new QueryBuilder();
        }

        /// <summary>Тестирование построения запроса получения записей.</summary>
        [Test]
        public void TestBuildQueryWhenOnlyGetRecords()
        {
            // Act
            _testedInstance.HandleStart();
            _testedInstance.HandleGettingEnumerator(typeof(OneSDataRecord));
            _testedInstance.HandleGettingRecords(SOURCE_NAME);
            _testedInstance.HandleEnd();

            var result = _testedInstance.BuiltQuery;

            // Assert
            AssertDataRecordsQuery(result, SOURCE_NAME);
        }

        /// <summary>Тестирование построения запроса выборки полей записей.</summary>
        [Test]
        public void TestBuildQueryWhenSelectingTuple()
        {
            // Arrange
            var expectedExpression = Trait
                     .Of<OneSDataRecord>()
                     .SelectExpression(r => new {Name = r.GetString("name"), Value = r.GetInt32("value")});
            var traitOfOutputType = expectedExpression.GetTraitOfOutputType();

            // Act
            _testedInstance.HandleStart();
            _testedInstance.HandleGettingEnumerator(traitOfOutputType.Type);
            _testedInstance.HandleSelect(expectedExpression);
            _testedInstance.HandleGettingRecords(SOURCE_NAME);
            _testedInstance.HandleEnd();

            var result = _testedInstance.BuiltQuery;

            // Assert
            AssertDataRecordsQuery(result, SOURCE_NAME, expectedExpression);
        }

        /// <summary>Тестирование построения запроса с фильтрации записей.</summary>
        [Test]
        public void TestBuildQueryWhenFiltering()
        {
            // Arrange
            Expression<Func<OneSDataRecord, bool>> expectedFilter = r => r.GetString("[filterField]") == "filterValue";

            // Act
            _testedInstance.HandleStart();
            _testedInstance.HandleGettingEnumerator(typeof(OneSDataRecord));
            _testedInstance.HandleFilter(expectedFilter);
            _testedInstance.HandleGettingRecords(SOURCE_NAME);
            _testedInstance.HandleEnd();

            var result = _testedInstance.BuiltQuery;

            // Assert
            AssertDataRecordsQuery(result, SOURCE_NAME, expectedFilter: expectedFilter);
        }

        /// <summary>Тестирование построения запроса выборки и фильтрации записей.</summary>
        [Test]
        public void TestBuildQueryWhenFilteringAndSelectingTuple()
        {
            // Arrange
            Expression<Func<OneSDataRecord, bool>> expectedFilter = r => r.GetString("[filterField]") == "filterValue";
            var expectedSelector = Trait
                     .Of<OneSDataRecord>()
                     .SelectExpression(r => new { Name = r.GetString("name"), Value = r.GetInt32("value") });
            var traitOfOutputType = expectedSelector.GetTraitOfOutputType();

            // Act
            _testedInstance.HandleStart();
            _testedInstance.HandleGettingEnumerator(traitOfOutputType.Type);
            _testedInstance.HandleSelect(expectedSelector);
            _testedInstance.HandleFilter(expectedFilter);
            _testedInstance.HandleGettingRecords(SOURCE_NAME);
            _testedInstance.HandleEnd();

            var result = _testedInstance.BuiltQuery;

            // Assert
            AssertDataRecordsQuery(result, SOURCE_NAME, expectedSelector, expectedFilter: expectedFilter);
        }

        private void TestBuildQueryWhenSorting(Action<QueryBuilder> queryBuilderAction,
                                               params SortExpression[] expectedSorters)
        {
            // Act
            _testedInstance.HandleStart();
            _testedInstance.HandleGettingEnumerator(typeof(OneSDataRecord));

            queryBuilderAction(_testedInstance);

            _testedInstance.HandleGettingRecords(SOURCE_NAME);
            _testedInstance.HandleEnd();

            var result = _testedInstance.BuiltQuery;

            // Assert
            AssertDataRecordsQuery(result, SOURCE_NAME, expectedSorters: expectedSorters);
        }

        [Test]
        public void TestBuildQueryWhenSorting()
        {
            // Arrange
            Expression<Func<OneSDataRecord, int>> sortKeyExpression = r => r.GetInt32("int_field");

            // Arrange-Act-Assert
            TestBuildQueryWhenSorting(
                qb => qb.HandleOrderBy(sortKeyExpression),
                new SortExpression(sortKeyExpression, SortKind.Ascending));
        }

        
        [Test]
        public void TestBuildQueryWhenSortingDescending()
        {
            // Arrange
            Expression<Func<OneSDataRecord, int>> sortKeyExpression = r => r.GetInt32("int_field");

            // Arrange-Act-Assert
            TestBuildQueryWhenSorting(
                qb => qb.HandleOrderByDescending(sortKeyExpression),
                new SortExpression(sortKeyExpression, SortKind.Descending));
        }

        [Test]
        public void TestBuildQueryWhenSortingAscendingAndDescending()
        {
            // Arrange
            Expression<Func<OneSDataRecord, int>> sortKey1Expression = r => r.GetInt32("int_field");
            Expression<Func<OneSDataRecord, string>> sortKey2Expression = r => r.GetString("string_field");

            // Arrange-Act-Assert
            TestBuildQueryWhenSorting(
                qb =>
                    {
                        qb.HandleThenByDescending(sortKey2Expression);
                        qb.HandleOrderBy(sortKey1Expression);
                    },
                new SortExpression(sortKey1Expression, SortKind.Ascending),
                new SortExpression(sortKey2Expression, SortKind.Descending));
        }

        [Test]
        public void TestBuildQueryWhenSortingDescendingAndAscending()
        {
            // Arrange
            Expression<Func<OneSDataRecord, int>> sortKey1Expression = r => r.GetInt32("int_field");
            Expression<Func<OneSDataRecord, string>> sortKey2Expression = r => r.GetString("string_field");

            // Arrange-Act-Assert
            TestBuildQueryWhenSorting(
                qb =>
                {
                    qb.HandleThenBy(sortKey2Expression);
                    qb.HandleOrderByDescending(sortKey1Expression);
                },
                new SortExpression(sortKey1Expression, SortKind.Descending),
                new SortExpression(sortKey2Expression, SortKind.Ascending));
        }

        [Test]
        public void TestBuildQueryWhenDoubleSorting()
        {
            // Arrange
            Expression<Func<OneSDataRecord, int>> sortKey1Expression = r => r.GetInt32("int_field");
            Expression<Func<OneSDataRecord, string>> sortKey2Expression = r => r.GetString("string_field");

            // Arrange-Act-Assert
            TestBuildQueryWhenSorting(
                qb =>
                {
                    qb.HandleOrderBy(sortKey2Expression);
                    qb.HandleOrderBy(sortKey1Expression);
                },
                new SortExpression(sortKey2Expression, SortKind.Ascending));
        }

        [Test]
        public void TestBuildQueryComplex()
        {
            // Arrange
            Expression<Func<OneSDataRecord, bool>> expectedFilter = r => r.GetString("[filterField]") == "filterValue";

            Expression<Func<OneSDataRecord, int>> sortKey1 = r => r.GetInt32("any_field");
            Expression<Func<OneSDataRecord, string>> sortKey2 = r => r.GetString("any_field");
            Expression<Func<OneSDataRecord, DateTime>> sortKey3 = r => r.GetDateTime("any_field");

            var expectedSelector = Trait
                     .Of<OneSDataRecord>()
                     .SelectExpression(r => new { Name = r.GetString("name"), Value = r.GetInt32("value") });
            var traitOfOutputType = expectedSelector.GetTraitOfOutputType();

            // Act
            _testedInstance.HandleStart();
            _testedInstance.HandleGettingEnumerator(traitOfOutputType.Type);
            
            _testedInstance.HandleDistinct();
            _testedInstance.HandleSelect(expectedSelector);

            _testedInstance.HandleThenBy(sortKey3);
            _testedInstance.HandleThenByDescending(sortKey2);
            _testedInstance.HandleOrderByDescending(sortKey1);

            _testedInstance.HandleFilter(expectedFilter);
            _testedInstance.HandleGettingRecords(SOURCE_NAME);
            _testedInstance.HandleEnd();

            var result = _testedInstance.BuiltQuery;

            // Assert
            AssertDataRecordsQuery(
                result,
                SOURCE_NAME,
                expectedSelector,
                true,
                expectedFilter,
                new SortExpression(sortKey1, SortKind.Descending),
                new SortExpression(sortKey2, SortKind.Descending),
                new SortExpression(sortKey3, SortKind.Ascending));
        }

        [Test]
        public void TestBuildSumQuery()
        {
            // Arrange
            Expression<Func<OneSDataRecord, bool>> expectedFilter = r => r.GetString("[filterField]") == "filterValue";
            Expression<Func<OneSDataRecord, int>> expectedSelector = r => r.GetInt32("any_field");

            // Act
            _testedInstance.HandleStart();
            _testedInstance.HandleAggregate(typeof(int), AggregateFunction.Summa, typeof(int));

            _testedInstance.HandleSelect(expectedSelector);

            _testedInstance.HandleFilter(expectedFilter);
            _testedInstance.HandleGettingRecords(SOURCE_NAME);
            _testedInstance.HandleEnd();

            var result = _testedInstance.BuiltQuery;

            // Assert
            AssertDataRecordsScalarQuery<int, int>(result, SOURCE_NAME,  expectedSelector, AggregateFunction.Summa, expectedFilter: expectedFilter);
        }
    }
}
