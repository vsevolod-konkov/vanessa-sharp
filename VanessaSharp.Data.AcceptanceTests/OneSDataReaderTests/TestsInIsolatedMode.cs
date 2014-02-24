using Moq;
using NUnit.Framework;
using VanessaSharp.Data.AcceptanceTests.Mocks;
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

        internal override void OnNewOneSObjectAsking(NewOneSObjectEventArgs args)
        {
            if (args.RequiredType != typeof(IQuery))
                return;

            Assert.IsNull(_queryMock);

            _queryMock = MockHelper.CreateDisposableMock<IQuery>();
            _queryMock
                .SetupSet(q => q.Text = It.IsAny<string>())
                .Verifiable();
            _queryMock
                .Setup(q => q.Execute())
                .Returns(QueryResultMockFactory.Create(ExpectedData));

            args.CreatedInstance = _queryMock.Object;
        }
    }
}
