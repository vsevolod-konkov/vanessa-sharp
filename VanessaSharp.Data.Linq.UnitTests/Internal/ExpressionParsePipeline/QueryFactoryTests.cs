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
        /// <see cref="QueryFactory.CreateQuery(string,System.Linq.Expressions.LambdaExpression,System.Collections.ObjectModel.ReadOnlyCollection{VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SortExpression},bool,System.Nullable{int})"/>.
        /// </summary>
        [Test]
        public void TestCreateDataRecordsQuery()
        {
            // Arrange
            Expression<Func<OneSDataRecord, bool>> filter = r => true;
            Expression<Func<OneSDataRecord, int>> sortKey = r => 5;
            var sorter = new SortExpression(sortKey, SortKind.Ascending);
            
            // Act
            var result = QueryFactory.CreateQuery(SOURCE_NAME, filter, new[] {sorter}.ToReadOnly(), true, 5);

            // Assert
            AssertDataRecordsQuery(result, SOURCE_NAME, true, 5, filter, sorter);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="QueryFactory.CreateQuery"/>
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
            var result = QueryFactory.CreateQuery(SOURCE_NAME, selector, filter, new[]{sorter}.ToReadOnly(), true, 4);

            // Assert
            AssertDataRecordsQuery(result, SOURCE_NAME, selector, true, 4, filter, sorter);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="QueryFactory.CreateQuery"/>.
        /// </summary>
        [Test]
        public void TestCreateTypedRecordsQuery()
        {
            // Arrange
            Expression<Func<InputData, bool>> filter = r => true;
            Expression<Func<InputData, int>> sortKey = r => 5;
            var sorter = new SortExpression(sortKey, SortKind.Ascending);

            // Act
            var result = QueryFactory.CreateQuery(typeof(InputData), filter, new[]{sorter}.ToReadOnly(), true, null);

            // Assert
            AssertTypedRecordsQueryAndTestTransform(result, true, null, filter, sorter);
        }

        /// <summary>
        /// Тестирование
        /// <see cref="QueryFactory.CreateQuery"/>.
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
            var result = QueryFactory.CreateQuery(selector, filter, new[]{sorter}.ToReadOnly(), false, null);

            // Assert
            AssertTypedRecordsQueryAndTestTransform(result, selector,  false, null, filter, sorter);
        }

        /// <summary>
        /// Тестирование 
        /// <see cref="QueryFactory.CreateScalarQuery(System.Linq.Expressions.LambdaExpression,System.Linq.Expressions.LambdaExpression,System.Collections.ObjectModel.ReadOnlyCollection{VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SortExpression},bool,VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.AggregateFunction,System.Type)"/>
        /// </summary>
        [Test]
        public void TestCreateDataRecordsScalarQuery()
        {
            // Arrange
            Expression<Func<OneSDataRecord, OutputData>> selector = r => new OutputData();
            Expression<Func<OneSDataRecord, bool>> filter = r => true;
            Expression<Func<OneSDataRecord, int>> sortKey = r => 5;
            var sorter = new SortExpression(sortKey, SortKind.Ascending);
            const AggregateFunction AGGREGATE_FUNCTION = AggregateFunction.Maximum;

            // Act
            var result = QueryFactory.CreateScalarQuery(SOURCE_NAME, selector, filter, new[] { sorter }.ToReadOnly(), false, AGGREGATE_FUNCTION, typeof(int));

            // Assert
            AssertDataRecordsScalarQueryAndTestTransform<OutputData, int>(result, SOURCE_NAME, selector, AGGREGATE_FUNCTION, false, filter, sorter);
        }

        /// <summary>
        /// Тестирование 
        /// <see cref="QueryFactory.CreateScalarQuery(System.Linq.Expressions.LambdaExpression,System.Linq.Expressions.LambdaExpression,System.Collections.ObjectModel.ReadOnlyCollection{VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SortExpression},bool,VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.AggregateFunction,System.Type)"/>
        /// </summary>
        [Test]
        public void TestCreateTypedRecordsScalarQuery()
        {
            // Arrange
            Expression<Func<InputData, OutputData>> selector = r => new OutputData();
            Expression<Func<InputData, bool>> filter = r => true;
            Expression<Func<InputData, int>> sortKey = r => 5;
            var sorter = new SortExpression(sortKey, SortKind.Ascending);
            const AggregateFunction AGGREGATE_FUNCTION = AggregateFunction.Count;

            // Act
            var result = QueryFactory.CreateScalarQuery(selector, filter, new[] { sorter }.ToReadOnly(), true, AGGREGATE_FUNCTION, typeof(int));

            // Assert
            AssertTypedRecordsScalarQueryAndTestTransform<InputData, OutputData, int>(result, selector, AGGREGATE_FUNCTION, true, filter, sorter);
        }

        /// <summary>
        /// Тестирование <see cref="QueryFactory.CreateCountQuery"/>.
        /// </summary>
        [Test]
        public void TestCreateCountQuery()
        {
            // Arrange
            Expression<Func<InputData, bool>> filter = r => true;
            Expression<Func<InputData, int>> sortKey = r => 5;
            var sorter = new SortExpression(sortKey, SortKind.Ascending);

            // Act
            var result = QueryFactory.CreateCountQuery(typeof(InputData), filter, new[] { sorter }.ToReadOnly(), typeof(int));

            // Assert
            AssertTypedRecordsScalarQueryAndTestTransform<InputData, InputData, int>(result, null, AggregateFunction.Count, false, filter, sorter);
        }

        public sealed class InputData
        {}

        public sealed class OutputData
        {}
    }
}
