using System;
using Moq;
using NUnit.Framework;

namespace VanessaSharp.Proxy.Common.Tests
{
    /// <summary>
    /// Тесты на <see cref="OneSProxyWrapperWithGlobalContext"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSProxyWrapperWithGlobalContextTests
        : OneSProxyWrapperTests
    {
        /// <summary>
        /// Мок для реализации <see cref="IOneSWrapFactory"/>.
        /// </summary>
        private Mock<IOneSWrapFactory> _wrapFactoryMock;

        /// <summary>
        /// Глобальный контекст 1С.
        /// </summary>
        private OneSGlobalContext _globalContext;

        /// <summary>Инициализация тестируемого экземпляра.</summary>
        internal override OneSProxyWrapper InitTestedInstance()
        {
            _wrapFactoryMock = new Mock<IOneSWrapFactory>(MockBehavior.Strict);
            _globalContext = new OneSGlobalContext(new object());

            return new OneSProxyWrapperWithGlobalContext(
                _globalContext, _wrapFactoryMock.Object, ObjectDefinerMock.Object);
        }

        /// <summary>Создание объекта 1С.</summary>
        private object CreateOneSObject()
        {
            var obj = new object();
            SetupIsOneSObject(obj, true);

            return obj;
        }

        /// <summary>Тестирование поведения по умолчанию для запроса стандартных типов.</summary>
        /// <param name="requestedType">Запрашиваемый тип.</param>
        [Test]
        public void TestDefaultTypes(
            [Values(typeof(object), typeof(IDisposable), typeof(IGlobalContextBound))] Type requestedType)
        {
            // Assert
            var obj = CreateOneSObject();

            // Act
            var result = TestedInstance.Wrap(obj, requestedType);

            // Assert
            Assert.IsInstanceOf<OneSContextBoundObject>(result);
            var contextBoundObject = (OneSContextBoundObject)result;
            Assert.AreSame(_globalContext, contextBoundObject.GlobalContext);
            AssertOneSObject(obj, contextBoundObject);

            _wrapFactoryMock.Verify(f => f.CreateWrap(It.IsAny<object>(), It.IsAny<CreateWrapParameters>()), Times.Never());
        }

        /// <summary>Тестовый интерфейс.</summary>
        public interface ITestObject : IDisposable
        {}

        /// <summary>
        /// Тестовая реализация тестового интерфейса.
        /// </summary>
        public sealed class TestObject : OneSObject
        {
            internal TestObject(object comObject, IOneSProxyWrapper proxyWrapper)
                : base(comObject, proxyWrapper)
            {}
        }

        /// <summary>
        /// Тестирования использования фабрики оберток.
        /// </summary>
        private void TestUsingWrapFactory(
            Func<object, CreateWrapParameters, OneSObject> createWrapReturns, 
            Action<object, object> assertResultAction = null, 
            Type expectedException = null)
        {
            // Arrange
            var obj = CreateOneSObject();

            object actualObj = null;
            CreateWrapParameters parameters = null;
            _wrapFactoryMock
                .Setup(f => f.CreateWrap(It.IsAny<object>(), It.IsAny<CreateWrapParameters>()))
                .Returns(createWrapReturns)
                .Callback<object, CreateWrapParameters>((o, pms) =>
                {
                    actualObj = o; parameters = pms;
                })
                .Verifiable();

            // Act
            var requestedType = typeof(ITestObject);
            Func<object> testingAction = () => TestedInstance.Wrap(obj, requestedType);
            if (expectedException == null)
            {
                var result = testingAction();
                if (assertResultAction != null)
                {
                    // Assert Result
                    assertResultAction(obj, result);
                }
            }
            else
            {
                Assert.Throws(expectedException, () => testingAction());
            }
            
            // Assert Call
            _wrapFactoryMock.Verify(f => f.CreateWrap(It.IsAny<object>(), It.IsAny<CreateWrapParameters>()), Times.Once());
            Assert.AreSame(obj, actualObj);
            Assert.IsNotNull(parameters);
            Assert.AreEqual(requestedType, parameters.RequiredType);
            Assert.AreSame(TestedInstance, parameters.ProxyWrapper);
            Assert.AreSame(_globalContext, parameters.GlobalContext);
        }

        /// <summary>
        /// Тестирования использования фабрики оберток
        /// в случае если есть реализация для запрашиваемого интерфейса.
        /// </summary>
        [Test]
        public void TestUsingWrapFactoryWhenSupportedType()
        {
            TestUsingWrapFactory(
                (o, pms) => new TestObject(o, pms.ProxyWrapper),
                assertResultAction: (o, result) =>
                    {
                        Assert.IsInstanceOf<TestObject>(result);
                        AssertOneSObject(o, (TestObject)result);
                    }
                );
        }

        /// <summary>
        /// Тестирования использования фабрики оберток
        /// в случае если нет реализации для запрашиваемого интерфейса.
        /// </summary>
        [Test]
        public void TestUsingWrapFactoryWhenNotSupportedType()
        {
            TestUsingWrapFactory(
                (o, pms) => { throw new NotSupportedException(); },
                expectedException: typeof(InvalidOperationException)
                );
        }
    }
}
