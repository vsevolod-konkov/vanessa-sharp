using System;
using System.Diagnostics.Contracts;
using System.Dynamic;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Базовый класс над объектами 1С.</summary>
    public class OneSObject : DynamicObject, IOneSProxy, IDisposable
    {
        /// <summary>Прокси-объект над оберткой 1С.</summary>
        private readonly OneSProxy _proxy;

        /// <summary>Конструктор.</summary>
        /// <param name="proxyCreator">Создатель Прокси-объекта над RCW-объектом 1С.</param>
        internal OneSObject(Func<OneSObject, OneSProxy> proxyCreator)
        {
            Contract.Requires<ArgumentNullException>(proxyCreator != null);

            _proxy = proxyCreator(this);
        }

        /// <summary>
        /// Конструктор, 
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

        /// <summary>Обертыватель объекта.</summary>
        internal IOneSProxyWrapper ProxyWrapper
        {
            get { return _proxy.ProxyWrapper; }
        }

        /// <summary>Динамическое обращение к члену нижележащего объекта.</summary>
        /// <param name="memberName">Имя члена.</param>
        public object this[string memberName]
        {
            get { return _proxy.GetMemberValue(memberName); }

            set { _proxy.SetMemberValue(memberName, value);}
        }

        /// <summary>Попытка конвертации в требуемый тип.</summary>
        /// <param name="binder">Привязчик.</param>
        /// <param name="result">Результат конвертации.</param>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            Contract.Assert(binder != null);

            return _proxy.TryConvert(binder, out result);
        }

        /// <summary>Попытка получения элемента по индексу.</summary>
        /// <param name="binder">Биндер индексатора.</param>
        /// <param name="indexes">Индексы.</param>
        /// <param name="result">Получаемый элемент.</param>
        public sealed override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            Contract.Assert(binder != null);
            Contract.Assert(indexes != null);

            return _proxy.TryGetIndex(binder, indexes, out result);
        }

        /// <summary>Попытка получения члена.</summary>
        /// <param name="binder">Биндер члена.</param>
        /// <param name="result">Значение члена.</param>
        public sealed override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Contract.Assert(binder != null);

            return _proxy.TryGetMember(binder, out result);
        }
        
        /// <summary>Попытка выполнения члена экземпляра.</summary>
        /// <param name="binder">Биндер выполнения члена.</param>
        /// <param name="args">Аргументы выполнения.</param>
        /// <param name="result">Результат вычисления.</param>
        public sealed override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Contract.Assert(binder != null);
            Contract.Assert(args != null);

            return _proxy.TryInvokeMember(binder, args, out result);
        }

        /// <summary>Попытка установки элемента по индексу.</summary>
        /// <param name="binder">Биндер установки по индексу.</param>
        /// <param name="indexes">Индексы.</param>
        /// <param name="value">Значение, которое необходимо установить.</param>
        public sealed override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            Contract.Assert(binder != null);
            Contract.Assert(indexes != null);

            return _proxy.TrySetIndex(binder, indexes, value);
        }

        /// <summary>Попытка установить значение члена.</summary>
        /// <param name="binder">Биндер установки значения члена.</param>
        /// <param name="value">Устанавливаемое значение.</param>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Contract.Assert(binder != null);

            return _proxy.TrySetMember(binder, value);
        }

        /// <summary>Освобождение ресурсов.</summary>
        public virtual void Dispose()
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

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if ((obj == null) || (GetType() != obj.GetType()))
                return false;

            return _proxy.Equals(((OneSObject)obj)._proxy);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return GetType().GetHashCode() * 251
                ^ _proxy.GetHashCode();    
            }
        }
    }
}
