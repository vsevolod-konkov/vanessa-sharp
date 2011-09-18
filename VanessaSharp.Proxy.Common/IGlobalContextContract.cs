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
        dynamic IGlobalContext.NewObject(string typeName)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(typeName), "Имя типа не может быть пустым.");
            Contract.Ensures(Contract.Result<object>() != null);

            return null;
        }

        bool IGlobalContext.ExclusiveMode()
        {
            throw new NotImplementedException();
        }

        void IGlobalContext.SetExclusiveMode(bool value)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        { }
    }
}
