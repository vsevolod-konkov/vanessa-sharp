using System;
using System.Runtime.InteropServices;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Вспомогательные методы.</summary>
    public static class OneSProxyHelper
    {
        /// <summary>Проверка является ли объект объектом 1С.</summary>
        /// <param name="obj">Тестируемый объект.</param>
        internal static bool IsOneSObject(object obj)
        {
            return Marshal.IsComObject(obj);
        }

        /// <summary>
        /// Обертывание объекта в обертку поддерживающую <see cref="IDisposable"/>
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="obj">Оборачиваемый объект.</param>
        public static DisposableWrapper<T> WrapToDisposable<T>(this T obj)
        {
            return DisposableWrapper<T>.Create(obj);
        }
    }
}
