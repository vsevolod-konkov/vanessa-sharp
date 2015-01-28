namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Тип записи запроса.
    /// </summary>
    public enum SelectRecordType
    {
        /// <summary>
        /// Детальная (конечная) запись результата запроса.
        /// </summary>
        DetailRecord,

        /// <summary>
        /// Итоговая запись по группировке запроса.
        /// </summary>
        GroupTotal,

        /// <summary>
        /// Итоговая запись по иерархии запроса.
        /// </summary>
        TotalByHierarchy,

        /// <summary>
        /// Общая итоговая запись запроса.
        /// </summary>
        Overall
    }
}
