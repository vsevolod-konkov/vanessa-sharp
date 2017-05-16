using System;
using System.Text.RegularExpressions;
using VanessaSharp.Proxy.Common.Interop;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Стандартная реализация <see cref="IOneSConnector"/>.</summary>
    internal sealed class OneSConnector : OneSConnectorBase
    {
        /// <summary>
        /// Создание экземпляра <see cref="OneSConnector"/> по ProgID
        /// CO-класса коннектора 1С.
        /// </summary>
        public static OneSConnector CreateFromProgId(string progId)
        {
            return new OneSConnector(
                progId, GetVersionFromProgId(progId));
        }

        /// <summary>Получение строки версии из ProgID.</summary>
        private static string GetVersionFromProgId(string progId)
        {
            var match = _progIdRegEx.Match(progId);
            return (match.Success)
                ? "8." + match.Groups["version"].Value
                : "unknown";
        }
        private static readonly Regex _progIdRegEx = new Regex(@"^V8?<version>d", RegexOptions.Compiled);

        /// <summary>
        /// Созданеие экземпляра <see cref="OneSConnector"/> по номеру версии.
        /// </summary>
        /// <param name="version">Версия.</param>
        public static OneSConnector CreateFromVersion(OneSVersion version)
        {
            return new OneSConnector(
                GetProgId(version), GetVersionString(version));
        }

        /// <summary>
        /// Получение ProgID co-класса COM-соединителя 1С по версии платформы.
        /// </summary>
        private static string GetProgId(OneSVersion version)
        {
            return string.Format("{0}.COMConnector", version);
        }

        /// <summary>Попытка создания экземпляра COM-соединителя.</summary>
        /// <param name="version">Версия технологической платформы.</param>
        internal static OneSConnector TryCreateFromVersion(OneSVersion version)
        {
            var progId = GetProgId(version);
            var type = Type.GetTypeFromProgID(progId);

            return (type == null)
                ? null
                : new OneSConnector(Activator.CreateInstance(type), GetVersionString(version));
        }

        /// <summary>
        /// Получение строки версии по <see cref="OneSVersion"/>.
        /// </summary>
        private static string GetVersionString(OneSVersion version)
        {
            return string.Format("8.{0}", (int)version);
        }

        /// <summary>
        /// Создание RCW-обертки co-класса конектора 1С.
        /// </summary>
        /// <param name="progId">ProgId</param>
        private static object CreateComConnector(string progId)
        {
            var type = Type.GetTypeFromProgID(progId);
            if (type == null)
            {
                throw new InvalidOperationException(string.Format(
                    "В системе не найден тип co-класса COM-соединителя 1С с ProgID: {0}.", progId));
            }

            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// Конструктор, принимающий ProgID co-класса коннектора 1С.
        /// </summary>
        /// <param name="progId">ProgID co-класса коннектора 1С.</param>
        /// <param name="version">Строка версии 1С.</param>
        private OneSConnector(string progId, string version)
            : this(CreateComConnector(progId), version)
        {
            
        }

        /// <summary>Конструктор принимающий RCW-обертки COM-коннектора.</summary>
        /// <param name="comConnector">
        /// RCW-обертка соединителя к информационной БД 1С.
        /// </param>
        /// <param name="version">Строка версии 1С.</param>
        internal OneSConnector(object comConnector, string version)
            : base(comConnector)
        {
            _version = version;
        }

        /// <summary>
        /// Выполнение метода соединения объекта <see cref="OneSConnectorBase.ComConnector"/>.
        /// </summary>
        /// <param name="connectString">Строка соединения.</param>
        protected override object ComConnect(string connectString)
        {
            dynamic comConnector = ComConnector;

            return comConnector.Connect(connectString);
        }

        private void SetComConnector2Property(Action<IV8ComConnector2, uint> setter, uint value)
        {
            var comConnector2 = ComConnector as IV8ComConnector2;
            if (comConnector2 != null)
                setter(comConnector2, value);
        }

        private uint GetComConnector2Property(Func<IV8ComConnector2, uint> getter)
        {
            var comConnector2 = ComConnector as IV8ComConnector2;
            return (comConnector2 == null)
                ? 0
                : getter(comConnector2);
        }

        /// <summary>Время ожидания подключения.</summary>
        public override uint PoolTimeout
        {
            get { return GetComConnector2Property(c => c.PoolTimeout); }

            set
            {
                SetComConnector2Property((c, v) => c.PoolTimeout = v, value);
            }
        }

        /// <summary>Мощность подключения.</summary>
        public override uint PoolCapacity
        {
            get { return GetComConnector2Property(c => c.PoolCapacity); }

            set
            {
                SetComConnector2Property((c, v) => c.PoolCapacity = v, value);
            }
        }

        /// <summary>Версия.</summary>
        public override string Version
        {
            get { return _version; }
        }
        private readonly string _version;
    }
}
