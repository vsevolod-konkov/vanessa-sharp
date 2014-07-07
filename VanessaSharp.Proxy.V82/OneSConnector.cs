using VanessaSharp.Interop.V82;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Proxy.V82
{
    /// <summary>Реализация интерфейса <see cref="IOneSConnector"/> для версии 1С 8.2.</summary>
    public sealed class OneSConnector : OneSConnectorBase, IOneSConnector
    {
        /// <summary>Конструктор принимающий COM-объект соединителя к 1С.</summary>
        /// <param name="comConnector">COM-объект соединителя к 1С.</param>
        internal OneSConnector(IV8COMConnector2 comConnector) : base(comConnector)
        {}

        /// <summary>Создание COM-соединителя с 1С.</summary>
        private static IV8COMConnector2 CreateComConnector()
        {
            return GetNewComConnector(() => new COMConnector());
        }

        /// <summary>Конструктор без аргументов.</summary>
        public OneSConnector()
            : this(CreateComConnector())
        {}

        /// <summary>
        /// RCW-обертка соединителя к информационной БД 1С.
        /// </summary>
        private new IV8COMConnector2 ComConnector
        {
            get { return (IV8COMConnector2)base.ComConnector; }
        }

        /// <summary>
        /// Выполнение метода соединения объекта <see cref="OneSConnectorBase.ComConnector"/>.
        /// </summary>
        /// <param name="connectString">Строка соединения.</param>
        protected override object ComConnect(string connectString)
        {
            return ComConnector.Connect(connectString);
        }

        /// <summary>Время ожидания подключения.</summary>
        public override uint PoolTimeout
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
        public override uint PoolCapacity
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

        /// <summary>Версия.</summary>
        public override string Version
        {
            get { return "8.2"; }
        }
    }
}
