using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace VsevolodKonkov.OneSSharp.Data.Proxies
{
    /// <summary>Прокси глобального контекста информационной базы 1С.</summary>
    internal sealed class GlobalContext : IDisposable
    {
        /// <summary>Коннектор к информационной базе 1С.</summary>
        private readonly V8.COMConnectorClass _connector;

        /// <summary>COM-объект глобального контекста информационной базы 1С.</summary>
        private readonly object _global;

        /// <summary>Закэшированный тип объект глобального контекста.</summary>
        private readonly Type _typeGlobal;

        /// <summary>Признак, того что неуправляемые ресурсы очищены.</summary>
        private bool _disposed;

        /// <summary>Конструктор.</summary>
        /// <param name="connector">COM-объект устанавливающий соединение.</param>
        /// <param name="globalCtx">COM-объект глобального контекста инфомационной базы 1С.</param>
        private GlobalContext(V8.COMConnectorClass connector, object globalCtx)
        {
            _connector = connector;
            _global = globalCtx;
            _typeGlobal = _global.GetType();
        }
        
        /// <summary>Соединение с информационной базой 1С.</summary>
        /// <param name="parameters">Параметр соединения.</param>
        /// <returns>Прокси глобального контекста информационной базы 1С.</returns>
        public static GlobalContext Connect(ConnectionParameters parameters)
        {
            ChecksHelper.CheckArgumentNotNull(parameters.ConnectionString, "connectionString");
            
            var connector = new V8.COMConnectorClass();
            try
            {
                if (parameters.PoolTimeout.HasValue)
                    connector.PoolTimeout = (uint)parameters.PoolTimeout.Value;

                if (parameters.PoolCapacity.HasValue)
                    connector.PoolCapacity = (uint)parameters.PoolCapacity.Value;
                
                try
                {
                    var globalContext = connector.Connect(parameters.ConnectionString);
                    Trace.Assert(globalContext != null, "globalContext != null");
                    return new GlobalContext(connector, globalContext);
                }
                catch (COMException e)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Ошибка подключения к информационной базе 1C. Строка соединения: \"{0}\". Код ошибки: \"{1}\". Сообщение: \"{2}\".",
                            parameters.ConnectionString, e.ErrorCode, e.Message), e);
                }
            }
            catch
            {
                Marshal.FinalReleaseComObject(connector);
                throw;
            }
        }

        /// <summary>Время ожидания.</summary>
        public int PoolTimeout
        {
            get { return (int)_connector.PoolTimeout; }
            set { _connector.PoolTimeout = (uint)value; }
        }

        /// <summary>Мощность пула соединений.</summary>
        public int PoolCapacity
        {
            get { return (int)_connector.PoolCapacity; }
            set { _connector.PoolCapacity = (uint)value; }
        }

        /// <summary>Признак монопольного режима.</summary>
        public bool IsExclusiveMode
        {
            get 
            {
                return (bool)InvokeMethod("ExclusiveMode");
            }

            set 
            {
                InvokeMethod("SetExclusiveMode", value);
            }
        }

        /// <summary>Начать транзакцию.</summary>
        public void BeginTransaction()
        {
            InvokeMethod("BeginTransaction");
        }

        /// <summary>Принятие транзакции.</summary>
        public void CommitTransaction()
        {
            InvokeMethod("CommitTransaction");
        }

        /// <summary>Отмена транзакции.</summary>
        public void RollbackTransaction()
        {
            InvokeMethod("RollbackTransaction");
        }

        public OneSObjectProxy CreateQuery()
        {
            return new OneSObjectProxy(InvokeMethod("NewObject", "Query"));
        }
        
        /// <summary>Освобождения ресурсов.</summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Marshal.FinalReleaseComObject(_global);
                Marshal.FinalReleaseComObject(_connector);
                _disposed = true;
            }
        }

        /// <summary>Выполнение метода глобального контекста.</summary>
        /// <param name="methodName">Имя метода.</param>
        /// <param name="args">Аргументы метода</param>
        /// <returns>Результат выполнения метода.</returns>
        private object InvokeMethod(string methodName, params object[] args)
        {
            return _typeGlobal.InvokeMember(
                methodName,
                BindingFlags.Default | BindingFlags.InvokeMethod,
                null,
                _global,
                args);
        }
    }
}
