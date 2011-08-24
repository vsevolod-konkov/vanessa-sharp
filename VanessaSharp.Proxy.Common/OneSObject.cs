using System;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Базовый класс над объектами 1С.</summary>
    public class OneSObject : DynamicObject, IOneSProxy, IDisposable
    {
        /// <summary>Прокси-объект над оберткой 1С.</summary>
        private readonly OneSProxy _proxy;

        /// <summary>
        /// Базовый конструктор.
        /// </summary>
        /// <param name="comObject">RCW-обертка COM-объекта 1C.</param>
        /// <param name="proxyWrapper">Обертыватель объекта.</param>
        internal OneSObject(object comObject, IOneSProxyWrapper proxyWrapper)
        {
            Contract.Requires<ArgumentNullException>(
                comObject != null, "comObject не может быть равен null");
            Contract.Requires<ArgumentNullException>(
                proxyWrapper != null, "proxyWrapper не может быть равен null");

            _proxy = new OneSProxy(comObject, proxyWrapper);
        }

        /// <summary>Попытка получения элемента по индексу.</summary>
        /// <param name="binder">Биндер индексатора.</param>
        /// <param name="indexes">Индексы.</param>
        /// <param name="result">Получаемый элемент.</param>
        public sealed override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(indexes != null);

            return _proxy.TryGetIndex(binder, indexes, out result);
        }

        /// <summary>Попытка получения члена.</summary>
        /// <param name="binder">Биндер члена.</param>
        /// <param name="result">Значение члена.</param>
        public sealed override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Contract.Requires<ArgumentNullException>(binder != null);

            return _proxy.TryGetMember(binder, out result);
        }
        
        /// <summary>Попытка выполнения члена экземпляра.</summary>
        /// <param name="binder">Биндер выполнения члена.</param>
        /// <param name="args">Аргументы выполнения.</param>
        /// <param name="result">Результат вычисления.</param>
        public sealed override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(args != null);

            return _proxy.TryInvokeMember(binder, args, out result);
        }

        /// <summary>Попытка установки элемента по индексу.</summary>
        /// <param name="binder">Биндер установки по индексу.</param>
        /// <param name="indexes">Индексы.</param>
        /// <param name="value">Значение, которое необходимо установить.</param>
        public sealed override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(indexes != null);

            return _proxy.TrySetIndex(binder, indexes, value);
        }

        /// <summary>Попытка установить значение члена.</summary>
        /// <param name="binder">Биндер установки значения члена.</param>
        /// <param name="value">Устанавливаемое значение.</param>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);

            return _proxy.TrySetMember(binder, value);
        }

        /// <summary>Освобождение ресурсов.</summary>
        public void Dispose()
        {
            _proxy.Dispose();
        }

        /// <summary>
        /// Прокси с динамическим поведением.
        /// </summary>
        protected dynamic DynamicProxy
        {
            get { return _proxy; }
        }

        /// <summary>
        /// Получение исходного RCW-обертки 1С-объекта.
        /// </summary>
        object IOneSProxy.Unwrap()
        {
            return _proxy.Unwrap();
        }
    }
}
