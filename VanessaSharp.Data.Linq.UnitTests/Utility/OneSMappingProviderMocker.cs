using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Moq;
using VanessaSharp.Data.Linq.Internal;

namespace VanessaSharp.Data.Linq.UnitTests.Utility
{
    /// <summary>
    /// Класс для инициализации мока <see cref="IOneSMappingProvider"/>
    /// fluent-стилем.
    /// </summary>
    internal static class OneSMappingProviderMocker
    {
        /// <summary>
        /// Старт описания карты соответстия типизированной записи
        /// в источнике данных 1С для мока.
        /// </summary>
        /// <typeparam name="T">Тип записи.</typeparam>
        /// <param name="mock">Объект мока.</param>
        /// <param name="sourceName">Имя источника данных 1С, соответствующего типу записи.</param>
        public static Builder<T> BeginSetupGetTypeMappingFor<T>(this Mock<IOneSMappingProvider> mock, string sourceName)
        {
            Contract.Requires<ArgumentNullException>(mock != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));
            
            return new Builder<T>(mock, OneSTypeMapper.BeginFor<T>(sourceName));
        }

        public sealed class Builder<T>
        {
            private readonly Mock<IOneSMappingProvider> _mock;
            private readonly OneSTypeMapper.Builder<T> _builder;

            public Builder(Mock<IOneSMappingProvider> mock, OneSTypeMapper.Builder<T> builder)
            {
                _mock = mock;
                _builder = builder;
            }

            /// <summary>
            /// Установление соответствия между членом типа полю источника данных 1С.
            /// </summary>
            /// <typeparam name="TValue">Тип значения члена типа.</typeparam>
            /// <param name="memberAccessor">Выражение доступа к значению члена типа.</param>
            /// <param name="fieldName">Имя поля.</param>
            public Builder<T> FieldMap<TValue>(Expression<Func<T, TValue>> memberAccessor, string fieldName)
            {
                Contract.Requires<ArgumentNullException>(memberAccessor != null);
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fieldName));
                
                _builder.MapField(memberAccessor, fieldName);

                return this;
            }

            /// <summary>
            /// Завершение инициалзиации карты соответствия для мока.
            /// </summary>
            public void End()
            {
                _mock
                    .Setup(p => p.GetTypeMapping(typeof(T)))
                    .Returns(_builder.End);
            }
        }
    }
}
