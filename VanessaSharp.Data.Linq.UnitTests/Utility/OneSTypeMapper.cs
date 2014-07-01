using System;
using System.Collections.Generic;
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
        private static OneSFieldMapping CreateFieldMapping<T, TResult>(Expression<Func<T, TResult>> accessor, string fieldName)
        {
            var memberInfo = ((MemberExpression)accessor.Body).Member;

            return new OneSFieldMapping(memberInfo, fieldName);
        }
        
        /// <summary>Начало описания маппинга типизированной записи к источнику данных 1С.</summary>
        /// <typeparam name="T">Тип типизированной записи.</typeparam>
        public static Builder<T> BeginFor<T>(string sourceName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));
            
            return new Builder<T>(sourceName);
        }

        /// <summary>
        /// Билдер маппинга.
        /// </summary>
        /// <typeparam name="T">Тип записи, для которой строится маппинг.</typeparam>
        public sealed class Builder<T>
        {
            private readonly string _sourceName;
             
            private readonly List<OneSFieldMapping> _fieldMappings = new List<OneSFieldMapping>();

            public Builder(string sourceName)
            {
                _sourceName = sourceName;
            }

            /// <summary>
            /// Установление соответствия между членом типа полю источника данных 1С.
            /// </summary>
            /// <typeparam name="TValue">Тип значения члена типа.</typeparam>
            /// <param name="memberAccessor">Выражение доступа к значению члена типа.</param>
            /// <param name="fieldName">Имя поля.</param>
            public Builder<T> MapField<TValue>(Expression<Func<T, TValue>> memberAccessor, string fieldName)
            {
                Contract.Requires<ArgumentNullException>(memberAccessor != null);
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fieldName));

                _fieldMappings.Add(CreateFieldMapping(memberAccessor, fieldName));

                return this;
            }

            /// <summary>Завершение описания.</summary>
            public OneSTypeMapping End
            {
                get
                {
                    return new OneSTypeMapping(
                        _sourceName, _fieldMappings.ToReadOnly());
                }
            }
        }
    }
}
