using System.Text;
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

        /// <summary>
        /// Тестирование построения сложного SQL-условия.
        /// </summary>
        [Test]
        public void TestBuildSqlComplexCondition()
        {
            // Assert
            var testedInstance = new SqlBinaryOperationCondition(
                SqlBinaryLogicOperationType.And,
                new SqlBinaryRelationCondition(
                    SqlBinaryRelationType.Equal,
                    new SqlBinaryOperationExpression(
                        SqlBinaryArithmeticOperationType.Add,
                        new SqlFieldExpression(SqlDefaultTableExpression.Instance, "field1"),
                        new SqlNegateExpression(
                            new SqlFieldExpression(SqlDefaultTableExpression.Instance, "field2"))),
                    new SqlBinaryOperationExpression(
                        SqlBinaryArithmeticOperationType.Subtract,
                        new SqlFieldExpression(SqlDefaultTableExpression.Instance, "field3"),
                        SqlLiteralExpression.Create(42))),
                new SqlBinaryOperationCondition(
                    SqlBinaryLogicOperationType.Or,
                    new SqlIsNullCondition(new SqlFieldExpression(SqlDefaultTableExpression.Instance, "field4"), true),
                    new SqlLikeCondition(
                        new SqlFieldExpression(SqlDefaultTableExpression.Instance, "field5"),
                        true, "%ABC_", null)
                    ));

            var sqlBuilder = new StringBuilder();

            // Act
            testedInstance.AppendSqlTo(sqlBuilder, SqlBuildOptions.IgnoreSpaces);
            var result = sqlBuilder.ToString();

            // Assert
            Assert.AreEqual(
                "( ( field1 + ( -field2 ) ) = ( field3 - 42 ) ) AND ( ( field4 IS NULL ) OR ( field5 LIKE \"%ABC_\" ) )",
                result);
        }
    }
}
