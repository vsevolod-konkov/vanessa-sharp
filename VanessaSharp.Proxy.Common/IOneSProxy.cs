namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Интерфейс прокси-объекта над 1С-объектом.
    /// </summary>
    internal interface IOneSProxy
    {
        /// <summary>
        /// Получение исходного RCW-обертки 1С-объекта.
        /// </summary>
        object Unwrap();
    }
}
