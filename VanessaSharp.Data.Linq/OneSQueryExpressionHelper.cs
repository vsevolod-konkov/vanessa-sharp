﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VanessaSharp.Data.Linq
{
    /// <summary>Вспомогательные методы для linq-выражений.</summary>
    internal static class OneSQueryExpressionHelper
    {
        /// <summary>
        /// Получение описание метаданных метода, по лямбда-выражению вызова этого метода.
        /// </summary>
        /// <param name="expression">Лямбда-выражение.</param>
        public static MethodInfo ExtractMethodInfo(LambdaExpression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Requires<ArgumentException>(expression.Body.NodeType == ExpressionType.Call);
            
            return ((MethodCallExpression)expression.Body).Method;
        }

        /// <summary>
        /// Получение описание метаданных метода, по лямбда-выражению вызова этого метода.
        /// </summary>
        /// <param name="expression">Лямбда-выражение.</param>
        public static MethodInfo ExtractMethodInfo<T>(Expression<T> expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Requires<ArgumentException>(expression.Body.NodeType == ExpressionType.Call);

            return ExtractMethodInfo((LambdaExpression)expression);
        }

        /// <summary>
        /// Получение описание метаданных метода <see cref="OneSDataRecord"/>, по лямбда-выражению вызова этого метода.
        /// </summary>
        /// <param name="expression">Лямбда-выражение.</param>
        private static MethodInfo GetDataRecordMethod<T>(Expression<Func<OneSDataRecord, T>> expression)
        {
            return ExtractMethodInfo(expression);
        }

        /// <summary>
        /// Получение описание метаданных метода <see cref="IValueConverter"/>, по лямбда-выражению вызова этого метода.
        /// </summary>
        /// <param name="expression">Лямбда-выражение.</param>
        private static MethodInfo GetValueConverterMethod<T>(Expression<Func<IValueConverter, T>> expression)
        {
            return ExtractMethodInfo(expression);
        }
        
        /// <summary>
        /// Метод <see cref="OneSQueryMethods.GetRecords"/>.
        /// </summary>
        public static MethodInfo GetRecordsMethodInfo
        {
            get { return _getRecordsMethodInfo; }
        }
        private static readonly MethodInfo _getRecordsMethodInfo = GetGetRecordsMethodInfo();

        private static Type GetSingleParameterType(Type genericType)
        {
            Contract.Requires<ArgumentNullException>(genericType != null);
            Contract.Requires<ArgumentException>(genericType.IsGenericType);

            return genericType.GetGenericArguments().Single();
        }

        /// <summary>
        /// Определение, является ли метод методом <see cref="IEnumerable{T}.GetEnumerator"/>.
        /// </summary>
        /// <param name="method">Проверяемый метод.</param>
        /// <param name="itemType">Тип элемента.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если метод является методом <see cref="IEnumerable{T}.GetEnumerator"/>.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        public static bool IsEnumerableGetEnumeratorMethod(MethodInfo method, out Type itemType)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            const string GET_ENUMERATOR_METHOD_NAME = "GetEnumerator";

            var declaringType = method.DeclaringType;
            if (declaringType != null && declaringType.IsInterface && declaringType.IsGenericType &&
                declaringType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                if (method.Name == GET_ENUMERATOR_METHOD_NAME)
                {
                    itemType = GetSingleParameterType(declaringType);
                    return true;
                }
            }

            itemType = default(Type);
            return false;
        }

        // TODO: Убрать копипасту
        /// <summary>
        /// Определяется, является ли метод методом 
        /// <see cref="Queryable.Select{TSource,TResult}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TResult}})"/> 
        /// </summary>
        /// <param name="method">Проверяемый метод.</param>
        /// <param name="itemType">Тип элементов.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если метод является методом
        /// <see cref="Queryable.Select{TSource,TResult}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TResult}})"/>.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        public static bool IsQueryableSelectMethod(MethodInfo method, out Type itemType)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            const string QUERYABLE_SELECT_METHOD_NAME = "Select";
            const int SELECT_EXPRESSION_PARAMETER_INDEX = 1;
            const int FUNC_OUTPUT_TYPE_PARAMETER_INDEX = 1;

            var declaringType = method.DeclaringType;
            if (declaringType == typeof(Queryable))
            {
                if (method.Name == QUERYABLE_SELECT_METHOD_NAME)
                {
                    var selectExpressionType = method.GetParameters()[SELECT_EXPRESSION_PARAMETER_INDEX].ParameterType;
                    Type funcType;
                    if (IsExpressionType(selectExpressionType, typeof(Func<,>), out funcType))
                    {
                        itemType = funcType.GetGenericArguments()[FUNC_OUTPUT_TYPE_PARAMETER_INDEX];
                        return true;
                    }
                }
            }

            itemType = default(Type);
            return false;
        }

        // TODO: Убрать копипасту
        /// <summary>
        /// Определяется, является ли метод методом 
        /// <see cref="Queryable.Where{TSource}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,bool}})"/> 
        /// </summary>
        /// <param name="method">Проверяемый метод.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если метод является методом
        /// <see cref="Queryable.Where{TSource}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,bool}})"/>.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        public static bool IsQueryableWhereMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            const string QUERYABLE_WHERE_METHOD_NAME = "Where";
            const int FILTER_EXPRESSION_PARAMETER_INDEX = 1;

            var declaringType = method.DeclaringType;
            if (declaringType == typeof(Queryable))
            {
                if (method.Name == QUERYABLE_WHERE_METHOD_NAME)
                {
                    var whereExpressionType = method.GetParameters()[FILTER_EXPRESSION_PARAMETER_INDEX].ParameterType;
                    Type funcType;
                    if (IsExpressionType(whereExpressionType, typeof(Func<,>), out funcType))
                        return true;
                }
            }

            return false;
        }

        // TODO: Убрать копипасту
        /// <summary>
        /// Определяется, является ли метод методом 
        /// <see cref="Queryable.OrderBy{TSource,TKey}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/> 
        /// </summary>
        /// <param name="method">Проверяемый метод.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если метод является методом
        /// <see cref="Queryable.OrderBy{TSource,TKey}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        public static bool IsQueryableOrderByMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            const string QUERYABLE_ORDER_BY_METHOD_NAME = "OrderBy";

            var declaringType = method.DeclaringType;
            if (declaringType == typeof(Queryable))
            {
                if (method.Name == QUERYABLE_ORDER_BY_METHOD_NAME && method.GetParameters().Length == 2)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Проверка того, что тип является <see cref="Expression{TDelegate}"/>.
        /// И что делегат является обобщенным типом <paramref name="openDelegateType"/>.
        /// </summary>
        /// <param name="testedType">Тестируемый тип.</param>
        /// <param name="openDelegateType">Обобщенный тип делегата.</param>
        /// <param name="closedDelegateType">
        /// Конкретный тип делегата.
        /// Возвращается <c>null</c>, если проверка вернула <c>false</c>.
        /// </param>
        /// <returns>
        /// Возвращает <c>true</c>, если тип яляется <see cref="Expression{TDelegate}"/>
        /// и тип-параметр делегата в <see cref="Expression{TDelegate}"/> является обощением
        /// <paramref name="openDelegateType"/>.
        /// В ином случае возвращает <c>true</c>.
        /// </returns>
        private static bool IsExpressionType(Type testedType, Type openDelegateType, out Type closedDelegateType)
        {
            if (testedType.IsGenericType
                && (testedType.GetGenericTypeDefinition() == typeof(Expression<>)))
            {
                var parameterType = GetSingleParameterType(testedType);
                if (parameterType.GetGenericTypeDefinition() == openDelegateType)
                {
                    closedDelegateType = parameterType;
                    return true;
                }
            }

            closedDelegateType = default(Type);
            return false;
        }

        #region Методы OneSDataRecord

        /// <summary>Метод <see cref="OneSDataRecord.GetString(string)"/>.</summary>
        public static MethodInfo DataRecordGetStringMethod
        {
            get { return _dataRecordGetStringMethod; }
        }
        private static readonly MethodInfo _dataRecordGetStringMethod = GetDataRecordMethod(r => r.GetString(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetInt32(string)"/>.</summary>
        public static MethodInfo DataRecordGetInt32Method
        {
            get { return _dataRecordGetInt32Method; }
        }
        private static readonly MethodInfo _dataRecordGetInt32Method = GetDataRecordMethod(r => r.GetInt32(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetDouble(string)"/>.</summary>
        public static MethodInfo DataRecordGetDoubleMethod
        {
            get { return _dataRecordGetDoubleMethod; }
        }
        private static readonly MethodInfo _dataRecordGetDoubleMethod = GetDataRecordMethod(r => r.GetDouble(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetBoolean(string)"/>.</summary>
        public static MethodInfo DataRecordGetBooleanMethod
        {
            get { return _dataRecordGetBooleanMethod; }
        }
        private static readonly MethodInfo _dataRecordGetBooleanMethod = GetDataRecordMethod(r => r.GetBoolean(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetDateTime(string)"/>.</summary>
        public static MethodInfo DataRecordGetDateTimeMethod
        {
            get { return _dataRecordGetDateTimeMethod; }
        }
        private static readonly MethodInfo _dataRecordGetDateTimeMethod = GetDataRecordMethod(r => r.GetDateTime(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetChar(string)"/>.</summary>
        public static MethodInfo DataRecordGetCharMethod
        {
            get { return _dataRecordGetCharMethod; }
        }
        private static readonly MethodInfo _dataRecordGetCharMethod = GetDataRecordMethod(r => r.GetChar(""));

        #endregion

        #region Методы IValueConverter

        /// <summary>Метод <see cref="IValueConverter.ToString(object)"/>.</summary>
        public static MethodInfo ValueConverterToStringMethod
        {
            get { return _valueConverterToStringMethod; }
        }
        private static readonly MethodInfo _valueConverterToStringMethod = GetValueConverterMethod(c => c.ToString(""));

        /// <summary>Метод <see cref="IValueConverter.ToInt32"/>.</summary>
        public static MethodInfo ValueConverterToInt32Method
        {
            get { return _valueConverterToInt32Method; }
        }
        private static readonly MethodInfo _valueConverterToInt32Method = GetValueConverterMethod(c => c.ToInt32(0));

        /// <summary>Метод <see cref="IValueConverter.ToDouble"/>.</summary>
        public static MethodInfo ValueConverterToDoubleMethod
        {
            get { return _valueConverterToDoubleMethod; }
        }
        private static readonly MethodInfo _valueConverterToDoubleMethod = GetValueConverterMethod(c => c.ToDouble(0));

        /// <summary>Метод <see cref="IValueConverter.ToBoolean"/>.</summary>
        public static MethodInfo ValueConverterToBooleanMethod
        {
            get { return _valueConverterToBooleanMethod; }
        }
        private static readonly MethodInfo _valueConverterToBooleanMethod = GetValueConverterMethod(c => c.ToBoolean(0));

        /// <summary>Метод <see cref="IValueConverter.ToDateTime"/>.</summary>
        public static MethodInfo ValueConverterToDateTimeMethod
        {
            get { return _valueConverterToDateTimeMethod; }
        }
        private static readonly MethodInfo _valueConverterToDateTimeMethod = GetValueConverterMethod(c => c.ToDateTime(0));

        /// <summary>Метод <see cref="IValueConverter.ToChar(object)"/>.</summary>
        public static MethodInfo ValueConverterToCharMethod
        {
            get { return _valueConverterToCharMethod; }
        }
        private static readonly MethodInfo _valueConverterToCharMethod = GetValueConverterMethod(c => c.ToChar(""));

        #endregion

        /// <summary>
        /// Получение метода <see cref="OneSQueryMethods.GetRecords"/>.
        /// </summary>
        /// <returns></returns>
        private static MethodInfo GetGetRecordsMethodInfo()
        {
            return ExtractMethodInfo<Func<IQueryable<OneSDataRecord>>>(
                () => OneSQueryMethods.GetRecords("[sourceName]"));
        }

        /// <summary>
        /// Генерация выражения получения записей.
        /// </summary>
        /// <param name="sourceName">Имя источника.</param>
        public static Expression GetRecordsExpression(string sourceName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));
            Contract.Ensures(Contract.Result<Expression>() != null);

            return Expression.Call(GetRecordsMethodInfo,
                    Expression.Constant(sourceName));
        }

        /// <summary>Получение типа результата выражения.</summary>
        /// <param name="expression">Выражение.</param>
        public static Type GetResultType(this Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            var lambdaExpression = expression as LambdaExpression;

            return (lambdaExpression == null)
                ? expression.Type
                : lambdaExpression.ReturnType;
        }

        public static MethodInfo GetGetEnumeratorMethodInfo<T>()
        {
            return ItemMethods<T>.GetEnumeratorMethodInfo;
        }

        private static class ItemMethods<T>
        {
            public static MethodInfo GetEnumeratorMethodInfo
            {
                get { return _getEnumeratorMethodInfo; }
            }
            private static readonly MethodInfo _getEnumeratorMethodInfo = GetGetEnumeratorMethodInfo();

            private static MethodInfo GetGetEnumeratorMethodInfo()
            {
                return ExtractMethodInfo<Func<IEnumerable<T>, IEnumerator<T>>>(
                    e => e.GetEnumerator());
            }
        }
    }
}
