using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using VanessaSharp.Proxy.Common.Interop;

namespace VanessaSharp.Proxy.Common.Tests
{
    /// <summary>Unit-тесты на <see cref="OneSConnector"/>.</summary>
    [TestFixture(Description="Unit-тесты на правильные вызовы")]
    public sealed class OneSConnectorTests
    {
        /// <summary>Строка соединения.</summary>
        private const string CONNECT_STRING = "db.info";

        /// <summary>Иницилизация тестового экземпляра для выполненения соединения.</summary>
        /// <param name="action">Дополнительное действие над моком соединения.</param>
        /// <param name="mockComConnector">Мок.</param>
        private static OneSConnector InitTestedInstanceForConnect(
            Action<ISetup<IV8ComConnector, object>> action, out Mock<IV8ComConnector> mockComConnector)
        {
            mockComConnector = new Mock<IV8ComConnector>(MockBehavior.Strict);
            var setup = mockComConnector.Setup(c => c.Connect(CONNECT_STRING));
            if (action != null)
                action(setup);

            return new OneSConnector(mockComConnector.Object, "V8.X");
        }

        /// <summary>Иницилизация тестового экземпляра для выполненения соединения.</summary>
        /// <param name="action">Дополнительное действие над моком соединения.</param>
        private static OneSConnector InitTestedInstanceForConnect(Action<ISetup<IV8ComConnector, dynamic>> action)
        {
            Mock<IV8ComConnector> mockComConnector;
            return InitTestedInstanceForConnect(action, out mockComConnector);
        }

        /// <summary>Тестирование вызова метода <see cref="OneSConnector.Connect"/>.</summary>
        [Test(Description="Тестирование вызова метода Connect")]
        public void TestConnect()
        {
            var result = new object();

            Mock<IV8ComConnector> mockComConnector;
            var testingInstance = InitTestedInstanceForConnect(s => s.Returns(result), out mockComConnector);

            var actualResult = testingInstance.Connect(CONNECT_STRING);
            Assert.IsInstanceOf<OneSGlobalContext>(actualResult);

            Assert.AreSame(result, ((IOneSProxy)actualResult).Unwrap());
            mockComConnector.Verify(c => c.Connect(CONNECT_STRING), Times.Once());
        }

        /// <summary>Тестирование вызова метода <see cref="OneSConnector.Connect"/> в случае получения исключения при соединении.</summary>
        [Test(Description = "Тестирование пробрасывания исключения при вызове метода Connect")]
        public void TestThrowConnectByComException()
        {
            const string MESSAGE = "Ошибка 1С";
            const int ERROR_CODE = 12345;
            var testingInstance = InitTestedInstanceForConnect(s => s.Throws(new COMException(MESSAGE, ERROR_CODE)));

            var exception = Assert.Throws<InvalidOperationException>(() => testingInstance.Connect(CONNECT_STRING));
            Assert.AreEqual(string.Format(
                        "Ошибка подключения к информационной базе 1C. Строка соединения: \"{0}\". Код ошибки: \"{1}\". Сообщение: \"{2}\".",
                        CONNECT_STRING, ERROR_CODE, MESSAGE), exception.Message);
        }

        /// <summary>Тестирование вызова метода <see cref="OneSConnector.Connect"/> в случае получения нулевой ссылки при соединении.</summary>
        [Test(Description = "Тестирование выбрасывания исключения при получении нулевой ссылки при вызове метода Connect")]
        public void TestThrowConnectByReturnNull()
        {
            var testingInstance = InitTestedInstanceForConnect(s => s.Returns(null));

            var exception = Assert.Throws<InvalidOperationException>(() => testingInstance.Connect(CONNECT_STRING));
            Assert.AreEqual("Соединитель к 1С вернул null при соединении.", exception.Message);
        }

        /// <summary>Проверка установки свойства.</summary>
        /// <param name="propertyGetter">Получатель свойства.</param>
        /// <param name="testingAction">Действие тестирования экземпляра.</param>
        /// <param name="verifingAction">Действие проверки установки свойства.</param>
        /// <param name="propertyValue">Значение свойства.</param>
        private static void AssertSetProperty(Expression<Func<IV8ComConnector2, uint>> propertyGetter,
            Action<OneSConnector, uint> testingAction, Action<IV8ComConnector2, uint> verifingAction, uint propertyValue)
        {
            var mockComConnector = new Mock<IV8ComConnector2>(MockBehavior.Strict);
            mockComConnector.SetupProperty(propertyGetter);

            var testingConnector = new OneSConnector(mockComConnector.Object, "V8.X");
            testingAction(testingConnector, propertyValue);

            mockComConnector.VerifySet(c => verifingAction(c, propertyValue), Times.Once());
        }

        /// <summary>
        /// Тестирование установки значения свойства <see cref="OneSConnector.PoolTimeout"/> у коннектора 1С.
        /// </summary>
        [Test(Description = "Тестирование установки значения свойства PoolTimeout у коннектора 1С")]
        public void TestSetPoolTimeout()
        {
            AssertSetProperty(
                c => c.PoolTimeout,
                (t, v) => t.PoolTimeout = v,
                (c, v) => c.PoolTimeout = v,
                5);
        }

        /// <summary>
        /// Тестирование установки значения свойства <see cref="OneSConnector.PoolCapacity"/> у коннектора 1С.
        /// </summary>
        [Test(Description = "Тестирование установки значения свойства PoolCapacity у коннектора 1С")]
        public void TestSetPoolCapacity()
        {
            AssertSetProperty(
                c => c.PoolCapacity, 
                (t, v) => t.PoolCapacity = v, 
                (c, v) => c.PoolCapacity = v,
                10);
        }
    }
}
