using System;

namespace VanessaSharp.Data.UnitTests
{
    /// <summary>
    /// Мок объекта поддерживающего интерфейс
    /// <see cref="IDisposable"/>.
    /// </summary>
    internal interface IDisposableMock
    {
        /// <summary>
        /// Проверка того, что вызывался метод <see cref="IDisposable.Dispose"/>
        /// </summary>
        void VerifyDispose();
    }
}