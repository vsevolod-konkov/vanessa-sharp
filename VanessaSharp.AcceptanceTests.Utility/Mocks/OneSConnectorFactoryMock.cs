using System;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.AcceptanceTests.Utility.Mocks
{
    /// <summary>
    /// Моковая реализация <see cref="IOneSConnectorFactory"/>.
    /// </summary>
    public sealed class OneSConnectorFactoryMock : IOneSConnectorFactory
    {
        /// <summary>Событие запроса на создания объекта 1С.</summary>
        public event EventHandler<NewOneSObjectEventArgs> NewOneSObjectAsking;
        
        /// <summary>Создание соединения в зависимости от версии.</summary>
        /// <param name="version">Версия.</param>
        /// <returns>Возвращает объект коннектора к информационной БД определенной версии.</returns>
        /// <exception cref="ArgumentNullException">В случае, если значение <paramref name="version"/> было пустым.</exception>
        /// <exception cref="InvalidOperationException">В случае, если фабрика не может создать экземпляр коннектора заданной версии.</exception>
        public IOneSConnector Create(string version)
        {
            return new OneSConnectorMock(AskOneSNewObject);
        }

        /// <summary>Запрос на создание объекта 1С.</summary>
        /// <param name="requiredType">Тип, который должен поддерживать создаваемый объект.</param>
        private object AskOneSNewObject(Type requiredType)
        {
            if (NewOneSObjectAsking == null)
                return null;

            var args = new NewOneSObjectEventArgs(requiredType);
            NewOneSObjectAsking(this, args);

            return args.CreatedInstance;
        }

        /// <summary>
        /// Моковая реализация <see cref="IOneSConnector"/>.
        /// </summary>
        private sealed class OneSConnectorMock : IOneSConnector
        {
            private readonly Func<Type, object> _newObjectDelegate;

            public OneSConnectorMock(Func<Type, object> newObjectDelegate)
            {
                _newObjectDelegate = newObjectDelegate;
            }

            public void Dispose() {}

            public IGlobalContext Connect(string connectString)
            {
                return new GlobalContextMock(_newObjectDelegate);
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
            private readonly Func<Type, object> _newObjectDelegate; 

            private bool _isExclusiveMode;

            public GlobalContextMock(Func<Type, object> newObjectDelegate)
            {
                _newObjectDelegate = newObjectDelegate;
            }

            public void Dispose() {}

            public dynamic NewObject(string typeName)
            {
                throw new NotImplementedException("Метод NewObject не поддерживается.");
            }

            public T NewObject<T>() where T : IGlobalContextBound
            {
                var requiredType = typeof(T);

                var instance = _newObjectDelegate(requiredType);
                if (instance == null)
                {
                    throw new NotSupportedException(string.Format(
                        "Создание экземпляра типа \"{0}\" методом NewObject не поддерживается.",
                        requiredType));
                }

                return (T)instance;
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

            public string String(object obj)
            {
                return (obj == null)
                           ? null
                           : obj.ToString();
            }
        }
    }
}
