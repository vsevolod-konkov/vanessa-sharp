namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Обход результата запроса.
    /// </summary>
    [OneSEnum]
    public enum QueryResultIteration
    {
        /// <summary>
        /// По умолчанию.
        /// </summary>
        Default,
        
        /// <summary>
        /// Линейный.
        /// </summary>
        [OneSEnumValue]
        Linear,

        /// <summary>
        /// По группировкам.
        /// </summary>
        [OneSEnumValue]
        ByGroups,

        /// <summary>
        /// По группировкам с иерархией.
        /// </summary>
        [OneSEnumValue]
        ByGroupsWithHierarchy
    }
}
