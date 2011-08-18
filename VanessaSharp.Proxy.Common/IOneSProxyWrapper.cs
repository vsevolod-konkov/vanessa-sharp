namespace VanessaSharp.Proxy.Common
{
    /// <summary>Обертыватель объектов 1С.</summary>
    public interface IOneSProxyWrapper
    {
        /// <summary>Создание обертки над объектом.</summary>
        /// <param name="obj">Обертываемый объект.</param>
        object Wrap(object obj);
    }
}
