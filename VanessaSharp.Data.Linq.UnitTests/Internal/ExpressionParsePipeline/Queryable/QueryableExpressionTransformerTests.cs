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
    public sealed class QueryableExpressionTransformerTests : SimpleQueryBuildingTestsBase
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
            var expression = TestHelperQueryProvider.BuildTestQueryExpression(SOURCE_NAME);

            // Act
            var result = _testedInstance.Transform(expression);

            // Assert
            AssertDataRecordQuery(result, SOURCE_NAME);
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
            var selectExpression = Trait.Of<OneSDataRecord>()
                .SelectExpression(r => new { StringField = r.GetString("[string_field]"), IntField = r.GetInt32("[int_field]") });
            var trait = selectExpression.GetTraitOfOutputType();
            
            var testedExpression = BuildQueryableExpression(q => q.Select(selectExpression));
            
            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            var typedQuery = AssertDataRecordQuery(trait, result, SOURCE_NAME);

            Assert.AreEqual(selectExpression, typedQuery.Selector);
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
            Expression<Func<OneSDataRecord, bool>> filterExpression = r => r.GetString("filterField") == "filterValue";
            var testedExpression = BuildQueryableExpression(q => q.Where(filterExpression));

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            AssertDataRecordQuery(result, SOURCE_NAME, expectedFilter: filterExpression);
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
            var testedExpression = BuildQueryableExpression(queryAction);

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            AssertDataRecordQuery(result, SOURCE_NAME, expectedSorters: expectedSorters);
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
            Expression<Func<OneSDataRecord, int>> orderbyExpression = r => r.GetInt32("sort_field");
            
            // Arrange-Act-Assert
            TestTransformSortingQueryable(
                q => q.OrderBy(orderbyExpression),
                new SortExpression(orderbyExpression, SortKind.Ascending)
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
            Expression<Func<OneSDataRecord, int>> orderbyExpression = r => r.GetInt32("sort_field");

            // Arrange-Act-Assert
            TestTransformSortingQueryable(
                q => q.OrderByDescending(orderbyExpression),
                new SortExpression(orderbyExpression, SortKind.Descending)
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
            Expression<Func<OneSDataRecord, int>> orderbyExpression = r => r.GetInt32("sort_field_1");
            Expression<Func<OneSDataRecord, string>> thenbyExpression = r => r.GetString("sort_field_2");

            // Arrange-Act-Assert
            TestTransformSortingQueryable(
                q => q.OrderBy(orderbyExpression).ThenByDescending(thenbyExpression),
                new SortExpression(orderbyExpression, SortKind.Ascending),
                new SortExpression(thenbyExpression, SortKind.Descending)
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
            Expression<Func<OneSDataRecord, int>> orderbyExpression = r => r.GetInt32("sort_field_1");
            Expression<Func<OneSDataRecord, string>> thenbyExpression = r => r.GetString("sort_field_2");

            // Arrange-Act-Assert
            TestTransformSortingQueryable(
                q => q.OrderByDescending(orderbyExpression).ThenBy(thenbyExpression),
                new SortExpression(orderbyExpression, SortKind.Descending),
                new SortExpression(thenbyExpression, SortKind.Ascending)
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
            var selectExpression = Trait.Of<OneSDataRecord>()
                .SelectExpression(r => new { StringField = r.GetString("[string_field]"), IntField = r.GetInt32("[int_field]") });
            var trait = selectExpression.GetTraitOfOutputType();

            Expression<Func<OneSDataRecord, bool>> filterExpression = r => r.GetString("[filter_field]") == "filter_value";
            Expression<Func<OneSDataRecord, int>> sortKey1Expression = r => r.GetInt32("[sort_key1]");
            Expression<Func<OneSDataRecord, string>> sortKey2Expression = r => r.GetString("[sort_key2]");
            Expression<Func<OneSDataRecord, DateTime>> sortKey3Expression = r => r.GetDateTime("[sort_key3]");

            var testedExpression = BuildQueryableExpression(q => q
                    .Where(filterExpression)
                    .OrderBy(sortKey1Expression)
                    .ThenByDescending(sortKey2Expression)
                    .ThenBy(sortKey3Expression)
                    .Select(selectExpression));

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            var typedQuery = AssertDataRecordQuery(
                trait,
                result,
                SOURCE_NAME,
                filterExpression, 
                new SortExpression(sortKey1Expression, SortKind.Ascending), 
                new SortExpression(sortKey2Expression, SortKind.Descending),
                new SortExpression(sortKey3Expression, SortKind.Ascending));

            Assert.AreEqual(selectExpression, typedQuery.Selector);
        }

        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае запроса типизированных кортежей.
        /// </summary>
        [Test]
        public void TestTransformQueryableTuple()
        {
            // Arrange
            var query = TestHelperQueryProvider.QueryOf<AnyDataType>();
            var testedExpression = TestHelperQueryProvider.BuildTestQueryExpression(query);

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            var tupleQuery = AssertAndCast<IQuery<AnyDataType, AnyDataType>>(result);
            Assert.IsInstanceOf<SourceDescriptionByType<AnyDataType>>(tupleQuery.Source);
            Assert.IsNull(tupleQuery.Selector);
            Assert.IsNull(tupleQuery.Filter);
            Assert.AreEqual(0, tupleQuery.Sorters.Count);
        }

        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае запроса типизированных кортежей c фильтрацией.
        /// </summary>
        [Test]
        public void TestTransformQueryableTupleWithFilter()
        {
            // Arrange
            Expression<Func<AnyDataType, bool>> filterExpression = d => d.Id == 5;

            var query = TestHelperQueryProvider.QueryOf<AnyDataType>();
            query = query.Where(filterExpression);
            var testedExpression = TestHelperQueryProvider.BuildTestQueryExpression(query);

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            var typedQuery = AssertAndCast<IQuery<AnyDataType, AnyDataType>>(result);
            Assert.IsInstanceOf<SourceDescriptionByType<AnyDataType>>(typedQuery.Source);
            Assert.IsNull(typedQuery.Selector);
            Assert.AreSame(filterExpression, typedQuery.Filter);
            Assert.AreEqual(0, typedQuery.Sorters.Count);
        }

        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае запроса типизированных кортежей с сортировкой.
        /// </summary>
        [Test]
        public void TestTransformQueryableTupleWithSorting()
        {
            // Arrange
            Expression<Func<AnyDataType, int>> sortExpression1 = d => d.Id;
            Expression<Func<AnyDataType, string>> sortExpression2 = d => d.Name;

            var query = TestHelperQueryProvider
                .QueryOf<AnyDataType>()
                .OrderByDescending(sortExpression1)
                .ThenBy(sortExpression2);

            var testedExpression = TestHelperQueryProvider.BuildTestQueryExpression(query);

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            var typedQuery = AssertAndCast<IQuery<AnyDataType, AnyDataType>>(result);
            Assert.IsInstanceOf<SourceDescriptionByType<AnyDataType>>(typedQuery.Source);
            Assert.IsNull(typedQuery.Selector);
            Assert.IsNull(typedQuery.Filter);

            Assert.AreEqual(2, typedQuery.Sorters.Count);

            Assert.AreEqual(sortExpression1, typedQuery.Sorters[0].KeyExpression);
            Assert.AreEqual(SortKind.Descending, typedQuery.Sorters[0].Kind);

            Assert.AreEqual(sortExpression2, typedQuery.Sorters[1].KeyExpression);
            Assert.AreEqual(SortKind.Ascending, typedQuery.Sorters[1].Kind);
        }

        /// <summary>
        /// Тестирование <see cref="QueryableExpressionTransformer.Transform"/>
        /// в случае запроса типизированных кортежей.
        /// </summary>
        [Test]
        public void TestTransformQueryableTupleWithSelect()
        {
            // Arrange
            var selectExpression = Trait.Of<AnyDataType>().SelectExpression(d => new {DataId = d.Id});
            var trait = selectExpression.GetTraitOfOutputType();

            var query = TestHelperQueryProvider
                .QueryOf<AnyDataType>()
                .Select(selectExpression);
            
            var testedExpression = TestHelperQueryProvider.BuildTestQueryExpression(query);

            // Act
            var result = _testedInstance.Transform(testedExpression);

            // Assert
            var tupleQuery = AssertAndCastTupleQuery(trait, result);
            Assert.IsInstanceOf<SourceDescriptionByType<AnyDataType>>(tupleQuery.Source);
            Assert.AreSame(selectExpression, tupleQuery.Selector);
            Assert.IsNull(tupleQuery.Filter);
            Assert.AreEqual(0, tupleQuery.Sorters.Count);
        }

        private static IQuery<AnyDataType, TOutput> AssertAndCastTupleQuery<TOutput>(Trait<TOutput> outputTrait, IQuery query)
        {
            return AssertAndCast<IQuery<AnyDataType, TOutput>>(query);
        }

        public sealed class AnyDataType
        {
            public int Id;

            public string Name;
        }
    }
}
