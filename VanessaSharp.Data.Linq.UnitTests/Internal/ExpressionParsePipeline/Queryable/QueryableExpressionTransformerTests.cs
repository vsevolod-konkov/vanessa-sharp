using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Queryable
{
    /// <summary>
    /// Тестирование статических методов парсера выражений <see cref="QueryableExpressionTransformer"/>.
    /// </summary>
    [TestFixture]
    public sealed class QueryableExpressionTransformerTests : QueryBuildingTestsBase
    {
        /// <summary>Тестируемый экземпляр.</summary>
        private QueryableExpressionTransformer _testedInstance;

        /// <summary>Инициализация тестируемого экземпляра.</summary>
        [SetUp]
        public void SetUp()
        {
            _testedInstance = new QueryableExpressionTransformer();
        }

        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае когда запросом является простое получение записей из источника данных.
        /// </summary>
        [Test]
        public void TestTransformGetRecordsQueryable()
        {
            // Arrange
            var expression = QueryableExpression
                .ForDataRecords(SOURCE_NAME)
                .Expression;

            // Act
            var result = _testedInstance.Transform(expression);

            // Assert
            AssertDataRecordsQuery(result, SOURCE_NAME);
        }

        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае когда в запросе вызывался метод 
        /// <see cref="Queryable.Select{TSource,TResult}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TResult}})"/>.
        /// </summary>
        [Test]
        public void TestTransformSelectingQueryable()
        {
            // Arrange
            var selector = Trait
                .Of<OneSDataRecord>()
                .SelectExpression(r => new { StringField = r.GetString("[string_field]"), IntField = r.GetInt32("[int_field]") });
            
            var testedExpression = QueryableExpression
                .ForDataRecords(SOURCE_NAME)
                .Query(q => q.Select(selector));
            
            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            AssertDataRecordsQuery(result, SOURCE_NAME, selector);
        }

        /// <summary>
        /// Тестирование получения <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае когда в запросе вызывался метод
        /// <see cref="Queryable.Where{TSource}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,bool}})"/>.
        /// </summary>
        [Test]
        public void TestTransformFilteringQueryable()
        {
            // Arrange
            Expression<Func<OneSDataRecord, bool>> filter = r => r.GetString("filterField") == "filterValue";
            var testedExpression = QueryableExpression
                .ForDataRecords(SOURCE_NAME)
                .Query(q => q.Where(filter));

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            AssertDataRecordsQuery(result, SOURCE_NAME, expectedFilter: filter);
        }

        /// <summary>
        /// Тестирование в случае
        /// </summary>
        /// <param name="queryAction">Запрос над <see cref="IQueryable{OneSDataRecord}"/></param>
        /// <param name="expectedSorters">Ожидаемые сортировщики в результате.</param>
        private void TestTransformSortingQueryable(
            Func<IQueryable<OneSDataRecord>, IQueryable<OneSDataRecord>> queryAction,
            params SortExpression[] expectedSorters)
        {
            // Arrange
            var testedExpression = QueryableExpression
                .ForDataRecords(SOURCE_NAME)
                .Query(queryAction);

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            AssertDataRecordsQuery(result, SOURCE_NAME, expectedSorters: expectedSorters);
        }


        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае когда вызывается метод
        /// <see cref="Queryable.OrderBy{TSource,TKey}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
        /// </summary>
        [Test]
        public void TestTransformQueryableWithOrderBy()
        {
            // Arrange
            Expression<Func<OneSDataRecord, int>> sorter = r => r.GetInt32("sort_field");
            
            // Arrange-Act-Assert
            TestTransformSortingQueryable(
                q => q.OrderBy(sorter),
                new SortExpression(sorter, SortKind.Ascending)
                );
        }

        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае когда вызывается метод
        /// <see cref="Queryable.OrderByDescending{TSource,TKey}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
        /// </summary>
        [Test]
        public void TestTransformQueryableWithOrderByDescending()
        {
            // Arrange
            Expression<Func<OneSDataRecord, int>> sorter = r => r.GetInt32("sort_field");

            // Arrange-Act-Assert
            TestTransformSortingQueryable(
                q => q.OrderByDescending(sorter),
                new SortExpression(sorter, SortKind.Descending)
                );
        }

        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае когда вызывается метод
        /// <see cref="Queryable.OrderBy{TSource,TKey}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>
        /// и
        /// <see cref="Queryable.ThenByDescending{TSource,TKey}(System.Linq.IOrderedQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
        /// </summary>
        [Test]
        public void TestTransformQueryableWithOrderByAndThenByDescending()
        {
            // Arrange
            Expression<Func<OneSDataRecord, int>> sorter1 = r => r.GetInt32("sort_field_1");
            Expression<Func<OneSDataRecord, string>> sorter2 = r => r.GetString("sort_field_2");

            // Arrange-Act-Assert
            TestTransformSortingQueryable(
                q => q.OrderBy(sorter1).ThenByDescending(sorter2),
                new SortExpression(sorter1, SortKind.Ascending),
                new SortExpression(sorter2, SortKind.Descending)
                );
        }

        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае когда вызывается метод
        /// <see cref="Queryable.OrderByDescending{TSource,TKey}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>
        /// и
        /// <see cref="Queryable.ThenBy{TSource,TKey}(System.Linq.IOrderedQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
        /// </summary>
        [Test]
        public void TestTransformQueryableWithOrderByDescendingAndThenBy()
        {
            // Arrange
            Expression<Func<OneSDataRecord, int>> sorter1 = r => r.GetInt32("sort_field_1");
            Expression<Func<OneSDataRecord, string>> sorter2 = r => r.GetString("sort_field_2");

            // Arrange-Act-Assert
            TestTransformSortingQueryable(
                q => q.OrderByDescending(sorter1).ThenBy(sorter2),
                new SortExpression(sorter1, SortKind.Descending),
                new SortExpression(sorter2, SortKind.Ascending)
                );
        }

        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае когда в запросе, есть и фильтрация, и сортировки и выборка данных.
        /// </summary>
        [Test]
        public void TestTransformQueryableWithFilteringAndSortingAndSelecting()
        {

            // Arrange
            var selector = Trait.Of<OneSDataRecord>()
                .SelectExpression(r => new { StringField = r.GetString("[string_field]"), IntField = r.GetInt32("[int_field]") });

            Expression<Func<OneSDataRecord, bool>> filter = r => r.GetString("[filter_field]") == "filter_value";
            Expression<Func<OneSDataRecord, int>> sortKey1 = r => r.GetInt32("[sort_key1]");
            Expression<Func<OneSDataRecord, string>> sortKey2 = r => r.GetString("[sort_key2]");
            Expression<Func<OneSDataRecord, DateTime>> sortKey3 = r => r.GetDateTime("[sort_key3]");

            var testedExpression = QueryableExpression
                .ForDataRecords(SOURCE_NAME)
                .Query(q => q
                    .Where(filter)
                    .OrderBy(sortKey1)
                    .ThenByDescending(sortKey2)
                    .ThenBy(sortKey3)
                    .Select(selector)
                    .Distinct());

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            AssertDataRecordsQuery(
                result,
                SOURCE_NAME,
                selector,
                true,
                filter, 
                new SortExpression(sortKey1, SortKind.Ascending), 
                new SortExpression(sortKey2, SortKind.Descending),
                new SortExpression(sortKey3, SortKind.Ascending));
        }

        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае запроса типизированных записей.
        /// </summary>
        [Test]
        public void TestTransformQueryableTypedRecords()
        {
            // Arrange
            var testedExpression = QueryableExpression
                .For<SomeData>()
                .Expression;

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
           AssertTypedRecordsQuery<SomeData>(result);
        }

        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае запроса типизированных записей c фильтрацией.
        /// </summary>
        [Test]
        public void TestTransformQueryableTypedRecordsWithFilter()
        {
            // Arrange
            Expression<Func<SomeData, bool>> expectedFilter = d => d.Id == 5;

            var testedExpression = QueryableExpression
                .For<SomeData>()
                .Query(q => q.Where(expectedFilter));

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            AssertTypedRecordsQuery(result, expectedFilter: expectedFilter);
        }

        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае запроса типизированных записей с сортировкой.
        /// </summary>
        [Test]
        public void TestTransformQueryableTypedRecordsWithSorting()
        {
            // Arrange
            Expression<Func<SomeData, int>> sortKeySelector1 = d => d.Id;
            Expression<Func<SomeData, string>> sortKeySelector2 = d => d.Name;

            var testedExpression = QueryableExpression
                .For<SomeData>()
                .Query(q => q
                    .OrderByDescending(sortKeySelector1)
                    .ThenBy(sortKeySelector2)
                    );

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            AssertTypedRecordsQuery<SomeData>(
                result,
                false,
                null,
                new SortExpression(sortKeySelector1, SortKind.Descending),
                new SortExpression(sortKeySelector2, SortKind.Ascending));
        }

        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае запроса типизированных кортежей.
        /// </summary>
        [Test]
        public void TestTransformQueryableTupleWithSelect()
        {
            // Arrange
            var selector = Trait.Of<SomeData>().SelectExpression(d => new {DataId = d.Id});

            var testedExpression = QueryableExpression
                .For<SomeData>()
                .Query(q => q.Select(selector));

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            AssertTypedRecordsQuery(result, selector);
        }

        /// <summary>
        /// Тестовый тип записей.
        /// </summary>
        public sealed class SomeData
        {
            public int Id;

            public string Name;
        }
    }
}
