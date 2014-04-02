using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>Тестирование парсера выражений.</summary>
    [TestFixture]
    public sealed class ExpressionParserTests : TestsBase
    {
        /// <summary>Тестируемый экземпляр.</summary>
        private readonly ExpressionParser _testedInstance = ExpressionParser.Default;

        /// <summary>
        /// Тестирование парсинга выражения получения записей из источника.
        /// </summary>
        [Test]
        public void TestParseGetRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";

            var expression = Expression.Call(
                OneSQueryExpressionHelper.GetRecordsExpression(SOURCE_NAME),
                OneSQueryExpressionHelper.GetGetEnumeratorMethodInfo<OneSDataRecord>());

            // Act
            var product = _testedInstance.Parse(expression);

            // Assert
            var command = product.Command;

            Assert.AreEqual("SELECT * FROM " + SOURCE_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var recordProduct = AssertAndCast<CollectionReadExpressionParseProduct<OneSDataRecord>>(product);
            Assert.IsInstanceOf<OneSDataRecordReaderFactory>(recordProduct.ItemReaderFactory);
        }

        /// <summary>
        /// Тестирование парсинга выражения выборки полей из записей источника.
        /// </summary>
        [Test]
        public void TestParseSelectRecordsExpression()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            const string STRING_FIELD_NAME = "[string_field]";
            const string INT32_FIELD_NAME = "[int_field]";

            var provider = new TestHelperQueryProvider();
            var query = provider.CreateQuery<OneSDataRecord>(
                OneSQueryExpressionHelper.GetRecordsExpression(SOURCE_NAME));

            var selectQuery = query.Select(r => new { StringField = r.GetString(STRING_FIELD_NAME), IntField = r.GetInt32(INT32_FIELD_NAME) });

            var expression = Expression.Call(
                selectQuery.Expression,
                GetGetEnumeratorMethodInfo(selectQuery));

            // Act
            var product = _testedInstance.Parse(expression);

            // Assert
            var command = product.Command;

            Assert.AreEqual("SELECT " + STRING_FIELD_NAME + ", " + INT32_FIELD_NAME + " FROM " + SOURCE_NAME, command.Sql);
            Assert.AreEqual(0, command.Parameters.Count);

            var recordProduct = AssertAndCastCollectionReadExpressionParseProduct(selectQuery, product);
            
            // TODO test recordProduct
        }

        /// <summary>Вспомогательный метод для подстановки анонимного типа.</summary>
        private static MethodInfo GetGetEnumeratorMethodInfo<T>(IQueryable<T> query)
        {
            return OneSQueryExpressionHelper.GetGetEnumeratorMethodInfo<T>();
        }

        private static CollectionReadExpressionParseProduct<T> AssertAndCastCollectionReadExpressionParseProduct<T>(
            IQueryable<T> query, ExpressionParseProduct product)
        {
            return AssertAndCast<CollectionReadExpressionParseProduct<T>>(product);
        }

        /// <summary>Вспомогательный класс для генерации linq-выражений.</summary>
        private sealed class TestHelperQueryProvider : IQueryProvider
        {
            public IQueryable CreateQuery(Expression expression)
            {
                throw new System.NotImplementedException();
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestHelperQuery<TElement>(this, expression);
            }

            public object Execute(Expression expression)
            {
                throw new System.NotImplementedException();
            }

            public TResult Execute<TResult>(Expression expression)
            {
                throw new System.NotImplementedException();
            }
        }

        /// <summary>Вспомогательный класс для генерации linq-выражений.</summary>
        private sealed class TestHelperQuery<T> : IQueryable<T>
        {
            private readonly Expression _expression;
            private readonly IQueryProvider _provider;

            public TestHelperQuery(TestHelperQueryProvider provider, Expression expression)
            {
                _provider = provider;
                _expression = expression;
            }

            public IEnumerator<T> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public Type ElementType { get; private set; }

            public Expression Expression
            {
                get { return _expression; }
            }

            public IQueryProvider Provider
            {
                get { return _provider; }
            }
        }
    }
}
