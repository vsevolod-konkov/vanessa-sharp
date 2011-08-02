namespace VsevolodKonkov.OneSSharp.Data.Proxies
{
    /// <summary>Параметры соединения с информационной базой 1С.</summary>
    internal sealed class ConnectionParameters
    {
        /// <summary>Строка соединения.</summary>
        public string ConnectionString { get; set; }

        /// <summary>Время ожидания подключения.</summary>
        public int? PoolTimeout { get; set; }

        /// <summary>Мощность подключения.</summary>
        public int? PoolCapacity { get; set; }
    }
}
