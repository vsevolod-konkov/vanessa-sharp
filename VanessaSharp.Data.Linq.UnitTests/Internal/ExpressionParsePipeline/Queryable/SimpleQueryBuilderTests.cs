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
    public sealed class SimpleQueryBuilderTests : SimpleQueryBuildingTestsBase
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
            AssertDataRecordQuery(result, SOURCE_NAME);
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
            var typedQuery = AssertDataRecordQuery(traitOfOutputType, result, SOURCE_NAME);
            Assert.AreEqual(expectedExpression, typedQuery.Selector);
        }

        /// <summary>Тестирование построения запроса с фильтрации записей.</summary>
        [Test]
        public void TestBuildQueryWhenFiltering()
        {
            // Arrange
            Expression<Func<OneSDataRecord, bool>> filterExpression = r => r.GetString("[filterField]") == "filterValue";

            // Act
            _testedInstance.HandleStart();
            _testedInstance.HandleGettingEnumerator(typeof(OneSDataRecord));
            _testedInstance.HandleFilter(filterExpression);
            _testedInstance.HandleGettingRecords(SOURCE_NAME);
            _testedInstance.HandleEnd();

            var result = _testedInstance.BuiltQuery;

            // Assert
            AssertDataRecordQuery(result, SOURCE_NAME, filterExpression);
        }

        /// <summary>Тестирование построения запроса выборки и фильтрации записей.</summary>
        [Test]
        public void TestBuildQueryWhenFilteringAndSelectingTuple()
        {
            // Arrange
            Expression<Func<OneSDataRecord, bool>> filterExpression = r => r.GetString("[filterField]") == "filterValue";
            var selectExpression = Trait
                     .Of<OneSDataRecord>()
                     .SelectExpression(r => new { Name = r.GetString("name"), Value = r.GetInt32("value") });
            var traitOfOutputType = selectExpression.GetTraitOfOutputType();

            // Act
            _testedInstance.HandleStart();
            _testedInstance.HandleGettingEnumerator(traitOfOutputType.Type);
            _testedInstance.HandleSelect(selectExpression);
            _testedInstance.HandleFilter(filterExpression);
            _testedInstance.HandleGettingRecords(SOURCE_NAME);
            _testedInstance.HandleEnd();

            var result = _testedInstance.BuiltQuery;

            // Assert
            var typedQuery = AssertDataRecordQuery(traitOfOutputType, result, SOURCE_NAME, filterExpression);
            Assert.AreEqual(selectExpression, typedQuery.Selector);
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
            AssertDataRecordQuery(result, SOURCE_NAME, expectedSorters: expectedSorters);
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
            Expression<Func<OneSDataRecord, bool>> filterExpression = r => r.GetString("[filterField]") == "filterValue";

            Expression<Func<OneSDataRecord, int>> sortKey1Expression = r => r.GetInt32("any_field");
            Expression<Func<OneSDataRecord, string>> sortKey2Expression = r => r.GetString("any_field");
            Expression<Func<OneSDataRecord, DateTime>> sortKey3Expression = r => r.GetDateTime("any_field");

            var selectExpression = Trait
                     .Of<OneSDataRecord>()
                     .SelectExpression(r => new { Name = r.GetString("name"), Value = r.GetInt32("value") });
            var traitOfOutputType = selectExpression.GetTraitOfOutputType();

            // Act
            _testedInstance.HandleStart();
            _testedInstance.HandleGettingEnumerator(traitOfOutputType.Type);
            
            _testedInstance.HandleSelect(selectExpression);

            _testedInstance.HandleThenBy(sortKey3Expression);
            _testedInstance.HandleThenByDescending(sortKey2Expression);
            _testedInstance.HandleOrderByDescending(sortKey1Expression);

            _testedInstance.HandleFilter(filterExpression);
            _testedInstance.HandleGettingRecords(SOURCE_NAME);
            _testedInstance.HandleEnd();

            var result = _testedInstance.BuiltQuery;

            // Assert
            var typedQuery = AssertDataRecordQuery(
                traitOfOutputType,
                result,
                SOURCE_NAME,
                filterExpression,
                new SortExpression(sortKey1Expression, SortKind.Descending),
                new SortExpression(sortKey2Expression, SortKind.Descending),
                new SortExpression(sortKey3Expression, SortKind.Ascending));

            Assert.AreEqual(selectExpression, typedQuery.Selector);
        }
    }
}
