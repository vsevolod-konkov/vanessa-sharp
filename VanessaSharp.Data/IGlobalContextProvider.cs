using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    // TODO: Нужен Рефакторинг. Убрать интерфейс после внедрения параметрических запросов. Нужна просто фабрика объектов.
    /// <summary>
    /// Поставщик <see cref="IGlobalContext"/>
    /// для взаимодействия с 1C.
    /// </summary>
    internal interface IGlobalContextProvider
    {
        /// <summary>
        /// Глобальный контекст 1С.
        /// </summary>
        IGlobalContext GlobalContext { get; }
    }
}
