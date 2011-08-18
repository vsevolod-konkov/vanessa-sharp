using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using VanessaSharp.Proxy.Common;
using V82;

namespace VanessaSharp.Proxy.V82
{
    /// <summary>Реализация интерфейса <see cref="IOneSConnector"/> для версии 1С 8.2.</summary>
    public sealed class OneSConnector : IOneSConnector
    {
        /// <summary>COM-соединитель с 1С.</summary>
        private readonly DisposableWrapper<IV8COMConnector2> _comConnector;

        /// <summary>Конструктор принимающий COM-объект соединителя к 1С.</summary>
        /// <param name="comConnector">COM-объект соединителя к 1С.</param>
        internal OneSConnector(IV8COMConnector2 comConnector)
        {
            Contract.Requires<ArgumentNullException>(comConnector != null, "comConnector не может быть null");

            _comConnector = comConnector.WrapToDisposable();
        }

        /// <summary>Создание COM-соединителя с 1С.</summary>
        private static IV8COMConnector2 CreateComConnector()
        {
            try
            {
                return new COMConnectorClass();
            }
            catch (COMException e)
            {
                throw new InvalidOperationException(string.Format(
                    "Не удалось создать экземпляр COM-объекта 1С. Убедитесь что 1С установлена на машине. Исходная ошибка: \"{0}\".", 
                    e.Message), e);
            }
        }

        /// <summary>Конструктор без аргументов.</summary>
        public OneSConnector()
            : this(CreateComConnector())
        {}

        /// <summary>Соединение с информационной базой.</summary>
        /// <param name="connectString">Строка соединения.</param>
        /// <returns>Возвращает объект глобального контекста.</returns>
        public IGlobalContext Connect(string connectString)
        {
            var result = InvokeConnect(connectString);
            if (result == null)
                throw new InvalidOperationException("Соединитель к 1С вернул null при соединении.");

            return OneSObject.Wrap(result);
        }

        /// <summary>
        /// Нижележащий объект.
        /// </summary>
        private IV8COMConnector2 ComConnector
        {
            get { return _comConnector.Object; }
        }

        /// <summary>
        /// Выполнение соединения с информационной базой 1С.
        /// </summary>
        /// <param name="connectString">Строка соединения.</param>
        /// <returns>Возвращает объект глобального контекста.</returns>
        private dynamic InvokeConnect(string connectString)
        {
            Contract.Requires(!string.IsNullOrEmpty(connectString));

            try
            {
                return ComConnector.Connect(connectString);
            }
            catch (COMException e)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Ошибка подключения к информационной базе 1C. Строка соединения: \"{0}\". Код ошибки: \"{1}\". Сообщение: \"{2}\".",
                        connectString, e.ErrorCode, e.Message), e);
            }
        }

        /// <summary>Время ожидания подключения.</summary>
        public uint PoolTimeout
        {
            get
            {
                return ComConnector.PoolTimeout;
            }
            set
            {
                ComConnector.PoolTimeout = value;
            }
        }

        /// <summary>Мощность подключения.</summary>
        public uint PoolCapacity
        {
            get
            {
                return ComConnector.PoolCapacity;
            }
            set
            {
                ComConnector.PoolCapacity = value;
            }
        }

        /// <summary>Освобождение ресурсов.</summary>
        public void Dispose()
        {
            _comConnector.Dispose();
        }
    }
}
