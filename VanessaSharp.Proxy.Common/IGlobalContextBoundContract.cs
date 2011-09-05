using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Описание контракта для <see cref="IGlobalContextBound"/>.</summary>
    internal abstract class IGlobalContextBoundContract : IGlobalContextBound
    {
        IGlobalContext IGlobalContextBound.GlobalContext
        {
            get 
            {
                Contract.Ensures(Contract.Result<IGlobalContext>() != null);
                return null;
            }
        }

        void IDisposable.Dispose()
        {}
    }
}
