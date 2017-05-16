using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal;

namespace VanessaSharp.Data.Linq.UnitTests.Utility
{
    /// <summary>
    /// Класс для создания информации о соответствии типизированной записи
    /// источнику данных 1С fluent-стилем.
    /// </summary>
    internal static class OneSTypeMapper
    {
        /// <summary>
        /// Создание соответствия поля к источнику данных 1С.
        /// </summary>    
        private static OneSFieldMapping CreateFieldMapping<T, TResult>(Expression<Func<T, TResult>> accessor, string fieldName, OneSDataColumnKind kind)
        {
            var memberInfo = ((MemberExpression)accessor.Body).Member;

            return new OneSFieldMapping(memberInfo, fieldName, kind);
        }
        
        /// <summary>Начало описания маппинга типизированной записи к источнику данных 1С верхнего уровня.</summary>
        /// <typeparam name="T">Тип типизированной записи.</typeparam>
        public static RootBuilder<T> BeginForRoot<T>(string sourceName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));
            
            return new RootBuilder<T>(sourceName);
        }

        /// <summary>Начало описания маппинга типизированной записи к источнику данных 1С уровня табличной части.</summary>
        /// <typeparam name="T">Тип типизированной записи.</typeparam>
        public static TablePartBuilder<T> BeginForTablePart<T>(Type ownerType = null)
        {
            return new TablePartBuilder<T>(ownerType);
        }

        /// <summary>
        /// Базовый класс билдера маппинга
        /// </summary>
        /// <typeparam name="T">Тип записи, для которой строится маппинг.</typeparam>
        /// <typeparam name="TResult">Тип результата построения.</typeparam>
        public abstract class BuilderBase<T, TResult>
        {
            private readonly List<OneSFieldMapping> _fieldMappings = new List<OneSFieldMapping>();

            /// <summary>
            /// Установление соответствия между членом типа полю источника данных 1С.
            /// </summary>
            /// <typeparam name="TValue">Тип значения члена типа.</typeparam>
            /// <param name="memberAccessor">Выражение доступа к значению члена типа.</param>
            /// <param name="fieldName">Имя поля.</param>
            /// <param name="kind">Тип поля: реквизит или табличная часть.</param>
            protected void InternalMapField<TValue>(Expression<Func<T, TValue>> memberAccessor, string fieldName, OneSDataColumnKind kind)
            {
                Contract.Requires<ArgumentNullException>(memberAccessor != null);
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fieldName));

                _fieldMappings.Add(CreateFieldMapping(memberAccessor, fieldName, kind));
            }

            protected ReadOnlyCollection<OneSFieldMapping> GetFieldMappings()
            {
                return _fieldMappings.ToReadOnly();
            }

            /// <summary>Уровень тестируемых данных.</summary>
            public abstract OneSDataLevel Level { get; }

            /// <summary>Завершение построения.</summary>
            public abstract TResult End { get; }
        }

        /// <summary>
        /// Билдер маппинга верхнего уровня.
        /// </summary>
        /// <typeparam name="T">Тип записи, для которой строится маппинг.</typeparam>
        public sealed class RootBuilder<T> : BuilderBase<T, OneSTypeMapping>
        {
            private readonly string _sourceName;
             
            public RootBuilder(string sourceName)
            {
                _sourceName = sourceName;
            }

            /// <summary>
            /// Установление соответствия между членом типа полю источника данных 1С.
            /// </summary>
            /// <typeparam name="TValue">Тип значения члена типа.</typeparam>
            /// <param name="memberAccessor">Выражение доступа к значению члена типа.</param>
            /// <param name="fieldName">Имя поля.</param>
            /// <param name="kind">Тип поля: реквизит или табличная часть.</param>
            public RootBuilder<T> MapField<TValue>(Expression<Func<T, TValue>> memberAccessor, string fieldName, OneSDataColumnKind kind)
            {
                Contract.Requires<ArgumentNullException>(memberAccessor != null);
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fieldName));

                InternalMapField(memberAccessor, fieldName, kind);

                return this;
            }

            /// <summary>Уровень тестируемых данных.</summary>
            public override OneSDataLevel Level
            {
                get { return OneSDataLevel.Root; }
            }

            /// <summary>Завершение описания.</summary>
            public override OneSTypeMapping End
            {
                get
                {
                    return new OneSTypeMapping(
                        _sourceName, GetFieldMappings());
                }
            }
        }

        /// <summary>
        /// Билдер маппинга верхнего уровня.
        /// </summary>
        /// <typeparam name="T">Тип записи, для которой строится маппинг.</typeparam>
        public sealed class TablePartBuilder<T> : BuilderBase<T, OneSTablePartTypeMapping>
        {
            private readonly Type _ownerType;

            public TablePartBuilder(Type ownerType)
            {
                _ownerType = ownerType;
            }

            /// <summary>
            /// Установление соответствия между членом типа полю источника данных 1С.
            /// </summary>
            /// <typeparam name="TValue">Тип значения члена типа.</typeparam>
            /// <param name="memberAccessor">Выражение доступа к значению члена типа.</param>
            /// <param name="fieldName">Имя поля.</param>
            public TablePartBuilder<T> MapField<TValue>(Expression<Func<T, TValue>> memberAccessor, string fieldName)
            {
                Contract.Requires<ArgumentNullException>(memberAccessor != null);
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fieldName));

                InternalMapField(memberAccessor, fieldName, OneSDataColumnKind.Property);

                return this;
            }

            /// <summary>Уровень тестируемых данных.</summary>
            public override OneSDataLevel Level
            {
                get { return OneSDataLevel.TablePart; }
            }

            /// <summary>Завершение описания.</summary>
            public override OneSTablePartTypeMapping End
            {
                get
                {
                    return new OneSTablePartTypeMapping(_ownerType, GetFieldMappings());
                }
            }
        }
    }
}
