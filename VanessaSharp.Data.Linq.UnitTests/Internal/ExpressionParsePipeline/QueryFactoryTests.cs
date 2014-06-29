using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Тестирование <see cref="QueryFactory"/>.
    /// </summary>
    [TestFixture]
    public sealed class QueryFactoryTests : TestsBase
    {
        /// <summary>
        /// Тестирование 
        /// <see cref="QueryFactory.CreateQuery(string,System.Linq.Expressions.LambdaExpression,System.Collections.ObjectModel.ReadOnlyCollection{VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SortExpression})"/>.
        /// </summary>
        [Test]
        public void TestCreateDataRecordsQuery()
        {
            // Arrange
            const string SOURCE_NAME = "test";
            
            Expression<Func<OneSDataRecord, bool>> filter = r => true;
            Expression<Func<OneSDataRecord, int>> sorter = r => 5;
            var sorters = new ReadOnlyCollection<SortExpression>(new[] {new SortExpression(sorter, SortKind.Ascending)});
            
            // Act
            var result = QueryFactory.CreateQuery(SOURCE_NAME, filter, sorters);

            // Assert
            var typedQuery = AssertAndCast<QueryBase<OneSDataRecord, OneSDataRecord>>(result);
            var typedSourceDescription = AssertAndCast<ExplicitSourceDescription>(typedQuery.Source);
            Assert.AreEqual(SOURCE_NAME, typedSourceDescription.SourceName);
            Assert.IsNull(typedQuery.Selector);
            Assert.AreSame(filter, typedQuery.Filter);
            Assert.AreSame(sorters, typedQuery.Sorters);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="QueryFactory.CreateQuery(string,System.Linq.Expressions.LambdaExpression,System.Collections.ObjectModel.ReadOnlyCollection{VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SortExpression})"/>
        /// </summary>
        [Test]
        public void TestCreateSelectDataRecordsQuery()
        {
            // Arrange
            const string SOURCE_NAME = "test";

            Expression<Func<OneSDataRecord, OutputData>> selector = r => new OutputData();
            Expression<Func<OneSDataRecord, bool>> filter = r => true;
            Expression<Func<OneSDataRecord, int>> sorter = r => 5;
            var sorters = new ReadOnlyCollection<SortExpression>(new[] { new SortExpression(sorter, SortKind.Ascending) });

            // Act
            var result = QueryFactory.CreateQuery(SOURCE_NAME, selector, filter, sorters);

            // Assert
            var typedQuery = AssertAndCast<QueryBase<OneSDataRecord, OutputData>>(result);
            var typedSourceDescription = AssertAndCast<ExplicitSourceDescription>(typedQuery.Source);
            Assert.AreEqual(SOURCE_NAME, typedSourceDescription.SourceName);
            Assert.AreSame(selector, typedQuery.Selector);
            Assert.AreSame(filter, typedQuery.Filter);
            Assert.AreSame(sorters, typedQuery.Sorters);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="QueryFactory.CreateQuery(string,System.Linq.Expressions.LambdaExpression,System.Collections.ObjectModel.ReadOnlyCollection{VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SortExpression})"/>.
        /// </summary>
        [Test]
        public void TestCreateTypedRecordsQuery()
        {
            // Arrange
            Expression<Func<InputData, bool>> filter = r => true;
            Expression<Func<InputData, int>> sorter = r => 5;
            var sorters = new ReadOnlyCollection<SortExpression>(new[] { new SortExpression(sorter, SortKind.Ascending) });

            // Act
            var result = QueryFactory.CreateQuery(typeof(InputData), filter, sorters);

            // Assert
            var typedQuery = AssertAndCast<QueryBase<InputData, InputData>>(result);
            Assert.IsInstanceOf<SourceDescriptionByType<InputData>>(typedQuery.Source);
            Assert.IsNull(typedQuery.Selector);
            Assert.AreSame(filter, typedQuery.Filter);
            Assert.AreSame(sorters, typedQuery.Sorters);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="QueryFactory.CreateQuery(string,System.Linq.Expressions.LambdaExpression,System.Collections.ObjectModel.ReadOnlyCollection{VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SortExpression})"/>.
        /// </summary>
        [Test]
        public void TestCreateSelectTypedRecordsQuery()
        {
            // Arrange
            Expression<Func<InputData, OutputData>> selector = r => new OutputData();
            Expression<Func<InputData, bool>> filter = r => true;
            Expression<Func<InputData, int>> sorter = r => 5;
            var sorters = new ReadOnlyCollection<SortExpression>(new[] { new SortExpression(sorter, SortKind.Ascending) });

            // Act
            var result = QueryFactory.CreateQuery(selector, filter, sorters);

            // Assert
            var typedQuery = AssertAndCast<QueryBase<InputData, OutputData>>(result);
            Assert.IsInstanceOf<SourceDescriptionByType<InputData>>(typedQuery.Source);
            Assert.AreSame(selector, typedQuery.Selector);
            Assert.AreSame(filter, typedQuery.Filter);
            Assert.AreSame(sorters, typedQuery.Sorters);
        }

        public sealed class InputData
        {}

        public sealed class OutputData
        {}
    }
}
