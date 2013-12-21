using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests
{
    /// <summary>
    /// Тесты на <see cref="OneSCommand.ExecuteReader(System.Data.CommandBehavior)"/>
    /// </summary>
    [TestFixture]
    public sealed class ExecuteReaderTests
    {
        /// <summary>Тестирование основного метода.</summary>
        [Test]
        public void Test()
        {
            const string TEST_SQL = "Запрос";

            // Arrange
            var oneSConnection = new OneSConnection();

            var queryResult = new Mock<IQueryResult>(MockBehavior.Strict).Object;

            var queryMock = new Mock<IQuery>(MockBehavior.Strict);
            queryMock
                .SetupSet(q => q.Text = It.IsAny<string>())
                .Verifiable();
            queryMock
                .Setup(q => q.Execute())
                .Returns(queryResult)
                .Verifiable();
            queryMock
                .Setup(q => q.Dispose())
                .Verifiable();

            var globalContextMock = new Mock<IGlobalContext>(MockBehavior.Strict);
            globalContextMock
                .Setup(ctx => ctx.NewObject<IQuery>())
                .Returns(queryMock.Object)
                .Verifiable();

            var globalContextProviderMock = new Mock<IGlobalContextProvider>(MockBehavior.Strict);
            globalContextProviderMock
                .SetupGet(p => p.GlobalContext)
                .Returns(globalContextMock.Object);

            var testedInstance = new OneSCommand(globalContextProviderMock.Object, oneSConnection);

            // Act
            testedInstance.CommandText = TEST_SQL;
            var reader = testedInstance.ExecuteReader();

            // Assert
            globalContextMock.Verify(ctx => ctx.NewObject<IQuery>(), Times.Once());
            queryMock.VerifySet(q => q.Text = TEST_SQL, Times.Once());
            queryMock.Verify(q => q.Execute(), Times.Once());
            queryMock.Verify(q => q.Dispose(), Times.Once());

            Assert.AreSame(queryResult, reader.QueryResult);
        }
    }
}
