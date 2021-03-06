﻿using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.InteropServices;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Базовая реализация <see cref="IOneSConnector"/>.
    /// </summary>
    public abstract class OneSConnectorBase : IOneSConnector
    {
        /// <summary>COM-соединитель с 1С.</summary>
        private readonly DisposableWrapper<object> _comConnector;

        /// <summary>
        /// Конструктор принимающий RCW-обертку соединителя.
        /// </summary>
        /// <param name="comConnector">
        /// RCW-обертка соединителя к информационной БД 1С.
        /// </param>
        protected OneSConnectorBase(object comConnector)
        {
            Contract.Requires<ArgumentNullException>(comConnector != null);
            
            _comConnector = comConnector.WrapToDisposable();
        }

        /// <summary>
        /// Получение RCW-обертки соединителя к информационной БД 1С.
        /// Перехватывает исключения COM и создает необходимую диагностику.
        /// </summary>
        /// <param name="creator">
        /// Делегат непосредственного создания обертки.
        /// </param>
        protected static TComConnector GetNewComConnector<TComConnector>(Func<TComConnector> creator)
        {
            Contract.Requires<ArgumentNullException>(creator != null);

            try
            {
                return creator();
            }
            catch (COMException e)
            {
                throw new InvalidOperationException(string.Format(
                    "Не удалось создать экземпляр COM-объекта 1С. Убедитесь что 1С установлена на машине. Исходная ошибка: \"{0}\".",
                    e.Message), e);
            }
        }

        /// <summary>
        /// RCW-обертка соединителя к информационной БД 1С.
        /// </summary>
        protected object ComConnector
        {
            get { return _comConnector.Object; }
        }

        /// <summary>
        /// Выполнение метода соединения объекта <see cref="ComConnector"/>.
        /// </summary>
        /// <param name="connectString">Строка соединения.</param>
        protected abstract object ComConnect(string connectString);

        /// <summary>
        /// Выполнение соединения с информационной базой 1С.
        /// </summary>
        /// <param name="connectString">Строка соединения.</param>
        /// <returns>Возвращает объект глобального контекста.</returns>
        private object InvokeConnect(string connectString)
        {
            try
            {
                return ComConnect(connectString);
            }
            catch (COMException e)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Ошибка подключения к информационной базе 1C. Строка соединения: \"{0}\". Код ошибки: \"{1}\". Сообщение: \"{2}\".",
                        connectString, e.ErrorCode, e.Message), e);
            }
        }

        /// <summary>Соединение с информационной базой.</summary>
        /// <param name="connectString">Строка соединения.</param>
        /// <returns>Возвращает объект глобального контекста.</returns>
        public IGlobalContext Connect(string connectString)
        {
            var result = InvokeConnect(connectString);
            if (result == null)
                throw new InvalidOperationException("Соединитель к 1С вернул null при соединении.");

            return new OneSGlobalContext(result);
        }

        /// <summary>
        /// Свойства позднего связывания COM-коннектора.
        /// </summary>
        /// <param name="propertyName">Имя свойства.</param>
        public virtual object this[string propertyName]
        {
            get
            {
                var type = ComConnector.GetType();

                return type.InvokeMember(propertyName,
                                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty, null,
                                         ComConnector, null);
            }

            set
            {
                var type = ComConnector.GetType();

                type.InvokeMember(propertyName,
                                  BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty | BindingFlags.IgnoreReturn,
                                  null, ComConnector, new[] { value });
            }
        }

        /// <summary>Инициализация свойств коннектора.</summary>
        /// <param name="initializer">
        /// Объект с одноименными свойствами коннектора.
        /// </param>
        public virtual void Init(object initializer)
        {
            if (initializer == null)
                return;

            var type = initializer.GetType();

            foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                SetProperty(
                    propertyInfo,
                    propertyInfo.GetValue(initializer, null));
            }

            foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                SetProperty(
                    fieldInfo,
                    fieldInfo.GetValue(initializer));
            }
        }

        private void SetProperty(MemberInfo memberInfo, object value)
        {
            if (value != null)
                this[memberInfo.Name] = value;
        }

        /// <summary>Время ожидания подключения.</summary>
        public abstract uint PoolTimeout { get; set; }

        /// <summary>Мощность подключения.</summary>
        public abstract uint PoolCapacity { get; set; }

        /// <summary>Версия.</summary>
        public abstract string Version { get; }

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _comConnector.Dispose();
        }
    }
}
