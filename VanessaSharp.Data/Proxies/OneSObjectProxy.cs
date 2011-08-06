using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace VanessaSharp.Data.Proxies
{
    /// <summary>Объект-обертка объекта 1С.</summary>
    /// <remarks>Временный вариант.</remarks>
    internal class OneSObjectProxy : IDisposable
    {
        private readonly object _obj;
        private readonly Type _type;
        private bool _disposed;

        public OneSObjectProxy(object obj)
        {
            ChecksHelper.CheckArgumentNotNull(obj, "obj");

            _obj = obj;
            _type = obj.GetType();
        }

        /// <summary>Выполнение метода.</summary>
        /// <param name="methodName">Имя метода.</param>
        /// <param name="args">Аргументы метода</param>
        /// <returns>Результат выполнения метода.</returns>
        public object InvokeMethod(string methodName, params object[] args)
        {
            return _type.InvokeMember(
                methodName,
                BindingFlags.Default | BindingFlags.InvokeMethod,
                null,
                _obj,
                args);
        }

        public object GetProperty(string propertyName)
        {
            ChecksHelper.CheckArgumentNotEmpty(propertyName, "propertyName");
            
            return _type.InvokeMember(
                propertyName,
                BindingFlags.Default | BindingFlags.GetProperty,
                null,
                _obj,
                null);
        }

        public void SetProperty(string propertyName, object value)
        {
            ChecksHelper.CheckArgumentNotEmpty(propertyName, "propertyName");

            _type.InvokeMember(
                propertyName,
                BindingFlags.Default | BindingFlags.SetProperty,
                null,
                _obj,
                new object[] { value });
        }

        /// <summary>Освобождения ресурсов.</summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Marshal.FinalReleaseComObject(_obj);
                _disposed = true;
            }
        }
    }
}
