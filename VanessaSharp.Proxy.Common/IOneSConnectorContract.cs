﻿using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Описание контракта <see cref="IOneSConnector"/>.</summary>
    [ContractClassFor(typeof(IOneSConnector))]
    internal abstract class IOneSConnectorContract : IOneSConnector
    {
        dynamic IOneSConnector.Connect(string connectString)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(connectString), "Строка соединения не может быть пустой.");
            Contract.Ensures(Contract.Result<object>() != null);

            return null;
        }

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

        void IDisposable.Dispose()
        {}
    }
}