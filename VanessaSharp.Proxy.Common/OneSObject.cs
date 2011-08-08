using System;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Базовый класс над объектами 1С.</summary>
    public sealed class OneSObject : DynamicObject, IDisposable
    {
        /// <summary>Обертка над нижележащим объектом.</summary>
        private readonly DisposableWrapper<object> _disposableWrapper;

        /// <summary>Конструктор принимающий RCW-обертку COM-объекта 1C.</summary>
        /// <param name="comObject">RCW-обертка COM-объекта 1C.</param>
        public OneSObject(object comObject)
        {
            _disposableWrapper = comObject.WrapToDisposable();
        }

        /// <summary>Обертка над объектом.</summary>
        /// <param name="obj">Объект.</param>
        public static object Wrap(object obj)
        {
            return (OneSProxyHelper.IsOneSObject(obj))
                ? new OneSObject(obj)
                : obj;
        }

        /// <summary>Снятие обертки с нижележащего объекта.</summary>
        private object Unwrap()
        {
            return _disposableWrapper.Object;
        }

        /// <summary>Снятие обертки с нижележащего объекта.</summary>
        /// <param name="obj">Объект с которого снимается обертка 1С.</param>
        private static object Unwrap(object obj)
        {
            var oneSObject = obj as OneSObject;
            return oneSObject == null
                ? obj
                : oneSObject.Unwrap();
        }

        /// <summary>Снятие обертки с массива объектов.</summary>
        /// <param name="objects">Массив с которого делается попытка снять обертку.</param>
        private static object[] Unwrap(object[] objects)
        {
            return objects
                .Select(o => Unwrap(o))
                .ToArray();
        }

        /// <summary>
        /// Слабосвязанное выполнение получения значения.
        /// </summary>
        /// <param name="binder">Привязчик.</param>
        private object Invoke(CallSiteBinder binder)
        {
            return Wrap(
                new FuncCaller(_disposableWrapper, binder)
                .Invoke());
        }

        /// <summary>
        /// Слабосвязанное выполнение получения значения.
        /// </summary>
        /// <param name="arg1">Аргумент функции получения значения.</param>
        /// <param name="binder">Привязчик.</param>
        private object Invoke<T>(CallSiteBinder binder, T arg1)
        {
            return Wrap(
                new FuncCaller<T>(_disposableWrapper, binder)
                .Invoke(arg1));
        }

        /// <summary>
        /// Слабосвязанное выполнение получения значения со снятием обертки с аргумента.
        /// </summary>
        /// <param name="arg">Аргумент функции получения значения.</param>
        /// <param name="binder">Привязчик.</param>
        private object Invoke(CallSiteBinder binder, object arg)
        {
            return Invoke<object>(binder, Unwrap(arg));
        }

        /// <summary>
        /// Слабосвязанное выполнение получения значения со снятием обертки с массива аргументов.
        /// </summary>
        /// <param name="args">Аргументы функции получения значения.</param>
        /// <param name="binder">Привязчик.</param>
        private object Invoke(CallSiteBinder binder, object[] args)
        {
            return Invoke<object[]>(binder, Unwrap(args));
        }

        /// <summary>Выполнение действия без аргументов.</summary>
        /// <param name="binder">Привязчик.</param>
        private void InvokeAction(CallSiteBinder binder)
        {
            new ActionCaller(_disposableWrapper, binder).Invoke();
        }

        /// <summary>Выполнение действия с одним аргументом.</summary>
        /// <typeparam name="T">Тип аргумента.</typeparam>
        /// <param name="binder">Привязчик.</param>
        /// <param name="arg1">Аргумент.</param>
        private void InvokeAction<T>(CallSiteBinder binder, T arg1)
        {
            new ActionCaller<T>(_disposableWrapper, binder).Invoke(arg1);
        }

        /// <summary>
        /// Выполнение действия с двумя аргументами.
        /// </summary>
        /// <typeparam name="T1">Тип первого аргумента.</typeparam>
        /// <typeparam name="T2">Тип второго аргумента.</typeparam>
        /// <param name="binder">Привязчик.</param>
        /// <param name="arg1">Первый аргумент.</param>
        /// <param name="arg2">Второй аргумент.</param>
        private void InvokeAction<T1, T2>(CallSiteBinder binder, T1 arg1, T2 arg2)
        {
            new ActionCaller<T1, T2>(_disposableWrapper, binder).Invoke(arg1, arg2);
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            result = Invoke(binder, arg);
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = Invoke(binder);
            return true;
        }

        public override bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result)
        {
            result = Invoke(binder, args);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = Invoke(binder, indexes);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = Invoke(binder);
            return true;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = Invoke(binder, args);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = Invoke(binder, args);
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            InvokeAction(binder, Unwrap(indexes), Unwrap(value));
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            InvokeAction(binder, Unwrap(value));
            return true;
        }

        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
        {
            result = Invoke(binder);
            return true;
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            InvokeAction(binder);
            return true;
        }

        public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes)
        {
            InvokeAction(binder, Unwrap(indexes));
            return true;
        }

        /// <summary>Освобождение ресурсов.</summary>
        public void Dispose()
        {
            _disposableWrapper.Dispose();
        }

        #region Вспомогательные типы
        
        /// <summary>Слабосвязанный вызыватель.</summary>
        /// <typeparam name="T">Тип делегата слабосвязанного вызова.</typeparam>
        private abstract class Caller<T> where T : class
        {
            /// <summary>Место вызова.</summary>
            protected readonly CallSite<T> _callSite;

            /// <summary>Обертка над объектом для которого делаются вызовы.</summary>
            private DisposableWrapper<object> _disposableWrapper;

            /// <summary>Конструктор.</summary>
            /// <param name="disposableWrapper">Обертка над объектом для которого делаются вызовы.</param>
            /// <param name="binder">Привязчик.</param>
            protected Caller(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
            {
                _disposableWrapper = disposableWrapper;
                _callSite = CallSite<T>.Create(binder);
            }

            /// <summary>Объект над которым делаются вызовы.</summary>
            protected object Object
            {
                get { return _disposableWrapper.Object; }
            }
        }

        /// <summary>
        /// Слабосвязанный вызыватель функции без аргументов.
        /// </summary>
        private sealed class FuncCaller : Caller<Func<CallSite, object, object>>
        {
            /// <summary>Конструктор.</summary>
            /// <param name="disposableWrapper">Обертка над объектом для которого делаются вызовы.</param>
            /// <param name="binder">Привязчик.</param>
            public FuncCaller(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            {}

            /// <summary>Выполнение функции.</summary>
            public object Invoke()
            {
                return _callSite.Target(_callSite, Object);
            }
        }

        /// <summary>
        /// Слабосвязанный вызыватель функции с одним аргументом.
        /// </summary>
        /// <typeparam name="T">Тип аргумента.</typeparam>
        private sealed class FuncCaller<T> : Caller<Func<CallSite, object, T, object>>
        {
            /// <summary>Конструктор.</summary>
            /// <param name="disposableWrapper">Обертка над объектом для которого делаются вызовы.</param>
            /// <param name="binder">Привязчик.</param>
            public FuncCaller(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            {}

            /// <summary>
            /// Выполнение функции.
            /// </summary>
            /// <param name="arg1">Аргумент для функции.</param>
            public object Invoke(T arg1)
            {
                return _callSite.Target(_callSite, Object, arg1);
            }
        }

        /// <summary>
        /// Слабосвязанный вызыватель действия без аргументов.
        /// </summary>
        private sealed class ActionCaller : Caller<Action<CallSite, object>>
        {
            /// <summary>Конструктор.</summary>
            /// <param name="disposableWrapper">Обертка над объектом для которого делаются вызовы.</param>
            /// <param name="binder">Привязчик.</param>
            public ActionCaller(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            { }

            /// <summary>
            /// Выполнение действия.
            /// </summary>
            public void Invoke()
            {
                _callSite.Target(_callSite, Object);
            }
        }

        /// <summary>
        /// Слабосвязанный вызыватель действия с одним аргументом.
        /// </summary>
        /// <typeparam name="T">Тип аргумента.</typeparam>
        private sealed class ActionCaller<T> : Caller<Action<CallSite, object, T>>
        {
            /// <summary>Конструктор.</summary>
            /// <param name="disposableWrapper">Обертка над объектом для которого делаются вызовы.</param>
            /// <param name="binder">Привязчик.</param>
            public ActionCaller(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            { }

            /// <summary>
            /// Выполнение действия.
            /// </summary>
            /// <param name="arg1">Аргумент.</param>
            public void Invoke(T arg1)
            {
                _callSite.Target(_callSite, Object, arg1);
            }
        }

        /// <summary>
        /// Слабосвязанный вызыватель действия с двумя аргументами.
        /// </summary>
        /// <typeparam name="T1">Тип первого аргумента.</typeparam>
        /// <typeparam name="T2">Тип второго аргумента.</typeparam>
        private sealed class ActionCaller<T1, T2> : Caller<Action<CallSite, object, T1, T2>>
        {
            /// <summary>Конструктор.</summary>
            /// <param name="disposableWrapper">Обертка над объектом для которого делаются вызовы.</param>
            /// <param name="binder">Привязчик.</param>
            public ActionCaller(DisposableWrapper<object> disposableWrapper, CallSiteBinder binder)
                : base(disposableWrapper, binder)
            { }

            /// <summary>
            /// Выполнение действия.
            /// </summary>
            /// <param name="arg1">Первый аргумент.</param>
            /// <param name="arg2">Второй аргумент.</param>
            public void Invoke(T1 arg1, T2 arg2)
            {
                _callSite.Target(_callSite, Object, arg1, arg2);
            }
        }

        #endregion
    }
}
