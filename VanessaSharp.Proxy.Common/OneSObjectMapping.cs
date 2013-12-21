using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Описание соответствия мужду объектом 1С 
    /// и типа CLR.
    /// </summary>
    public sealed class OneSObjectMapping
    {
        /// <summary>Конструктор.</summary>
        /// <param name="interfaceType">
        /// Запрашиваемый тип интерфейса в CLR.
        /// </param>
        /// <param name="wrapType">
        /// Тип обертки реализации в CLR 
        /// запрашиваемого интерфейса.
        /// </param>
        /// <param name="oneSTypeName">
        /// Имя соответсвующего типа в 1С.
        /// </param>
        public OneSObjectMapping(Type interfaceType, Type wrapType, string oneSTypeName)
        {
            Contract.Requires<ArgumentNullException>(interfaceType != null);
            Contract.Requires<ArgumentNullException>(wrapType != null);

            _interfaceType = interfaceType;
            _wrapType = wrapType;
            _oneSTypeName = oneSTypeName;
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_interfaceType != null);
            Contract.Invariant(_wrapType != null);
        }

        /// <summary>
        /// Запрашиваемый тип интерфейса в CLR.
        /// </summary>
        public Type InterfaceType
        {
            get { return _interfaceType; }
        }
        private readonly Type _interfaceType;

        /// <summary>
        /// Тип обертки реализации в CLR 
        /// запрашиваемого интерфейса.
        /// </summary>
        public Type WrapType
        {
            get { return _wrapType; }
        }
        private readonly Type _wrapType;

        /// <summary>
        /// Имя соответсвующего типа в 1С.
        /// </summary>
        public string OneSTypeName
        {
            get { return _oneSTypeName; }
        }
        private readonly string _oneSTypeName;
    }
}
