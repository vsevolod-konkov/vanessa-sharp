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
        /// Глобальный контекст 1С.
        /// </summary>
        private OneSGlobalContext _globalContext;
        
        /// <summary>
        /// Мок для реализации <see cref="IOneSWrapFactory"/>.
        /// </summary>
        private Mock<IOneSWrapFactory> _wrapFactoryMock;

        /// <summary>
        /// Мок для реализации <see cref="IOneSEnumMapper"/>.
        /// </summary>
        private Mock<IOneSEnumMapper> _enumMapper; 

        /// <summary>Инициализация тестируемого экземпляра.</summary>
        internal override OneSProxyWrapper InitTestedInstance()
        {
            _globalContext = new OneSGlobalContext(new object());
            _wrapFactoryMock = new Mock<IOneSWrapFactory>(MockBehavior.Strict);
            _enumMapper = new Mock<IOneSEnumMapper>(MockBehavior.Strict);

            return new OneSProxyWrapperWithGlobalContext(
                ObjectDefinerMock.Object,
                _globalContext, _wrapFactoryMock.Object, 
                _enumMapper.Object);
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
                .Returns<object, CreateWrapParameters>((o, pms) =>
                {
                    actualObj = o; 
                    parameters = pms;

                    return createWrapReturns(o, pms);
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

        /// <summary>
        /// Тестирование использования <see cref="IOneSEnumMapper"/>
        /// для конвертации 1С-объекта в перечисление.
        /// </summary>
        [Test]
        public void TestUsingEnumMapper()
        {
            const TestEnum EXPECTED_VALUE = TestEnum.Enum2;
            
            // Arrange
            var obj = CreateOneSObject();

            _enumMapper
                .Setup(m => m.ConvertComObjectToEnum(obj, typeof(TestEnum)))
                .Returns(EXPECTED_VALUE);

            // Act
            var actualValue = TestedInstance.Wrap(obj, typeof(TestEnum));

            // Assert
            Assert.AreEqual(EXPECTED_VALUE, actualValue);

            _enumMapper
                .Verify(m => m.ConvertComObjectToEnum(obj, typeof(TestEnum)), Times.Once());
        }

        /// <summary>
        /// Тестовое перечисление.
        /// </summary>
        public enum TestEnum
        {
            Enum1,
            Enum2
        }
    }
}
