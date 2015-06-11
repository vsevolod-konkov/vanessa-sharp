using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Описание контракта <see cref="IOneSConnector"/>.</summary>
    [ContractClassFor(typeof(IOneSConnector))]
    internal abstract class IOneSConnectorContract : IOneSConnector
    {
        IGlobalContext IOneSConnector.Connect(string connectString)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(connectString), "Строка соединения не может быть пустой.");
            Contract.Ensures(Contract.Result<IGlobalContext>() != null);

            return null;
        }

        void IOneSConnector.Init(object initializer)
        {}

        uint IOneSConnector.PoolTimeout
        {
            get { return 0; }
            set {}
        }

        uint IOneSConnector.PoolCapacity
        {
            get { return 0; }
            set {}
        }

        string IOneSConnector.Version
        {
            get 
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
                
                throw new NotImplementedException(); 
            }
        }

        void IDisposable.Dispose()
        {}
    }
}
