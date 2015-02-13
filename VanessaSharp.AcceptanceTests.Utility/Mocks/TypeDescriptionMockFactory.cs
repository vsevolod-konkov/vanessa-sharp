using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Moq;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.AcceptanceTests.Utility.Mocks
{
    /// <summary>
    /// Фабричные методы для создания мока <see cref="ITypeDescription"/>.
    /// </summary>
    public static class TypeDescriptionMockFactory
    {
        /// <summary>Карта соответствия типов CLR наименованием типов в 1С.</summary>
        private static readonly Dictionary<Type, OneSTypeInfo> _mapTypeNames =
                new Dictionary<Type, OneSTypeInfo>
                {
                    { typeof(string), NullableType("Строка") },
                    { typeof(double), NullableType("Число") },
                    { typeof(bool), NullableType("Булево") },
                    { typeof(DateTime), NullableType("Дата") },
                    { typeof(Guid), NullableType("Уникальный идентификатор") },
                    { typeof(AnyType), Type("?") },
                    { typeof(IQueryResult), Type("Результат запроса") }
                };

        /// <summary>Тип Null.</summary>
        private static readonly IOneSType _nullType = CreateOneSType("Null");

        private static OneSTypeInfo NullableType(string typeName)
        {
            return new OneSTypeInfo(typeName, true);
        }

        private static OneSTypeInfo Type(string typeName)
        {
            return new OneSTypeInfo(typeName, false);
        }

        /// <summary>
        /// Получение описание типа в 1С по типу CLR.
        /// </summary>
        private static OneSTypeInfo GetOneSTypeInfoByClrType(Type type)
        {
            try
            {
                return _mapTypeNames[type];
            }
            catch (KeyNotFoundException e)
            {
                throw new InvalidOperationException(string.Format("Для типа \"{0}\" не найдено соответствия с названием типа в 1С", type), e);
            }
        }

        /// <summary>
        /// Получение имени типа в 1С по типу CLR.
        /// </summary>
        public static string GetOneSTypeNameByClrType(Type type)
        {
            return GetOneSTypeInfoByClrType(type).Name;
        }

        /// <summary>
        /// Создание мока <see cref="ITypeDescription"/>
        /// соответствующего типу CLR.
        /// </summary>
        public static ITypeDescription Create(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            var typeInfo = GetOneSTypeInfoByClrType(type); 

            return Create(typeInfo.Name, typeInfo.IsNullable);
        }

        private static ITypeDescription Create(string oneSTypeName, bool isNullable)
        {
            var mock = MockHelper
                .CreateDisposableMock<ITypeDescription>();

            var oneSType = CreateOneSType(oneSTypeName);
            var typeArray = (isNullable)
                                ? CreateOneSArray(oneSType, _nullType)
                                : CreateOneSArray(oneSType);
            mock
                .SetupGet(d => d.Types)
                .Returns(typeArray);

            return mock.Object;
        }

        private static IOneSArray<IOneSType> CreateOneSArray(params IOneSType[] types)
        {
            var typesArrayMock = MockHelper.CreateDisposableMock<IOneSArray<IOneSType>>();
            typesArrayMock
                .Setup(a => a.Count())
                .Returns(types.Length);
            
            typesArrayMock
                .Setup(a => a.Get(It.IsAny<int>()))
                .Returns<int>(i => types[i]);

            return typesArrayMock.Object;
        }

        private static IOneSType CreateOneSType(string oneSTypeName)
        {
            var oneSTypeMock = MockHelper.CreateDisposableMock<IOneSType>();
            var oneSType = oneSTypeMock.Object;
            
            var globalContextMock = new Mock<IGlobalContext>(MockBehavior.Strict);
            globalContextMock
                .Setup(ctx => ctx.String(oneSType))
                .Returns(oneSTypeName);

            oneSTypeMock
                .Setup(t => t.GlobalContext)
                .Returns(globalContextMock.Object);

            return oneSType;
        }

        #region Вспомогательные типы

        private struct OneSTypeInfo
        {
            public OneSTypeInfo(string name, bool isNullable)
            {
                _name = name;
                _isNullable = isNullable;
            }

            public string Name
            {
                get { return _name; }
            }
            private readonly string _name;

            public bool IsNullable
            {
                get { return _isNullable; }
            }
            private readonly bool _isNullable;
        }

        #endregion
    }
}
