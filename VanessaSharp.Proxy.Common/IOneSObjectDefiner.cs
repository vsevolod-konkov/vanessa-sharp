namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Интерфейс определителя, является ли объект
    /// - RCW оберткой над объектом 1С.
    /// </summary>
    internal interface IOneSObjectDefiner
    {
        /// <summary>Проверка является ли объект объектом 1С.</summary>
        /// <param name="obj">Тестируемый объект.</param>
        bool IsOneSObject(object obj);
    }
}
