using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Стандартная реализация <see cref="IOneSProxyWrapper"/>.
    /// </summary>
    internal class OneSProxyWrapper : IOneSProxyWrapper
    {
        /// <summary>
        /// Определитель того является ли объект
        /// RCW-оберткой над объектом 1С.
        /// </summary>
        private readonly IOneSObjectDefiner _oneSObjectDefiner;
        
        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_oneSObjectDefiner != null);
        }

        /// <summary>Конструктор, для тестирования.</summary>
        /// <param name="oneSObjectDefiner">
        /// Определитель того является ли объект
        /// RCW-оберткой над объектом 1С.
        /// </param>
        internal OneSProxyWrapper(IOneSObjectDefiner oneSObjectDefiner)
        {
            Contract.Requires<ArgumentNullException>(oneSObjectDefiner != null);

            _oneSObjectDefiner = oneSObjectDefiner;
        }

        public OneSProxyWrapper() : this(OneSObjectDefiner.Default)
        {}

        /// <summary>Создание обертки над объектом.</summary>
        /// <param name="obj">Обертываемый объект.</param>
        /// <param name="type">Тип интерфейса, который должен поддерживаться оберткой.</param>
        public object Wrap(object obj, Type type)
        {
            if (obj == null)
                return null;

            return (_oneSObjectDefiner.IsOneSObject(obj))
                  ? WrapOneSObject(obj, type)
                  : obj;  
        }
        
        /// <summary>Обертывание 1С-объекта.</summary>
        /// <param name="obj">Обертываемый объект.</param>
        /// <param name="type">Тип к которому можно привести возвращаемую обертку.</param>
        protected virtual OneSObject WrapOneSObject(object obj, Type type)
        {
            Contract.Requires<ArgumentNullException>(obj != null);
            Contract.Requires<ArgumentNullException>(type != null);

            return new OneSObject(obj, this);
        }
    }
}
