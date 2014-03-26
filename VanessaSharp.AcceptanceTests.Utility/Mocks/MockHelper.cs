using System;
using Moq;

namespace VanessaSharp.AcceptanceTests.Utility.Mocks
{
    /// <summary>Вспомогательные методы для создания моков.</summary>
    public static class MockHelper
    {
        private static void SetupDispose<T>(Mock<T> mock)
            where T : class, IDisposable
        {
            mock
                .Setup(o => o.Dispose())
                .Verifiable();
        }

        /// <summary>
        /// Создание мока объекта поддерживающего интерфейс <see cref="IDisposable"/>.
        /// </summary>
        /// <typeparam name="T">Тип мокового объекта.</typeparam>
        public static Mock<T> CreateDisposableMock<T>()
            where T : class, IDisposable
        {
            var result = new Mock<T>(MockBehavior.Strict);
            SetupDispose(result);

            return result;
        }

        public static void VerifyDispose<T>(Mock<T> mock)
            where T : class, IDisposable
        {
            mock.Verify(d => d.Dispose(), Times.AtMostOnce());
        }
    }
}
