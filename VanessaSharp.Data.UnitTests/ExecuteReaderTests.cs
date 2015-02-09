using System;
using System.Data;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests
{
    /// <summary>
    /// Тесты на <see cref="OneSCommand.ExecuteReader(System.Data.CommandBehavior)"/>
    /// </summary>
    [TestFixture]
    public sealed class ExecuteReaderTests
    {
        /// <summary>Тестовый запрос.</summary>
        private const string TEST_SQL = "Запрос";

        /// <summary>Объект результата запроса.</summary>
        private IQueryResult _queryResult;

        /// <summary>Мок запроса.</summary>
        private Mock<IQuery> _queryMock;

        /// <summary>Глобальный контекст.</summary>
        private Mock<IGlobalContext> _globalContextMock;

        /// <summary>Тестовый экземпляр.</summary>
        private OneSCommand _testedInstance;
        
        /// <summary>
        /// Инициализация тестов.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            var oneSConnection = new OneSConnection();

            _queryResult = new Mock<IQueryResult>(MockBehavior.Strict).Object;

            _queryMock = new Mock<IQuery>(MockBehavior.Strict);
            _queryMock
                .SetupSet(q => q.Text = It.IsAny<string>())
                .Verifiable();
            _queryMock
                .Setup(q => q.Execute())
                .Returns(_queryResult)
                .Verifiable();
            _queryMock
                .Setup(q => q.Dispose())
                .Verifiable();

            _globalContextMock = new Mock<IGlobalContext>(MockBehavior.Strict);
            _globalContextMock
                .Setup(ctx => ctx.NewObject<IQuery>())
                .Returns(_queryMock.Object)
                .Verifiable();

            var globalContextProviderMock = new Mock<IGlobalContextProvider>(MockBehavior.Strict);
            globalContextProviderMock
                .SetupGet(p => p.GlobalContext)
                .Returns(_globalContextMock.Object);

            _testedInstance = new OneSCommand(globalContextProviderMock.Object, oneSConnection);
        }
        
        private void AssertAfterExecute(
            OneSDataReader reader,
            QueryResultIteration expectedIteration = QueryResultIteration.Default,
            bool onCloseActionExists = false)
        {
            _globalContextMock.Verify(ctx => ctx.NewObject<IQuery>(), Times.Once());
            _queryMock.VerifySet(q => q.Text = TEST_SQL, Times.Once());
            _queryMock.Verify(q => q.Execute(), Times.Once());
            _queryMock.Verify(q => q.Dispose(), Times.Once());

            Assert.IsInstanceOf<QueryResultDataRecordsProvider>(reader.DataRecordsProvider);
            var dataRecordsProvider = (QueryResultDataRecordsProvider)reader.DataRecordsProvider;

            Assert.AreSame(_queryResult, dataRecordsProvider.QueryResult);
            Assert.AreEqual(expectedIteration, dataRecordsProvider.QueryResultIteration);
            Assert.AreEqual(onCloseActionExists, reader.HasOnCloseAction);
        }
        
        /// <summary>Тестирование основного метода.</summary>
        [Test]
        public void TestDefault()
        {
            // Arrange

            // Act
            _testedInstance.CommandText = TEST_SQL;
            var reader = _testedInstance.ExecuteReader();

            // Assert
            AssertAfterExecute(reader);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSCommand.ExecuteReader(System.Data.CommandBehavior)"/>
        /// с параметром <see cref="CommandBehavior.CloseConnection"/>.
        /// </summary>
        [Test]
        public void TestCommandBehaviorCloseConnection()
        {
            // Arrange

            // Act
            _testedInstance.CommandText = TEST_SQL;
            var reader = _testedInstance.ExecuteReader(CommandBehavior.CloseConnection);

            // Assert
            AssertAfterExecute(reader, onCloseActionExists: true);
        }

        /// <summary>Тестирование основного метода c параметрами.</summary>
        [Test]
        public void TestWithParameters()
        {
            // Arrange
            _queryMock
                .Setup(q => q.SetParameter(It.IsAny<string>(), It.IsAny<object>()))
                .Verifiable();

            var parameters = new[]
            { 
                new OneSParameter("Параметр1", "Значение"),
                new OneSParameter("Параметр2", 4534) 
            };


            // Act
            _testedInstance.CommandText = TEST_SQL;
            foreach (var p in parameters)
                _testedInstance.Parameters.Add(p);
            var reader = _testedInstance.ExecuteReader();

            // Assert
            AssertAfterExecute(reader);
            
            foreach (var p in parameters)
            {
                var name = p.ParameterName;
                var value = p.Value;

                _queryMock.Verify(q => q.SetParameter(name, value), Times.Once());
            }
        }

        /// <summary>Тестирование основного метода.</summary>
        [Test]
        [TestCase(QueryResultIteration.Linear)]
        [TestCase(QueryResultIteration.ByGroups)]
        [TestCase(QueryResultIteration.ByGroupsWithHierarchy)]
        public void TestWithDetermineQueryResultIteration(QueryResultIteration queryResultIteration)
        {
            // Arrange

            // Act
            _testedInstance.CommandText = TEST_SQL;
            var reader = _testedInstance.ExecuteReader(CommandBehavior.Default, queryResultIteration);

            // Assert
            AssertAfterExecute(reader, queryResultIteration);
        }
    }
}
