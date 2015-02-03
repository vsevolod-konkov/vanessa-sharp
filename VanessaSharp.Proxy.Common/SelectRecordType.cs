namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Тип записи запроса.
    /// </summary>
    [OneSEnum]
    public enum SelectRecordType
    {
        /// <summary>
        /// Детальная (конечная) запись результата запроса.
        /// </summary>
        [OneSEnumValue]
        DetailRecord,

        /// <summary>
        /// Итоговая запись по группировке запроса.
        /// </summary>
        [OneSEnumValue]
        GroupTotal,

        /// <summary>
        /// Итоговая запись по иерархии запроса.
        /// </summary>
        [OneSEnumValue]
        TotalByHierarchy,

        /// <summary>
        /// Общая итоговая запись запроса.
        /// </summary>
        [OneSEnumValue]
        Overall
    }
}
