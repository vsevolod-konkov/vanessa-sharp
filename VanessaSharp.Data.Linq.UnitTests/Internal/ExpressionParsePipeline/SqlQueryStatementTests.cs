using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline
{
    /// <summary>Тесты на <see cref="SqlQueryStatement"/>.</summary>
    [TestFixture]
    public sealed class SqlQueryStatementTests
    {
        /// <summary>Тестирование построения SQL-запроса.</summary>
        [Test]
        public void TestBuildSqlWhenExistsOnlySelectAndFrom()
        {
            // Assert
            var testedInstance = new SqlQueryStatement(
                new SqlSelectStatement(
                    new SqlColumnListExpression(
                        new SqlExpression[]
                        {
                            new SqlFieldExpression(SqlDefaultTableExpression.Instance, "field1"),
                            new SqlFieldExpression(SqlDefaultTableExpression.Instance, "field2"),
                            new SqlFieldExpression(SqlDefaultTableExpression.Instance, "field3")
                        }),
                        false,
                        null
                     ),
                new SqlFromStatement("source"),
                null, null);
            
            // Act
            var result = testedInstance.BuildSql();

            // Assert
            Assert.AreEqual(
                "SELECT field1, field2, field3 FROM source",
                result);
        }
    }
}
