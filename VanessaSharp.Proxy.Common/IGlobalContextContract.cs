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

        T IGlobalContext.NewObject<T>()
        {
            Contract.Ensures(Contract.Result<object>() != null);

            return default(T);
        }

        bool IGlobalContext.ExclusiveMode()
        {
            throw new NotImplementedException();
        }

        void IGlobalContext.SetExclusiveMode(bool value)
        {
            throw new NotImplementedException();
        }

        void IGlobalContext.BeginTransaction()
        {
            throw new NotImplementedException();
        }

        void IGlobalContext.CommitTransaction()
        {
            throw new NotImplementedException();
        }

        void IGlobalContext.RollbackTransaction()
        {
            throw new NotImplementedException();
        }

        string IGlobalContext.String(object obj)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        { }
    }
}
