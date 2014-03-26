using System.Linq;
using Moq;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;
using VanessaSharp.AcceptanceTests.Utility.Mocks;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.AcceptanceTests.OneSDataReaderTests
{
    /// <summary>Приемочные тесты на <see cref="OneSDataReader"/> в изолированном режиме.</summary>
    #if ISOLATED_MODE
    [TestFixture(Description = "Тестирование для изолированного режима.")]
    #endif
    public sealed class TestsInIsolatedMode : TestsBase
    {
        private Mock<IQuery> _queryMock;

        [SetUp]
        public void InitFields()
        {
            _queryMock = null;
        }

        public TestsInIsolatedMode()
            : base(TestMode.Isolated)
        {}

        /// <summary>
        /// Обработчик запроса на создание экземпляра объекта 1С.
        /// </summary>
        protected override void OnNewOneSObjectAsking(NewOneSObjectEventArgs args)
        {
            if (args.RequiredType != typeof(IQuery))
                return;

            Assert.IsNull(_queryMock);

            _queryMock = MockHelper.CreateDisposableMock<IQuery>();
            _queryMock
                .SetupSet(q => q.Text = It.IsAny<string>())
                .Verifiable();
            _queryMock
                .Setup(q => q.SetParameter(It.IsAny<string>(), It.IsAny<object>()))
                .Verifiable();
            _queryMock
                .Setup(q => q.Execute())
                .Returns(QueryResultMockFactory.Create(ExpectedData));

            args.CreatedInstance = _queryMock.Object;
        }

        /// <summary>Обработка после выполнения команды.</summary>
        /// <param name="command">Команда</param>
        protected override void OnAfterExecute(OneSCommand command)
        {
            var sql = command.CommandText;
            _queryMock.VerifySet(q => q.Text = sql, Times.Once());

            var parameters = command.Parameters.AsEnumerable();
            if (parameters.Any())
            {
                foreach (var p in parameters)
                {
                    _queryMock.Verify(q => q.SetParameter(p.ParameterName, p.Value), Times.Once());
                }
            }
            else
            {
                _queryMock.Verify(q => q.SetParameter(It.IsAny<string>(), It.IsAny<object>()), Times.Never());
            }
        }
    }
}
