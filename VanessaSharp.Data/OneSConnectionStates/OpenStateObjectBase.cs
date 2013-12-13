using System;
using System.Data;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    partial class OneSConnection
    {
        /// <summary>Базовый класс открытого состояния.</summary>
        private abstract class OpenStateObjectBase : StateObject
        {
            private bool _sharedGlobalContext;

            protected OpenStateObjectBase(IGlobalContext globalContext, string connectionString, int poolTimeout, int poolCapacity, string version)
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(connectionString));

                ChecksHelper.CheckArgumentNotNull(globalContext, "globalContext");
                ChecksHelper.CheckArgumentNotEmpty(version, "version");

                _globalContext = globalContext;
                _connectionString = connectionString;
                _poolTimeout = poolTimeout;
                _poolCapacity = poolCapacity;
                _version = version;
            }

            public override IGlobalContext GlobalContext
            {
                get { return _globalContext; }
            }
            private readonly IGlobalContext _globalContext;

            public sealed override StateObject OpenConnection()
            {
                throw new InvalidOperationException("Соединение уже открыто.");
            }

            public override StateObject CloseConnection()
            {
                return new ClosedStateObject
                {
                    ConnectionString = ConnectionString,
                    PoolTimeout = PoolTimeout,
                    PoolCapacity = PoolCapacity
                };
            }

            public sealed override ConnectionState ConnectionState
            {
                get { return ConnectionState.Open; }
            }

            public override string ConnectionString
            {
                get { return _connectionString; }
                set
                {
                    throw new InvalidOperationException("В состоянии Open соединения нельзя изменить свойство ConnectionString.");
                }
            }
            private readonly string _connectionString;

            protected sealed override void InternalDisposed()
            {
                if (!_sharedGlobalContext)
                    _globalContext.Dispose();
            }

            public sealed override int PoolTimeout
            {
                get { return _poolTimeout; }
                set
                {
                    if (PoolTimeout != value)
                    {
                        throw new InvalidOperationException(
                            "В состоянии Open соединения нельзя изменить свойство PoolTimeout.");
                    }
                }
            }
            private readonly int _poolTimeout;

            public sealed override int PoolCapacity
            {
                get { return _poolCapacity; }

                set
                {
                    if (PoolCapacity != value)
                    {
                        throw new InvalidOperationException(
                            "В состоянии Open соединения нельзя изменить свойство PoolCapacity.");
                    }
                }
            }
            private readonly int _poolCapacity;

            public sealed override bool IsExclusiveMode
            {
                get
                {
                    return _globalContext.ExclusiveMode();
                }

                set
                {
                    _globalContext.SetExclusiveMode(value);
                }
            }

            protected void UseGlobalContext()
            {
                _sharedGlobalContext = true;
            }

            public override string Version
            {
                get { return _version; }
            }
            private readonly string _version;
        }
    }
}