using System;
using Moq;
using NUnit.Framework;

namespace VanessaSharp.Proxy.Common.Tests
{
    /// <summary>
    /// Тестирование 
    /// <see cref="OneSProxyWrapper"/>.
    /// </summary>
    [TestFixture]
    public class OneSProxyWrapperTests
    {
        /// <summary>Мок реализации <see cref="IOneSObjectDefiner"/>.</summary>
        internal Mock<IOneSObjectDefiner> ObjectDefinerMock { get; private set; }
        
        /// <summary>Тестируемый экземпляр.</summary>
        internal OneSProxyWrapper TestedInstance { get; private set; }

        /// <summary>Инициализация тестов.</summary>
        [SetUp]
        public void SetUp()
        {
            ObjectDefinerMock = new Mock<IOneSObjectDefiner>(MockBehavior.Strict);
            TestedInstance = InitTestedInstance();
        }

        /// <summary>Инициализация тестируемого экземпляра.</summary>
        internal virtual OneSProxyWrapper InitTestedInstance()
        {
            return new OneSProxyWrapper(ObjectDefinerMock.Object);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSProxyWrapper.WrapOneSObject"/>
        /// когда передаваемая ссылка на объект равна <c>null</c>.
        /// </summary>
        [Test]
        public void TestWrapWhenObjectIsNull()
        {
            Assert.IsNull(TestedInstance.Wrap(null, typeof(object)));
        }

        /// <summary>
        /// Установка того, является ли объект - объектом 1С.
        /// </summary>
        /// <param name="obj">объект для которого производится установка.</param>
        /// <param name="isOneSObject">Значение устанавляиваемого признака.</param>
        protected void SetupIsOneSObject(object obj, bool isOneSObject)
        {
            ObjectDefinerMock
                .Setup(d => d.IsOneSObject(obj))
                .Returns(isOneSObject)
                .Verifiable();
        }
        
        /// <summary>
        /// Тестирование метода <see cref="OneSProxyWrapper.WrapOneSObject"/>
        /// когда передаваемая ссылка на объект не равна <c>null</c>.
        /// </summary>
        /// <param name="isOneSObject">
        /// Если обертываемый обект является объектом 1С.
        /// </param>
        /// <param name="assertAction">
        /// Действие проверки,
        /// первый аргумент - обертываемый объект,
        /// второй аргумент - полученая обертка в результате выполнения тестируемого метода.
        /// </param>
        private void TestWrapObjectIsNotNull(bool isOneSObject, Action<object, object> assertAction)
        {
            // Arrange
            var obj = new object();
            SetupIsOneSObject(obj, isOneSObject);

            // Act
            var result = TestedInstance.Wrap(obj, typeof(object));

            // Assert
            assertAction(obj, result);
            ObjectDefinerMock.Verify(d => d.IsOneSObject(obj), Times.Once());
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSProxyWrapper.WrapOneSObject"/>
        /// когда объект не является объектом 1С.
        /// </summary>
        [Test]
        public void TestWrapWhenObjectIsNotFromOneS()
        {
            TestWrapObjectIsNotNull(
                false,
                Assert.AreSame);
        }

        /// <summary>Проверка обертки над объектом.</summary>
        /// <param name="source">Исходный объект.</param>
        /// <param name="result">Проверяемый результат обертки.</param>
        protected void AssertOneSObject(object source, OneSObject result)
        {
            Assert.AreSame(TestedInstance, result.ProxyWrapper);

            IOneSProxy proxy = result;
            Assert.AreSame(source, proxy.Unwrap());
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSProxyWrapper.WrapOneSObject"/>
        /// когда объект является объектом 1С.
        /// </summary>
        [Test]
        public void TestWrapWhenObjectIsFromOneS()
        {
            TestWrapObjectIsNotNull(
                true,
                (obj, result) =>
                    {
                        Assert.IsInstanceOf<OneSObject>(result);
                        AssertOneSObject(obj, (OneSObject)result);
                    });
        }
    }
}
