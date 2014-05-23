using System;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>Тестирование <see cref="SimpleQueryBuilder"/>.</summary>
    [TestFixture]
    public sealed class SimpleQueryBuilderTests : TestsBase
    {
        private SimpleQueryBuilder _testedInstance;

        [SetUp]
        public void SetUp()
        {
            _testedInstance = new SimpleQueryBuilder();
        }

        /// <summary>Тестирование построения запроса получения записей.</summary>
        [Test]
        public void TestBuildGetRecordsQuery()
        {
            const string SOURCE_NAME = "[source]";
            
            // Act
            _testedInstance.HandleStart();
            _testedInstance.HandleGettingEnumerator(typeof(OneSDataRecord));
            _testedInstance.HandleGettingRecords(SOURCE_NAME);
            _testedInstance.HandleEnd();

            var result = _testedInstance.BuiltQuery;

            // Assert
            Assert.AreEqual(SOURCE_NAME, result.Source);
            Assert.IsInstanceOf<DataRecordsQuery>(result);
        }

        /// <summary>Тестирование построения запроса выборки полей записей.</summary>
        [Test]
        public void TestBuildSelectRecordsQuery()
        {
            const string SOURCE_NAME = "[source]";
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
            Assert.AreEqual(SOURCE_NAME, result.Source);
            var typedQuery = AssertAndCastSimpleQueryOf(traitOfOutputType, result);
            Assert.AreEqual(expectedExpression, typedQuery.SelectExpression);
        }

        /// <summary>Тестирование построения запроса с фильтрации записей.</summary>
        [Test]
        public void TestBuildFilteredGetRecordsQuery()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
            Expression<Func<OneSDataRecord, bool>> filterExpression = r => r.GetString("[filterField]") == "filterValue";

            // Act
            _testedInstance.HandleStart();
            _testedInstance.HandleGettingEnumerator(typeof(OneSDataRecord));
            _testedInstance.HandleFilter(filterExpression);
            _testedInstance.HandleGettingRecords(SOURCE_NAME);
            _testedInstance.HandleEnd();

            var result = _testedInstance.BuiltQuery;

            // Assert
            Assert.AreEqual(SOURCE_NAME, result.Source);
            Assert.AreEqual(filterExpression, result.Filter);
            Assert.IsInstanceOf<DataRecordsQuery>(result);
        }

        /// <summary>Тестирование построения запроса выборки и фильтрации записей.</summary>
        [Test]
        public void TestBuildFilteredSelectRecordsQuery()
        {
            // Arrange
            const string SOURCE_NAME = "[source]";
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
            Assert.AreEqual(SOURCE_NAME, result.Source);
            Assert.AreEqual(filterExpression, result.Filter);
            var typedQuery = AssertAndCastSimpleQueryOf(traitOfOutputType, result);
            Assert.AreEqual(selectExpression, typedQuery.SelectExpression);
        }

        private static CustomDataTypeQuery<T> AssertAndCastSimpleQueryOf<T>(Trait<T> trait, SimpleQuery query)
        {
            return AssertAndCast<CustomDataTypeQuery<T>>(query);
        }
    }
}
