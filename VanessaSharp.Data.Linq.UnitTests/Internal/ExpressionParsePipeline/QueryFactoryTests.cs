using System;
using System.Linq.Expressions;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Тестирование <see cref="QueryFactory"/>.
    /// </summary>
    [TestFixture]
    public sealed class QueryFactoryTests : QueryBuildingTestsBase
    {
        /// <summary>
        /// Тестирование 
        /// <see cref="QueryFactory.CreateQuery(string,System.Linq.Expressions.LambdaExpression,System.Collections.ObjectModel.ReadOnlyCollection{VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SortExpression}, bool)"/>.
        /// </summary>
        [Test]
        public void TestCreateDataRecordsQuery()
        {
            // Arrange
            Expression<Func<OneSDataRecord, bool>> filter = r => true;
            Expression<Func<OneSDataRecord, int>> sortKey = r => 5;
            var sorter = new SortExpression(sortKey, SortKind.Ascending);
            
            // Act
            var result = QueryFactory.CreateQuery(SOURCE_NAME, filter, new[] {sorter}.ToReadOnly(), true);

            // Assert
            AssertDataRecordsQuery(result, SOURCE_NAME, true, filter, sorter);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="QueryFactory.CreateQuery(string,System.Linq.Expressions.LambdaExpression,System.Collections.ObjectModel.ReadOnlyCollection{VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SortExpression}, bool)"/>
        /// </summary>
        [Test]
        public void TestCreateSelectDataRecordsQuery()
        {
            // Arrange
            Expression<Func<OneSDataRecord, OutputData>> selector = r => new OutputData();
            Expression<Func<OneSDataRecord, bool>> filter = r => true;
            Expression<Func<OneSDataRecord, int>> sortKey = r => 5;
            var sorter = new SortExpression(sortKey, SortKind.Ascending);

            // Act
            var result = QueryFactory.CreateQuery(SOURCE_NAME, selector, filter, new[]{sorter}.ToReadOnly(), true);

            // Assert
            AssertDataRecordsQuery(result, SOURCE_NAME, selector, true, filter, sorter);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="QueryFactory.CreateQuery(string,System.Linq.Expressions.LambdaExpression,System.Collections.ObjectModel.ReadOnlyCollection{VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SortExpression}, bool)"/>.
        /// </summary>
        [Test]
        public void TestCreateTypedRecordsQuery()
        {
            // Arrange
            Expression<Func<InputData, bool>> filter = r => true;
            Expression<Func<InputData, int>> sortKey = r => 5;
            var sorter = new SortExpression(sortKey, SortKind.Ascending);

            // Act
            var result = QueryFactory.CreateQuery(typeof(InputData), filter, new[]{sorter}.ToReadOnly(), true);

            // Assert
            AssertTypedRecordsQuery(result, true, filter, sorter);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="QueryFactory.CreateQuery(string,System.Linq.Expressions.LambdaExpression,System.Collections.ObjectModel.ReadOnlyCollection{VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SortExpression}, bool)"/>.
        /// </summary>
        [Test]
        public void TestCreateSelectTypedRecordsQuery()
        {
            // Arrange
            Expression<Func<InputData, OutputData>> selector = r => new OutputData();
            Expression<Func<InputData, bool>> filter = r => true;
            Expression<Func<InputData, int>> sortKey = r => 5;
            var sorter = new SortExpression(sortKey, SortKind.Ascending);

            // Act
            var result = QueryFactory.CreateQuery(selector, filter, new[]{sorter}.ToReadOnly(), false);

            // Assert
            AssertTypedRecordsQuery(result, selector,  false, filter, sorter);
        }

        public sealed class InputData
        {}

        public sealed class OutputData
        {}
    }
}
