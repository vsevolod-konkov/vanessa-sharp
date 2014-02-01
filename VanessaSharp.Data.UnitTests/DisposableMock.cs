using System;
using Moq;

namespace VanessaSharp.Data.UnitTests
{
    /// <summary>
    /// Мок объекта у которого должен
    /// вызываться метод <see cref="IDisposable.Dispose"/>.
    /// </summary>
    internal sealed class DisposableMock<T> : Mock<T>, IDisposableMock
        where T : class, IDisposable
    {
        /// <summary>Создание мока реализующего <see cref="IDisposable"/>.</summary>
        public DisposableMock()
            : base(MockBehavior.Strict)
        {
            SetupDispose();
        }
        
        /// <summary>
        /// Установка реализации <see cref="IDisposable.Dispose"/>
        /// для мока.
        /// </summary>
        private void SetupDispose()
        {
            Setup(o => o.Dispose())
                .Verifiable();
        }

        /// <summary>
        /// Проверка того, что вызывался метод <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void VerifyDispose()
        {
            Verify(o => o.Dispose(), Times.AtLeastOnce());
        }
    }
}