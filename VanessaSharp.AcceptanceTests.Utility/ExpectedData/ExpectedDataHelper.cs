using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace VanessaSharp.AcceptanceTests.Utility.ExpectedData
{
    /// <summary>Вспомогательные методы для ожидаемых данных.</summary>
    public static class ExpectedDataHelper
    {
        /// <summary>
        /// Получение информации о поле ожидаемых данных
        /// по выражению доступа к этой информации.
        /// </summary>
        /// <typeparam name="TExpectedData">Тип ожидаемых данных.</typeparam>
        /// <typeparam name="TValue">Тип значения поля.</typeparam>
        /// <param name="fieldAccessorExpression">Выражение доступа к этой информации</param>
        public static ExpectedFieldInfo<TExpectedData> ExtractFieldInfo<TExpectedData, TValue>(
            Expression<Func<TExpectedData, TValue>> fieldAccessorExpression)
        {
            Contract.Requires<ArgumentNullException>(fieldAccessorExpression != null);
            Contract.Ensures(Contract.Result<ExpectedFieldInfo<TExpectedData>>() != null);

            var fieldInfo = GetFieldInfo(fieldAccessorExpression);
            var fieldAttr = GetFieldAttribute(fieldInfo);

            var fieldType = fieldAttr.FieldType ?? fieldInfo.FieldType;

            var fieldAccessorFunc = Expression.Lambda<Func<TExpectedData, object>>(
                Expression.Convert(fieldAccessorExpression.Body, typeof(object)),
                fieldAccessorExpression.Parameters)
                .Compile();

            return new ExpectedFieldInfo<TExpectedData>(
                fieldAttr.FieldName,
                fieldType,
                fieldAccessorFunc
                );
        }

        private static FieldInfo GetFieldInfo(LambdaExpression fieldAccessorExpression)
        {
            Contract.Requires<ArgumentNullException>(fieldAccessorExpression != null);
            Contract.Ensures(Contract.Result<FieldInfo>() != null);

            var memberExpression = fieldAccessorExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException(string.Format(
                    "Выражение \"{0}\" не является выражением доступа к полю.",
                    fieldAccessorExpression));
            }

            var fieldInfo = memberExpression.Member as FieldInfo;
            if (fieldInfo == null)
            {
                throw new ArgumentException(string.Format(
                    "Член \"{0}\" не является полем типа.",
                    fieldAccessorExpression));
            }

            return fieldInfo;
        }

        private static FieldAttribute GetFieldAttribute(FieldInfo fieldInfo)
        {
            Contract.Requires<ArgumentNullException>(fieldInfo != null);
            Contract.Ensures(Contract.Result<FieldAttribute>() != null);

            var attrType = typeof(FieldAttribute);

            if (!fieldInfo.IsDefined(attrType, false))
            {
                throw new ArgumentException(string.Format(
                    "Поле \"{0}\" не маркировано атрибутом \"{1}\".",
                    fieldInfo, attrType));
            }

            return (FieldAttribute)fieldInfo.GetCustomAttributes(attrType, false)[0];
        }

        /// <summary>
        /// Получение экземпляров ожидаемых данных.
        /// </summary>
        /// <typeparam name="TExpectedData">
        /// Тип ожидаемых данных.
        /// </typeparam>
        public static IList<TExpectedData> GetExpectedData<TExpectedData>()
        {
            Contract.Ensures(Contract.Result<IList<TExpectedData>>() != null);
            
            const string EXPECTED_DATA_PROPERTY = "ExpectedData";

            var expectedDataProperty = typeof(TExpectedData)
                .GetProperty(EXPECTED_DATA_PROPERTY,
                             BindingFlags.Public | BindingFlags.Static);

            if (expectedDataProperty == null)
            {
                throw new InvalidOperationException(string.Format(
                    "В типе ожидаемых данных \"{0}\" не найдено статическое свойство \"{1}\" предназначенное для получения ожидаемых экземпляров.",
                    typeof(TExpectedData), EXPECTED_DATA_PROPERTY));
            }

            if (!typeof(IList<TExpectedData>).IsAssignableFrom(expectedDataProperty.PropertyType))
            {
                throw new InvalidOperationException(string.Format(
                    "Тип \"{0}\" свойства \"{1}\" ожидаемых данных описываемых типом \"{2}\" не приводим к типу \"{3}\".",
                    expectedDataProperty.PropertyType, EXPECTED_DATA_PROPERTY,
                    typeof(TExpectedData), typeof(IList<TExpectedData>)));
            }

            var result = (IList<TExpectedData>)expectedDataProperty.GetValue(null, null);

            if (result == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Cвойство \"{0}\" ожидаемых данных описываемых типом \"{1}\" вернуло null.",
                    EXPECTED_DATA_PROPERTY, typeof(TExpectedData)));
            }

            return result;
        }

        /// <summary>Информация по полю типа ожидаемых данных.</summary>
        /// <typeparam name="T">Тип.</typeparam>
        public sealed class ExpectedFieldInfo<T>
        {
            /// <summary>Конструктор.</summary>
            /// <param name="name">Имя поля.</param>
            /// <param name="type">Тип поля.</param>
            /// <param name="accessor">Делегат доступа к значению поля.</param>
            public ExpectedFieldInfo(string name, Type type, Func<T, object> accessor)
            {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
                Contract.Requires<ArgumentNullException>(type != null);
                Contract.Requires<ArgumentNullException>(accessor != null);

                _name = name;
                _type = type;
                _accessor = accessor;
            }

            /// <summary>Имя поля.</summary>
            public string Name
            {
                get
                {
                    Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

                    return _name;
                }
            }
            private readonly string _name;

            /// <summary>Тип поля.</summary>
            public Type Type
            {
                get
                {
                    Contract.Ensures(Contract.Result<Type>() != null);
                    
                    return _type;
                }
            }
            private readonly Type _type;

            /// <summary>Делегат доступа к значению поля.</summary>
            public Func<T, object> Accessor
            {
                get
                {
                    Contract.Ensures(Contract.Result<Func<T, object>>() != null);

                    return _accessor;
                }
            }
            private readonly Func<T, object> _accessor;
        }
    }
}
