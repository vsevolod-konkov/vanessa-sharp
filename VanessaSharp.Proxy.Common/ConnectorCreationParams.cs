namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Параметры создания коннектора к информационной базе 1С.
    /// </summary>
    public sealed class ConnectorCreationParams
    {
        /// <summary>
        /// Рекомендуемая версия коннектора.
        /// </summary>
        public OneSVersion? Version { get; set; }

        /// <summary>
        /// Рекомендуемый ProgId коннектора.
        /// </summary>
        public string ConnectorProgId { get; set; }

        /// <summary>
        /// Имя рекомендуемого типа реализации коннектора.
        /// </summary>
        public string TypeName { get; set; }
    }
}
