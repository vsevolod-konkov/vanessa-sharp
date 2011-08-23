using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Интерфейс глобального контекста.</summary>
    [ContractClass(typeof(IGlobalContextContract))]
    public interface IGlobalContext
    {
        /// <summary>Создание объекта.</summary>
        /// <param name="typeName">Имя типа.</param>
        dynamic NewObject(string typeName);
    }
}
