using System;
using System.Collections.ObjectModel;
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
        /// в источнике данных 1С для мока верхнего уровня.
        /// </summary>
        /// <typeparam name="T">Тип записи.</typeparam>
        /// <param name="mock">Объект мока.</param>
        /// <param name="sourceName">Имя источника данных 1С, соответствующего типу записи.</param>
        public static RootBuilder<T> BeginSetupGetTypeMappingForRoot<T>(this Mock<IOneSMappingProvider> mock, string sourceName)
        {
            Contract.Requires<ArgumentNullException>(mock != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));
            
            return new RootBuilder<T>(mock, OneSTypeMapper.BeginForRoot<T>(sourceName));
        }

        /// <summary>
        /// Старт описания карты соответстия типизированной записи
        /// в источнике данных 1С для мока уровня табличной части.
        /// </summary>
        /// <typeparam name="T">Тип записи.</typeparam>
        /// <param name="mock">Объект мока.</param>
        public static TablePartBuilder<T> BeginSetupGetTypeMappingForTablePart<T>(this Mock<IOneSMappingProvider> mock)
        {
            Contract.Requires<ArgumentNullException>(mock != null);

            return new TablePartBuilder<T>(mock, OneSTypeMapper.BeginForTablePart<T>());
        }

        /// <summary>Базовый класс построителя.</summary>
        /// <typeparam name="T">Тип данных.</typeparam>
        /// <typeparam name="TInnerBuilder">Тип внутреннего построителя метаданных.</typeparam>
        /// <typeparam name="TResult">Тип результата внутреннего построителя.</typeparam>
        public abstract class BuilderBase<T, TInnerBuilder, TResult>
            where TInnerBuilder : OneSTypeMapper.BuilderBase<T, TResult>
        {
            private readonly Mock<IOneSMappingProvider> _mock;
            
            protected BuilderBase(Mock<IOneSMappingProvider> mock, TInnerBuilder innerBuilder)
            {
                _mock = mock;
                _innerBuilder = innerBuilder;
            }
            
            /// <summary>
            /// Внутренний построитель метаданных.
            /// </summary>
            protected TInnerBuilder InnerBuilder
            {
                get { return _innerBuilder; }
            }
            private readonly TInnerBuilder _innerBuilder;

            /// <summary>
            /// Завершение инициалзиации карты соответствия для мока.
            /// </summary>
            public void End()
            {
                var level = InnerBuilder.Level;
                var type = typeof(T);

                _mock.Setup(p => p.CheckDataType(level, type));

                _mock
                    .Setup(p => p.IsDataType(level, type))
                    .Returns(true);

                _mock
                    .Setup(GetResultExpression())
                    .Returns(InnerBuilder.End);
            }

            /// <summary>
            /// Получение выражение для получения результата построения метаданных.
            /// </summary>
            protected abstract Expression<Func<IOneSMappingProvider, TResult>> GetResultExpression();
        }

        /// <summary>Построитель верхнего уровня.</summary>
        /// <typeparam name="T">Тип данных верхнего уровня.</typeparam>
        public sealed class RootBuilder<T> : BuilderBase<T, OneSTypeMapper.RootBuilder<T>, OneSTypeMapping>
        {
            public RootBuilder(Mock<IOneSMappingProvider> mock, OneSTypeMapper.RootBuilder<T> innerBuilder)
                : base(mock, innerBuilder)
            {}

            /// <summary>
            /// Установление соответствия между членом типа полю источника данных 1С.
            /// </summary>
            /// <typeparam name="TValue">Тип значения члена типа.</typeparam>
            /// <param name="memberAccessor">Выражение доступа к значению члена типа.</param>
            /// <param name="fieldName">Имя поля.</param>
            /// <param name="kind">Тип поля: реквизит или табличная часть.</param>
            public RootBuilder<T> FieldMap<TValue>(Expression<Func<T, TValue>> memberAccessor, string fieldName, OneSDataColumnKind kind = OneSDataColumnKind.Default)
            {
                Contract.Requires<ArgumentNullException>(memberAccessor != null);
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fieldName));
                
                InnerBuilder.MapField(memberAccessor, fieldName, kind);

                return this;
            }

            /// <summary>
            /// Получение выражение для получения результата построения метаданных.
            /// </summary>
            protected override Expression<Func<IOneSMappingProvider, OneSTypeMapping>> GetResultExpression()
            {
                return p => p.GetRootTypeMapping(typeof(T));
            }
        }

        /// <summary>Построитель для уровня табличной части.</summary>
        /// <typeparam name="T">Тип данных табличной части.</typeparam>
        public sealed class TablePartBuilder<T>
            : BuilderBase<T, OneSTypeMapper.TablePartBuilder<T>, OneSTablePartTypeMapping>
        {
            public TablePartBuilder(Mock<IOneSMappingProvider> mock, OneSTypeMapper.TablePartBuilder<T> builder)
                : base(mock, builder)
            {}

            /// <summary>
            /// Установление соответствия между членом типа полю источника данных 1С.
            /// </summary>
            /// <typeparam name="TValue">Тип значения члена типа.</typeparam>
            /// <param name="memberAccessor">Выражение доступа к значению члена типа.</param>
            /// <param name="fieldName">Имя поля.</param>
            public TablePartBuilder<T> FieldMap<TValue>(Expression<Func<T, TValue>> memberAccessor, string fieldName)
            {
                Contract.Requires<ArgumentNullException>(memberAccessor != null);
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fieldName));

                InnerBuilder.MapField(memberAccessor, fieldName);

                return this;
            }

            /// <summary>
            /// Получение выражение для получения результата построения метаданных.
            /// </summary>
            protected override Expression<Func<IOneSMappingProvider, OneSTablePartTypeMapping>> GetResultExpression()
            {
                return p => p.GetTablePartTypeMappings(typeof(T));
            }
        }
    }
}
