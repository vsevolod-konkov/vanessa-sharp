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
        private static readonly Dictionary<Type, string> _mapTypeNames =
                new Dictionary<Type, string>
                {
                    { typeof(string), "Строка" },
                    { typeof(double), "Число" },
                    { typeof(bool), "Булево"},
                    { typeof(DateTime), "Дата" },
                    { typeof(Guid), "XXX"},
                    { typeof(AnyType), "?"},
                    { typeof(IQueryResult), "Результат запроса"}
                };

        /// <summary>
        /// Получение имени типа в 1С по типу CLR.
        /// </summary>
        private static string GetOneSTypeNameByClrType(Type type)
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
        /// Создание мока <see cref="ITypeDescription"/>
        /// соответствующего типу CLR.
        /// </summary>
        public static ITypeDescription Create(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);
            
            return Create(GetOneSTypeNameByClrType(type));
        }

        private static ITypeDescription Create(string oneSTypeName)
        {
            var mock = MockHelper
                .CreateDisposableMock<ITypeDescription>();

            mock
                .SetupGet(d => d.Types)
                .Returns(
                    CreateOneSArray(
                        CreateOneSType(oneSTypeName)));

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
    }
}
