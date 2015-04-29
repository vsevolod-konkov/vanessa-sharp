using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Тестирование <see cref="ScalarReadExpressionParseProduct{T}"/>.
    /// </summary>
    [TestFixture]
    public sealed class ScalarReadExpressionParseProductTests
    {
        /// <summary>
        /// Тестирование метода <see cref="ScalarReadExpressionParseProduct{T}.Execute"/>.
        /// </summary>
        [Test]
        public void TestExecute()
        {
            // Arrange
            var sqlCommand = new SqlCommand("SQL", Empty.ReadOnly<SqlParameter>());
            
            var counter = 0;
            var valueConverter = new Mock<IValueConverter>(MockBehavior.Strict).Object;
            var rawValue = new object();

            const int EXPECTED_RESULT = 4325;
            Func<IValueConverter, object, int> converter = (c, o) =>
                {
                    counter++;

                    Assert.AreSame(valueConverter, c);
                    Assert.AreSame(rawValue, o);

                    return EXPECTED_RESULT;
                };

            var testedInstance = new ScalarReadExpressionParseProduct<int>(sqlCommand, converter);

            var sqlCommandExecuterMock = new Mock<ISqlCommandExecuter>(MockBehavior.Strict);
            sqlCommandExecuterMock
                .Setup(e => e.ExecuteScalar(sqlCommand))
                .Returns(Tuple.Create(valueConverter, rawValue));

            // Act
            var actualResult = testedInstance.Execute(sqlCommandExecuterMock.Object);

            // Assert
            Assert.AreEqual(actualResult, EXPECTED_RESULT);
            Assert.AreEqual(1, counter);
            
            sqlCommandExecuterMock
                .Verify(e => e.ExecuteScalar(sqlCommand), Times.Once());
        }
    }
}
