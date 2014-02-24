using Moq;
using NUnit.Framework;
using VanessaSharp.Data.AcceptanceTests.Mocks;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.AcceptanceTests.OneSCommandTests
{
    /// <summary>Тесты проверяющие правильность поведения выполнения запросов команды в изоляционном режиме.</summary>
    #if ISOLATED_MODE
    [TestFixture(Description = "Тестирование метода Execute для изоляционного режима.")]
    #endif
    public sealed class GeneralExecuteTestsInIsolatedMode : GeneralExecuteTestsBase
    {
        public GeneralExecuteTestsInIsolatedMode() : base(TestMode.Isolated)
        {}

        /// <summary>
        /// Счетчик запросов на создание объектов запроса.
        /// </summary>
        private int _queryAskedCounter;

        /// <summary>
        /// Мок объекта запроса.
        /// </summary>
        private Mock<IQuery> _queryMock;

        /// <summary>
        /// Мок объекта результата запроса.
        /// </summary>
        private Mock<IQueryResult> _queryResultMock; 

        /// <summary>
        /// Обработчик запроса на создание экземпляра объекта 1С.
        /// </summary>
        internal override void OnNewOneSObjectAsking(NewOneSObjectEventArgs args)
        {
            if (args.RequiredType != typeof(IQuery))
                return;

            _queryAskedCounter++;

            _queryResultMock = MockHelper.CreateDisposableMock<IQueryResult>();

            _queryMock = MockHelper.CreateDisposableMock<IQuery>();
            _queryMock
                .SetupSet(q => q.Text = TEST_SQL)
                .Verifiable();
            _queryMock
                .Setup(q => q.Execute())
                .Returns(_queryResultMock.Object)
                .Verifiable();

            args.CreatedInstance = _queryMock.Object;
        }

        /// <summary>Очистка полей перед тестом.</summary>
        [SetUp]
        public void ClearFields()
        {
            _queryAskedCounter = 0;
            _queryMock = null;
            _queryResultMock = null;
        }

        /// <summary>
        /// Дополнительная проверка в случае если выполнение 
        /// метода <see cref="OneSCommand.ExecuteReader()"/>
        /// вызвало исключение.
        /// </summary>
        protected override void AssertIfExecuteReaderIsInvalid()
        {
            Assert.AreEqual(0, _queryAskedCounter);
        }

        /// <summary>
        /// Дополнительная проверка в случае если выполнение 
        /// метода <see cref="OneSCommand.ExecuteReader()"/>
        /// было успешным.
        /// </summary>
        protected override void AssertIsExecuteReaderIsSuccess()
        {
            Assert.AreEqual(1, _queryAskedCounter);

            _queryMock.VerifySet(q => q.Text = TEST_SQL, Times.Once());
            _queryMock.Verify(q => q.Execute(), Times.Once());

            MockHelper.VerifyDispose(_queryMock);
            MockHelper.VerifyDispose(_queryResultMock);
        }
    }
}
