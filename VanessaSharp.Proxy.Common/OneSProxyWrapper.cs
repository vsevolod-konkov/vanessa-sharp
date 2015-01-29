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

        /// <summary>Создание обертки над объектом.</summary>
        /// <param name="obj">Обертываемый объект.</param>
        /// <param name="type">Тип интерфейса, который должен поддерживаться оберткой.</param>
        public object Wrap(object obj, Type type)
        {
            if (obj == null)
                return null;

            if (_oneSObjectDefiner.IsOneSObject(obj))
            {
                return type.IsEnum
                           ? ConvertToEnum(obj, type)
                           : WrapOneSObject(obj, type);
            }
                  
            return obj;  
        }
        
        /// <summary>Обертывание 1С-объекта.</summary>
        /// <param name="comObj">Обертываемый объект.</param>
        /// <param name="type">Тип к которому можно привести возвращаемую обертку.</param>
        protected virtual OneSObject WrapOneSObject(object comObj, Type type)
        {
            Contract.Requires<ArgumentNullException>(comObj != null);
            Contract.Requires<ArgumentNullException>(type != null);

            return new OneSObject(comObj, this);
        }

        /// <summary>
        /// Конвертация 1С-объекта в перечисление.
        /// </summary>
        /// <param name="comObj">Конвертируемый COM-объект.</param>
        /// <param name="enumType">Перечислимый тип.</param>
        protected virtual object ConvertToEnum(object comObj, Type enumType)
        {
            Contract.Requires<ArgumentNullException>(comObj != null);
            Contract.Requires<ArgumentNullException>(enumType != null);
            Contract.Requires<ArgumentException>(enumType.IsEnum);

            Contract.Ensures(Contract.Result<object>() != null);

            throw new NotSupportedException(string.Format(
                "Невозможно сконвертировать 1С-объект в тип \"{0}\". Конвертация объектов перечислений не поддерживается.",
                enumType));
        }
    }
}
