using System;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.AcceptanceTests.Mocks
{
    /// <summary>
    /// Моковая реализация <see cref="IOneSConnectorFactory"/>.
    /// </summary>
    internal sealed class OneSConnectorFactoryMock : IOneSConnectorFactory
    {
        /// <summary>Создание соединения в зависимости от версии.</summary>
        /// <param name="version">Версия.</param>
        /// <returns>Возвращает объект коннектора к информационной БД определенной версии.</returns>
        /// <exception cref="ArgumentNullException">В случае, если значение <paramref name="version"/> было пустым.</exception>
        /// <exception cref="InvalidOperationException">В случае, если фабрика не может создать экземпляр коннектора заданной версии.</exception>
        public IOneSConnector Create(string version)
        {
            return new OneSConnectorMock();
        }

        /// <summary>
        /// Моковая реализация <see cref="IOneSConnector"/>.
        /// </summary>
        private sealed class OneSConnectorMock : IOneSConnector
        {
            public void Dispose() {}

            public IGlobalContext Connect(string connectString)
            {
                return new GlobalContextMock();
            }

            public uint PoolTimeout { get; set; }
            public uint PoolCapacity { get; set; }
            public string Version { get { return "2.0"; } }
        }

        /// <summary>
        /// Моковая реализация <see cref="IGlobalContext"/>.
        /// </summary>
        private sealed class GlobalContextMock : IGlobalContext
        {
            private bool _isExclusiveMode;
            
            public void Dispose() {}

            public dynamic NewObject(string typeName)
            {
                throw new NotImplementedException("Метод NewObject не поддерживается.");
            }

            public T NewObject<T>() where T : IGlobalContextBound
            {
                throw new NotImplementedException();
            }

            public bool ExclusiveMode()
            {
                return _isExclusiveMode;
            }

            public void SetExclusiveMode(bool value)
            {
                _isExclusiveMode = value;
            }

            public void BeginTransaction()
            {}

            public void CommitTransaction()
            {}

            public void RollbackTransaction()
            {}
        }
    }


}
