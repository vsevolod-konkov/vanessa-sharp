using System;
using System.Runtime.InteropServices;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Обертка над объектом, так чтобы он реализовал интерфейс <see cref="IDisposable"/>.</summary>
    public abstract class DisposableWrapper<T> : IDisposable
    {
        /// <summary>Конструктор для того, чтобы нельзя было создать наследный класс вне сборки.</summary>
        internal DisposableWrapper(T obj)
        {
            _obj = obj;
        }

        /// <summary>Признак того, что ресурсы были освобождены и объект нельзя вызывать.</summary>
        private bool _disposed;

        /// <summary>
        /// Нижележащий объект.
        /// </summary>
        public T Object
        {
            get
            {
                if (_disposed)
                {
                    throw new InvalidOperationException(string.Format(
                        "Нельзя получить нижележащий объект для экземпляра объекта типа \"{0}\", так как был вызван метод Dispose.", 
                        typeof(DisposableWrapper<T>)));
                }

                return _obj;
            }
        }
        private T _obj;

        /// <summary>Освобождение ресурсов.</summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                InternalDispose();
                _obj = default(T);
                _disposed = true;
            }
        }

        /// <summary>Реальное освобождение ресурсов нижележащего объекта.</summary>
        /// <param name="obj"></param>
        protected abstract void InternalDispose();

        /// <summary>Фабричный метод создания обертки.</summary>
        /// <param name="obj">Объект.</param>
        public static DisposableWrapper<T> Create(T obj)
        {
            if (obj != null)
            {
                DisposableWrapper<T> result;
                
                if (ComDisposableWrapper.TryWrap(obj, out result))
                    return result;

                if (SupportDisposableWrapper.TryWrap(obj, out result))
                    return result;
            }

            return new DefaultDisposableWrapper(obj);
        }

        #region Вспомогательные типы

        /// <summary>Реализация по умолчанию.</summary>
        private sealed class DefaultDisposableWrapper : DisposableWrapper<T>
        {
            public DefaultDisposableWrapper(T obj)
                : base(obj)
            {}

            protected override void InternalDispose()
            {}
        }

        /// <summary>Обертка для COM-объекта.</summary>
        private sealed class ComDisposableWrapper : DisposableWrapper<T>
        {
            /// <summary>COM-объект.</summary>
            private object _comObject;
            
            private ComDisposableWrapper(T obj) : base(obj)
            {
                _comObject = obj;
            }

            /// <summary>Подходит для обертки.</summary>
            public static bool TryWrap(T obj, out DisposableWrapper<T> result)
            {
                if (Marshal.IsComObject(obj))
                {
                    result = new ComDisposableWrapper(obj);
                    return true;
                }

                result = null;
                return false;
            }

            protected override void InternalDispose()
            {
                Marshal.FinalReleaseComObject(_comObject);
                _comObject = null;
            }
        }

        /// <summary>Обертка для объекта поддерживающего <see cref="IDisposable"/>.</summary>
        private sealed class SupportDisposableWrapper : DisposableWrapper<T>
        {
            /// <summary>Объект поддерживающий освобождение ресурсов.</summary>
            private IDisposable _disposable;

            private SupportDisposableWrapper(T obj, IDisposable disposable)
                : base(obj)
            {
                _disposable = disposable;
            }

            /// <summary>Подходит для обертки.</summary>
            public static bool TryWrap(T obj, out DisposableWrapper<T> result)
            {
                var disposable = obj as IDisposable;
                if (disposable != null)
                {
                    result = new SupportDisposableWrapper(obj, disposable);
                    return true;
                }

                result = null;
                return false;
            }

            protected override void InternalDispose()
            {
                _disposable.Dispose();
                _disposable = null;
            }
        }

        #endregion
    }
}
