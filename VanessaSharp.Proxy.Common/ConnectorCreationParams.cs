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
        public string Version { get; set; }

        /// <summary>
        /// Имя рекомендуемый типа реализации коннектора.
        /// </summary>
        public string TypeName { get; set; }
    }
}
