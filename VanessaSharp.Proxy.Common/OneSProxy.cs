﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Прокси-объект над RCW-объектом 1С.
    /// </summary>
    internal sealed class OneSProxy : DynamicObject, IOneSProxy, IDisposable
    {
        /// <summary>Ссылка на пустой массив.</summary>
        /// <remarks>В целях оптимизации.</remarks>
        private static readonly object[] _emptyObjectsArray = new object[0];
        
        /// <summary>Обертка над нижележащим объектом.</summary>
        private readonly DisposableWrapper<object> _disposableWrapper;

        /// <summary>Обертыватель объекта.</summary>
        private readonly IOneSProxyWrapper _proxyWrapper;

        /// <summary>
        /// Базовый конструктор.
        /// </summary>
        /// <param name="comObject">RCW-обертка COM-объекта 1C.</param>
        /// <param name="proxyWrapper">Обертыватель объекта.</param>
        public OneSProxy(object comObject, IOneSProxyWrapper proxyWrapper)
        {
            Contract.Requires<ArgumentNullException>(
                comObject != null, "comObject не может быть равен null");
            Contract.Requires<ArgumentNullException>(
                proxyWrapper != null, "proxyWrapper не может быть равен null");

            _disposableWrapper = comObject.WrapToDisposable();
            _proxyWrapper = proxyWrapper;
        }

        /// <summary>Обертыватель объекта.</summary>
        internal IOneSProxyWrapper ProxyWrapper
        {
            get { return _proxyWrapper; }
        }

        /// <summary>Обертка над объектом.</summary>
        /// <param name="obj">Объект.</param>
        /// <param name="type">Тип интерфейса, который должен поддерживаться оберткой.</param>
        private object Wrap(object obj, Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            return _proxyWrapper.Wrap(obj, type);
        }

        /// <summary>Снятие обертки с нижележащего объекта.</summary>
        public object Unwrap()
        {
            return _disposableWrapper.Object;
        }

        /// <summary>Снятие обертки с нижележащего объекта.</summary>
        /// <param name="obj">Объект с которого снимается обертка 1С.</param>
        private static object Unwrap(object obj)
        {
            var oneSProxy = obj as IOneSProxy;
            return oneSProxy == null
                ? obj
                : oneSProxy.Unwrap();
        }

        /// <summary>
        /// Конвертация значения аргумента для 1С.
        /// </summary>
        /// <param name="value">Конвертируемое значение.</param>
        private object ConvertArgToOneS(object value)
        {
            var convertedValue = _proxyWrapper.ConvertToOneS(value);

            return Unwrap(convertedValue);
        }

        /// <summary>Конвертация массива объектов для использование в 1С.</summary>
        /// <param name="objects">Массив с которого делается попытка сконвертировать к нужному типу.</param>
        private object[] ConvertArgsToOneS(object[] objects)
        {
            return objects
                .Select(ConvertArgToOneS)
                .ToArray();
        }

        /// <summary>Выполнение функции.</summary>
        /// <param name="binder">Биндер.</param>
        /// <param name="argumentsCount">Количество аргументов функции.</param>
        /// <param name="args">Аргументы.</param>
        /// <param name="returnType">Тип возвращаемого объекта.</param>
        private object InvokeFunc(CallSiteBinder binder, int argumentsCount, object[] args, Type returnType)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(args != null);
            Contract.Requires<ArgumentNullException>(returnType != null);

            return Wrap(
                     CreateFuncCaller(_disposableWrapper, binder, argumentsCount)
                        .Invoke(ConvertArgsToOneS(args)),
                     returnType);
        }

        /// <summary>Динамическое получение значения члена нижележащего объекта.</summary>
        /// <param name="memberName">Имя члена.</param>
        public object GetMemberValue(string memberName)
        {
            object result;
            try
            {
                TryGetMember(GetGetMemberBinder(memberName), out result);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(string.Format(
                    "Произошла ошибка при получении значения члена \"{0}\". Ошибка: {1}",
                    memberName, e.Message));
            }

            return result;
        }

        /// <summary>Динамическое установка значения члена нижележащего объекта.</summary>
        /// <param name="memberName">Имя члена.</param>
        /// <param name="value">Устанавливаемое значение.</param>
        public void SetMemberValue(string memberName, object value)
        {
            try
            {
                TrySetMember(GetSetMemberBinder(memberName), value);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(string.Format(
                    "Произошла ошибка при получении значения члена \"{0}\". Ошибка: {1}",
                    memberName, e.Message));
            }
        }

        private static readonly IEnumerable<CSharpArgumentInfo> _argumentInfos = new CSharpArgumentInfo[0];
        private static readonly Type _oneSProxyType = typeof(OneSProxy);

        private GetMemberBinder GetGetMemberBinder(string memberName)
        {
            GetMemberBinder binder;
            if (!_getMemberBinders.TryGetValue(memberName, out binder))
            {
                binder = (GetMemberBinder)Binder.GetMember(CSharpBinderFlags.None, memberName, _oneSProxyType, _argumentInfos);
                _getMemberBinders.Add(memberName, binder);
            }

            return binder;
        }
        private readonly Dictionary<string, GetMemberBinder> _getMemberBinders = new Dictionary<string, GetMemberBinder>(); 
        

        private SetMemberBinder GetSetMemberBinder(string memberName)
        {
            SetMemberBinder binder;
            if (!_setMemberBinders.TryGetValue(memberName, out binder))
            {
                binder = (SetMemberBinder)Binder.SetMember(CSharpBinderFlags.None, memberName, _oneSProxyType, _argumentInfos);
                _setMemberBinders.Add(memberName, binder);
            }

            return binder;
        }
        private readonly Dictionary<string, SetMemberBinder> _setMemberBinders = new Dictionary<string, SetMemberBinder>(); 

        /// <summary>Попытка конвертации в требуемый тип.</summary>
        /// <param name="binder">Привязчик.</param>
        /// <param name="result">Результат конвертации.</param>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            Contract.Assert(binder != null);

            result = Wrap(Unwrap(), binder.ReturnType);
            return true;
        }

        /// <summary>Попытка получения элемента по индексу.</summary>
        /// <param name="binder">Биндер индексатора.</param>
        /// <param name="indexes">Индексы.</param>
        /// <param name="result">Получаемый элемент.</param>
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            Contract.Assert(binder != null);
            Contract.Assert(indexes != null);
            
            result = InvokeFunc(binder, binder.CallInfo.ArgumentCount, indexes, binder.ReturnType);
            return true;
        }

        /// <summary>Попытка получения члена.</summary>
        /// <param name="binder">Биндер члена.</param>
        /// <param name="result">Значение члена.</param>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Contract.Assert(binder != null);

            result = InvokeFunc(binder, 0, _emptyObjectsArray, binder.ReturnType);
            return true;
        }
        
        /// <summary>Попытка выполнения члена экземпляра.</summary>
        /// <param name="binder">Биндер выполнения члена.</param>
        /// <param name="args">Аргументы выполнения.</param>
        /// <param name="result">Результат вычисления.</param>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Contract.Assert(binder != null);
            Contract.Assert(args != null);

            result = InvokeFunc(binder, binder.CallInfo.ArgumentCount, args, binder.ReturnType);
            return true;
        }

        /// <summary>Выполнение действия.</summary>
        /// <param name="binder">Биндер.</param>
        /// <param name="argumentsCount">Количество аргументов.</param>
        /// <param name="args">Аргументы.</param>
        private void InvokeAction(CallSiteBinder binder, int argumentsCount, object[] args)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(args != null);

            CreateActionCaller(_disposableWrapper, binder, argumentsCount)
                .Invoke(ConvertArgsToOneS(args));
        }

        /// <summary>Попытка установки элемента по индексу.</summary>
        /// <param name="binder">Биндер установки по индексу.</param>
        /// <param name="indexes">Индексы.</param>
        /// <param name="value">Значение, которое необходимо установить.</param>
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            Contract.Assert(binder != null);
            Contract.Assert(indexes != null);
            
            if (indexes.Length != binder.CallInfo.ArgumentCount)
            {
                throw new ArgumentException(string.Format(
                    "Ожидалось, что количество индексов будет равно \"{0}\", а было равно \"{1}\".",
                    binder.CallInfo.ArgumentCount, indexes.Length));
            }

            InvokeAction(
                binder, 
                binder.CallInfo.ArgumentCount + 1, 
                AppendItem(indexes, value));

            return true;
        }

        /// <summary>Добавление еще одного элемента в конце массива.</summary>
        /// <param name="array">Исходный массив.</param>
        /// <param name="newItem">Добавляемый элемент.</param>
        /// <returns>Новый массив с добавленным элементом.</returns>
        private static object[] AppendItem(object[] array, object newItem)
        {
            var result = array;
            var oldLength = array.Length;
            Array.Resize(ref result, oldLength + 1);
            result[oldLength] = newItem;

            return result;
        }

        /// <summary>Попытка установить значение члена.</summary>
        /// <param name="binder">Биндер установки значения члена.</param>
        /// <param name="value">Устанавливаемое значение.</param>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Contract.Assert(binder != null);

            InvokeAction(binder, 1, new[] { value });
            return true;
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

            if (obj == null)
                return false;

            var otherProxy = obj as OneSProxy;
            return (otherProxy == null)
                       ? ReferenceEquals(Unwrap(), obj)
                       : ReferenceEquals(Unwrap(), otherProxy.Unwrap());
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
            var realObj = Unwrap();
            return (realObj == null)
                       ? 0
                       : realObj.GetHashCode();
        }

        /// <summary>Освобождение ресурсов.</summary>
        public void Dispose()
        {
            _disposableWrapper.Dispose();
        }

        /// <summary>Создание объекта вызова функции.</summary>
        /// <param name="disposableWrapper">Обертка над вызываемым объектом.</param>
        /// <param name="binder">Биндер.</param>
        /// <param name="argumentsCount">Количество аргументов.</param>
        private static IFuncCaller CreateFuncCaller(
            DisposableWrapper<object> disposableWrapper, CallSiteBinder binder, int argumentsCount)
        {
            Contract.Requires<ArgumentNullException>(disposableWrapper != null);
            Contract.Requires<ArgumentNullException>(binder != null);

            switch (argumentsCount)
            {
                case 0:
                    return new FuncCaller0(disposableWrapper, binder);
                case 1:
                    return new FuncCaller1(disposableWrapper, binder);
                case 2:
                    return new FuncCaller2(disposableWrapper, binder);
                case 3:
                    return new FuncCaller3(disposableWrapper, binder);
                case 4:
                    return new FuncCaller4(disposableWrapper, binder);
                case 5:
                    return new FuncCaller5(disposableWrapper, binder);
                case 6:
                    return new FuncCaller6(disposableWrapper, binder);
                default:
                    throw new NotSupportedException(string.Format(
                        "Не поддерживается вызов, если количество аргументов равно {0}", argumentsCount));
            }
        }

        /// <summary>Создание объекта вызова действия.</summary>
        /// <param name="disposableWrapper">Обертка над вызываемым объектом.</param>
        /// <param name="binder">Биндер.</param>
        /// <param name="argumentsCount">Количество аргументов.</param>
        private static IActionCaller CreateActionCaller(
            DisposableWrapper<object> disposableWrapper, CallSiteBinder binder, int argumentsCount)
        {
            Contract.Requires<ArgumentNullException>(disposableWrapper != null);
            Contract.Requires<ArgumentNullException>(binder != null);

            switch (argumentsCount)
            {
                case 1:
                    return new ActionCaller1(disposableWrapper, binder);
                case 2:
                    return new ActionCaller2(disposableWrapper, binder);
                case 3:
                    return new ActionCaller3(disposableWrapper, binder);
                case 4:
                    return new ActionCaller4(disposableWrapper, binder);
                default:
                    throw new NotSupportedException(string.Format(
                        "Не поддерживается вызов, если количество аргументов равно {0}", argumentsCount));
            }
        }

        #region Вспомогательные типы
        
        /// <summary>Слабосвязанный вызыватель.</summary>
        /// <typeparam name="T">Тип делегата слабосвязанного вызова.</typeparam>
        private abstract class Caller<T> where T : class
        {
            /// <summary>Место вызова.</summary>
            protected readonly CallSite<T> _callSite;

            /// <summary>Обертка над объектом для которого делаются вызовы.</summary>
            private readonly DisposableWrapper<object> _disposableWrapper;

            /// <summary>Конструктор.</summary>
            /// <param name="disposableWrapper">Обертка над объектом для которого делаются вызовы.</param>
            /// <param name="binder">Привязчик.</param>
            protected Caller(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
            {
                Contract.Requires<ArgumentNullException>(disposableWrapper != null);
                Contract.Requires<ArgumentNullException>(binder != null);
                
                _disposableWrapper = disposableWrapper;
                _callSite = CallSite<T>.Create(binder);
            }

            /// <summary>Объект над которым делаются вызовы.</summary>
            protected object Object
            {
                get { return _disposableWrapper.Object; }
            }

            /// <summary>Количество аргументов.</summary>
            protected abstract int ArgumentsCount { get; }

            /// <summary>Проверка аргументов.</summary>
            /// <param name="args">Массив аргументов.</param>
            protected void CheckArguments(object[] args)
            {
                Contract.Requires<ArgumentNullException>(args != null);

                if (args.Length != ArgumentsCount)
                {
                    throw new ArgumentException(
                        string.Format(
                        "Ожидалось, что количество аргументов будет равно \"{0}\", а оказалось \"{1}\".",
                        ArgumentsCount, args.Length));
                }
            }
        }

        #region Вызов функций

        /// <summary>Интерфейс вызова функции.</summary>
        private interface IFuncCaller
        {
            /// <summary>Вызов функции.</summary>
            /// <param name="args">Аргументы функции.</param>
            object Invoke(object[] args);
        }

        /// <summary>Базовый класс для вызова функций.</summary>
        /// <typeparam name="T">Тип делегата функции.</typeparam>
        private abstract class FuncCaller<T> : Caller<T>, IFuncCaller
            where T : class
        {
            /// <summary>Конструктор.</summary>
            protected FuncCaller(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            {}

            /// <summary>Перегружаемый метод выполнения действия.</summary>
            /// <param name="args">Аргументы.</param>
            protected abstract object InternalInvoke(object[] args);
            
            object IFuncCaller.Invoke(object[] args)
            {
                CheckArguments(args);

                return InternalInvoke(args);
            }
        }

        /// <summary>
        /// Вызыватель функции без аргументов.
        /// </summary>
        private sealed class FuncCaller0 : FuncCaller<Func<CallSite, object, object>>
        {
            /// <summary>Конструктор.</summary>
            public FuncCaller0(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            {}

            /// <summary>Выполнение функции.</summary>
            public object Invoke()
            {
                return _callSite.Target(_callSite, Object);
            }

            protected override int ArgumentsCount
            {
                get { return 0; }
            }

            protected override object InternalInvoke(object[] args)
            {
                return Invoke();
            }
        }

        /// <summary>
        /// Вызыватель функции с одним аргументом.
        /// </summary>
        private sealed class FuncCaller1 : FuncCaller<Func<CallSite, object, object, object>>
        {
            /// <summary>Конструктор.</summary>
            public FuncCaller1(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            { }

            /// <summary>Выполнение функции.</summary>
            public object Invoke(object arg)
            {
                return _callSite.Target(_callSite, Object, arg);
            }

            protected override int ArgumentsCount
            {
                get { return 1; }
            }

            protected override object InternalInvoke(object[] args)
            {
                return Invoke(args[0]);
            }
        }

        /// <summary>
        /// Вызыватель функции с двумя аргументами.
        /// </summary>
        private sealed class FuncCaller2 : FuncCaller<Func<CallSite, object, object, object, object>>
        {
            /// <summary>Конструктор.</summary>
            public FuncCaller2(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            { }

            /// <summary>Выполнение функции.</summary>
            public object Invoke(object arg1, object arg2)
            {
                return _callSite.Target(_callSite, Object, arg1, arg2);
            }

            protected override int ArgumentsCount
            {
                get { return 2; }
            }

            protected override object InternalInvoke(object[] args)
            {
                return Invoke(args[0], args[1]);
            }
        }

        /// <summary>
        /// Вызыватель функции с тремя аргументами.
        /// </summary>
        private sealed class FuncCaller3 
            : FuncCaller<Func<CallSite, object, object, object, object, object>>
        {
            /// <summary>Конструктор.</summary>
            public FuncCaller3(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            { }

            /// <summary>Выполнение функции.</summary>
            public object Invoke(object arg1, object arg2, object arg3)
            {
                return _callSite.Target(_callSite, Object, arg1, arg2, arg3);
            }

            protected override int ArgumentsCount
            {
                get { return 3; }
            }

            protected override object InternalInvoke(object[] args)
            {
                return Invoke(args[0], args[1], args[2]);
            }
        }

        /// <summary>
        /// Вызыватель функции с четырьмя аргументами.
        /// </summary>
        private sealed class FuncCaller4
            : FuncCaller<Func<CallSite, object, object, object, object, object, object>>
        {
            /// <summary>Конструктор.</summary>
            public FuncCaller4(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            { }

            /// <summary>Выполнение функции.</summary>
            public object Invoke(object arg1, object arg2, object arg3, object arg4)
            {
                return _callSite.Target(_callSite, Object, arg1, arg2, arg3, arg4);
            }

            protected override int ArgumentsCount
            {
                get { return 4; }
            }

            protected override object InternalInvoke(object[] args)
            {
                return Invoke(args[0], args[1], args[2], args[3]);
            }
        }

        /// <summary>
        /// Вызыватель функции с пятью аргументами.
        /// </summary>
        private sealed class FuncCaller5
            : FuncCaller<Func<CallSite, object, object, object, object, object, object, object>>
        {
            /// <summary>Конструктор.</summary>
            public FuncCaller5(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            { }

            /// <summary>Выполнение функции.</summary>
            public object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5)
            {
                return _callSite.Target(_callSite, Object, arg1, arg2, arg3, arg4, arg5);
            }

            protected override int ArgumentsCount
            {
                get { return 5; }
            }

            protected override object InternalInvoke(object[] args)
            {
                return Invoke(args[0], args[1], args[2], args[3], args[4]);
            }
        }

        /// <summary>
        /// Вызыватель функции с шестью аргументами.
        /// </summary>
        private sealed class FuncCaller6
            : FuncCaller<Func<CallSite, object, object, object, object, object, object, object, object>>
        {
            /// <summary>Конструктор.</summary>
            public FuncCaller6(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            { }

            /// <summary>Выполнение функции.</summary>
            public object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
            {
                return _callSite.Target(_callSite, Object, arg1, arg2, arg3, arg4, arg5, arg6);
            }

            protected override int ArgumentsCount
            {
                get { return 6; }
            }

            protected override object InternalInvoke(object[] args)
            {
                return Invoke(args[0], args[1], args[2], args[3], args[4], args[5]);
            }
        }

        #endregion

        #region Вызов действий
        
        /// <summary>Интерфейс вызова действий.</summary>
        private interface IActionCaller
        {
            /// <summary>Вызов действия.</summary>
            /// <param name="args">Аргументы.</param>
            void Invoke(object[] args);
        }

        /// <summary>Базовый класс вызывателя действий.</summary>
        /// <typeparam name="T">Тип делегата действия.</typeparam>
        private abstract class ActionCaller<T> : Caller<T>, IActionCaller
            where T : class
        {
            /// <summary>Конструктор.</summary>
            protected ActionCaller(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            {}

            /// <summary>
            /// Перегружаемый метод выполнения действия.
            /// </summary>
            /// <param name="args">Аргументы.</param>
            protected abstract void InternalInvoke(object[] args);

            void IActionCaller.Invoke(object[] args)
            {
                CheckArguments(args);
                InternalInvoke(args);
            }
        }


        /// <summary>
        /// Вызыватель действия с одним аргументом.
        /// </summary>
        private sealed class ActionCaller1 : ActionCaller<Action<CallSite, object, object>>
        {
            /// <summary>Конструктор.</summary>
            public ActionCaller1(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            { }

            /// <summary>
            /// Выполнение действия.
            /// </summary>
            public void Invoke(object arg)
            {
                _callSite.Target(_callSite, Object, arg);
            }

            protected override int ArgumentsCount
            {
                get { return 1; }
            }

            protected override void InternalInvoke(object[] args)
            {
                Invoke(args[0]);
            }
        }

        /// <summary>
        /// Вызыватель действия с двумя аргументами.
        /// </summary>
        private sealed class ActionCaller2 : ActionCaller<Action<CallSite, object, object, object>>
        {
            /// <summary>Конструктор.</summary>
            public ActionCaller2(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            { }

            /// <summary>
            /// Выполнение действия.
            /// </summary>
            public void Invoke(object arg1, object arg2)
            {
                _callSite.Target(_callSite, Object, arg1, arg2);
            }

            protected override int ArgumentsCount
            {
                get { return 2; }
            }

            protected override void InternalInvoke(object[] args)
            {
                Invoke(args[0], args[1]);
            }
        }

        /// <summary>
        /// Вызыватель действия с тремя аргументами.
        /// </summary>
        private sealed class ActionCaller3 : ActionCaller<Action<CallSite, object, object, object, object>>
        {
            /// <summary>Конструктор.</summary>
            public ActionCaller3(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            { }

            /// <summary>
            /// Выполнение действия.
            /// </summary>
            public void Invoke(object arg1, object arg2, object arg3)
            {
                _callSite.Target(_callSite, Object, arg1, arg2, arg3);
            }

            protected override int ArgumentsCount
            {
                get { return 3; }
            }

            protected override void InternalInvoke(object[] args)
            {
                Invoke(args[0], args[1], args[2]);
            }
        }

        /// <summary>
        /// Вызыватель действия с четырьмя аргументами.
        /// </summary>
        private sealed class ActionCaller4 : ActionCaller<Action<CallSite, object, object, object, object, object>>
        {
            /// <summary>Конструктор.</summary>
            public ActionCaller4(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            { }

            /// <summary>
            /// Выполнение действия.
            /// </summary>
            public void Invoke(object arg1, object arg2, object arg3, object arg4)
            {
                _callSite.Target(_callSite, Object, arg1, arg2, arg3, arg4);
            }

            protected override int ArgumentsCount
            {
                get { return 4; }
            }

            protected override void InternalInvoke(object[] args)
            {
                Invoke(args[0], args[1], args[2], args[3]);
            }
        }

        #endregion

        #endregion
    }
}
