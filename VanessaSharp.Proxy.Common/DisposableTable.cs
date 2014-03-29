using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Таблица, в которой собираются все ссылки на объекты 1C,
    /// которые необходимо освободить, в случае закрытия глобального контекста.
    /// </summary>
    internal sealed class DisposableTable : IDisposable
    {
        /// <summary>Список слабых ссылок на объекты 1С.</summary>
        private readonly LinkedList<WeakReference> _references = new LinkedList<WeakReference>();

        /// <summary>Объект для синхронного доступа к таблице.</summary>
        private readonly object _syncObject = new object();

        /// <summary>Таймер для цикла очистки.</summary>
        private readonly Timer _cleaningTimer;

        /// <summary>Флаг, что таблица была очищена.</summary>
        private bool _disposed = false;

        /// <summary>Период очистки.</summary>
        private const int CLEANING_PERIOD = 5000;

        /// <summary>Конструктор.</summary>
        public DisposableTable()
        {
            _cleaningTimer = new Timer(s => Cleaning(), null, Timeout.Infinite, CLEANING_PERIOD);
        }

        /// <summary>Регистрация объекта в таблице.</summary>
        /// <param name="disposableObj">Регистрируемый объект.</param>
        public void RegisterObject(IDisposable disposableObj)
        {
            Contract.Requires<ArgumentNullException>(disposableObj != null);
            
            lock (_syncObject)
            {
                if (_disposed)
                {
                    throw new InvalidOperationException(
                        "Ресурсы таблицы были освобождены. После освобождения ресурсов таблицы регистрация новых объектов недопустима.");
                }
                
                _references.AddLast(new WeakReference(disposableObj));
            }
        }

        /// <summary>
        /// Освобождение ресурсов объектов зарегистрированных в таблице.
        /// </summary>
        public void Dispose()
        {
            _cleaningTimer.Dispose();

            lock (_syncObject)
            {
                foreach (var weakReference in _references)
                {
                    if (weakReference.IsAlive)
                    {
                        var obj = weakReference.Target;
                        if (obj != null)
                        {
                            ((IDisposable)obj).Dispose();
                        }
                    }
                }
                _references.Clear();

                _disposed = true;
            }
        }

        /// <summary>Очистка таблицы от собранных сборщиком ссылок.</summary>
        private void Cleaning()
        {
            lock (_syncObject)
            {
                var node = _references.First;

                while (node != null)
                {
                    var nextNode = node.Next;
                    if (!node.Value.IsAlive)
                        _references.Remove(node);

                    node = nextNode;
                }
            }
        }
    }
}
