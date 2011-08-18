using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Контракт для интерфейса <see cref="IGlobalContext"/>.
    /// </summary>
    [ContractClassFor(typeof(IGlobalContext))]
    internal abstract class IGlobalContextContract : IGlobalContext
    {
        public dynamic NewObject(string typeName)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(typeName), "Имя типа не может быть пустым.");
            Contract.Ensures(Contract.Result<object>() != null);

            return null;
        }
    }
}
