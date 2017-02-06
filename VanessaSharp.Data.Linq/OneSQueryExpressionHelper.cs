using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;

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

        /// <summary>
        /// Метод <see cref="OneSQueryMethods.GetTypedRecords{T}"/>.
        /// </summary>
        /// <typeparam name="T">Тип запрашиваемых записей данных.</typeparam>
        public static MethodInfo GetGetTypedRecordsMethodInfo<T>()
        {
            return ExtractMethodInfo<Func<IQueryable<T>>>(
                () => OneSQueryMethods.GetTypedRecords<T>());
        }

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

        /// <summary>
        /// Определяет, является ли метод <paramref name="method"/>
        /// методом query-методом <see cref="Queryable"/> с именем <paramref name="testedMethodName"/>.
        /// </summary>
        /// <param name="method">Проверяемый метод.</param>
        /// <param name="testedMethodName">Ожидаемое имя метода.</param>
        private static bool IsQueryableMethod(MethodInfo method, string testedMethodName)
        {
            return method.DeclaringType == typeof(Queryable)
                && method.Name == testedMethodName;
        }

        /// <summary>
        /// Является ли метод методом <see cref="Queryable"/> выражением 
        /// и является ли второй параметр метода <see cref="Queryable"/>.
        /// </summary>
        /// <param name="method">Проверяемый метод.</param>
        /// <param name="testedMethodName">Ожидаемое имя метода.</param>
        private static bool IsQueryableMethodAndSecondParameterIsExpression(MethodInfo method, string testedMethodName)
        {
            if (!IsQueryableMethod(method, testedMethodName))
                return false;

            const int EXPRESSION_PARAMETER_INDEX = 1;

            var expressionType = method.GetParameters()[EXPRESSION_PARAMETER_INDEX].ParameterType;
            return IsExpressionType(expressionType, typeof(Func<,>));
        }

        /// <summary>
        /// Определяется, является ли метод методом 
        /// <see cref="Queryable.Select{TSource,TResult}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TResult}})"/> 
        /// </summary>
        /// <param name="method">Проверяемый метод.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если метод является методом
        /// <see cref="Queryable.Select{TSource,TResult}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TResult}})"/>.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        public static bool IsQueryableSelectMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            const string QUERYABLE_SELECT_METHOD_NAME = "Select";

            return IsQueryableMethodAndSecondParameterIsExpression(method, QUERYABLE_SELECT_METHOD_NAME);
        }

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

            return IsQueryableMethodAndSecondParameterIsExpression(method, QUERYABLE_WHERE_METHOD_NAME);
        }

        /// <summary>
        /// Определяет, является ли метод <paramref name="method"/>
        /// методом сортировки с именем <paramref name="testedMethodName"/>.
        /// </summary>
        /// <param name="method">Проверяемый метод.</param>
        /// <param name="testedMethodName">Ожидаемое имя метода сортировки.</param>
        private static bool IsQueryableSortMethod(MethodInfo method, string testedMethodName)
        {
            return IsQueryableMethod(method, testedMethodName)
                && method.GetParameters().Length == 2;
        }

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

            return IsQueryableSortMethod(method, QUERYABLE_ORDER_BY_METHOD_NAME);
        }

        /// <summary>
        /// Определяется, является ли метод методом 
        /// <see cref="Queryable.OrderByDescending{TSource,TKey}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/> 
        /// </summary>
        /// <param name="method">Проверяемый метод.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если метод является методом
        /// <see cref="Queryable.OrderByDescending{TSource,TKey}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        public static bool IsQueryableOrderByDescendingMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            const string QUERYABLE_ORDER_BY_DESCENDING_METHOD_NAME = "OrderByDescending";

            return IsQueryableSortMethod(method, QUERYABLE_ORDER_BY_DESCENDING_METHOD_NAME);
        }

        /// <summary>
        /// Определяется, является ли метод методом 
        /// <see cref="Queryable.ThenBy{TSource,TKey}(System.Linq.IOrderedQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/> 
        /// </summary>
        /// <param name="method">Проверяемый метод.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если метод является методом
        /// <see cref="Queryable.ThenBy{TSource,TKey}(System.Linq.IOrderedQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        public static bool IsQueryableThenByMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            const string QUERYABLE_THEN_BY_METHOD_NAME = "ThenBy";

            return IsQueryableSortMethod(method, QUERYABLE_THEN_BY_METHOD_NAME);
        }

        /// <summary>
        /// Определяется, является ли метод методом 
        /// <see cref="Queryable.ThenByDescending{TSource,TKey}(System.Linq.IOrderedQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/> 
        /// </summary>
        /// <param name="method">Проверяемый метод.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если метод является методом
        /// <see cref="Queryable.ThenByDescending{TSource,TKey}(System.Linq.IOrderedQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        public static bool IsQueryableThenByDescendingMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            const string QUERYABLE_THEN_BY_DESCENDING_METHOD_NAME = "ThenByDescending";

            return IsQueryableSortMethod(method, QUERYABLE_THEN_BY_DESCENDING_METHOD_NAME);
        }

        /// <summary>
        /// Определяет, является ли метод <paramref name="method"/>
        /// методом query-методом <see cref="Queryable"/> с именем <paramref name="testedMethodName"/>
        /// и не имеющий дополнительных аргументов.
        /// </summary>
        /// <param name="method">Проверяемый метод.</param>
        /// <param name="testedMethodName">Ожидаемое имя метода.</param>
        private static bool IsQueryableMethodWithoutArgs(MethodInfo method, string testedMethodName)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            return IsQueryableMethod(method, testedMethodName)
                && method.GetParameters().Length == 1;
        }

        /// <summary>
        /// Определяется, является ли метод методом
        /// <see cref="Queryable.Distinct{TSource}(System.Linq.IQueryable{TSource})"/>
        /// </summary>
        public static bool IsQueryableDistinctMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            return IsQueryableMethodWithoutArgs(method, "Distinct");
        }

        /// <summary>
        /// Определяется, является ли метод методом
        /// <see cref="Queryable.Sum(System.Linq.IQueryable{int})"/>
        /// </summary>
        public static bool IsQueryableSumMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            return IsQueryableMethodWithoutArgs(method, "Sum");
        }

        /// <summary>
        /// Определяется, является ли метод методом
        /// <see cref="Queryable.Average(System.Linq.IQueryable{int})"/>
        /// </summary>
        public static bool IsQueryableAverageMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            return IsQueryableMethodWithoutArgs(method, "Average");
        }

        /// <summary>
        /// Определяется, является ли метод методом
        /// <see cref="Queryable.Max{TSource}(System.Linq.IQueryable{TSource})"/>
        /// </summary>
        public static bool IsQueryableMaxMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            return IsQueryableMethodWithoutArgs(method, "Max");
        }

        /// <summary>
        /// Определяется, является ли метод методом
        /// <see cref="Queryable.Min{TSource}(System.Linq.IQueryable{TSource})"/>
        /// </summary>
        public static bool IsQueryableMinMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            return IsQueryableMethodWithoutArgs(method, "Min");
        }

        /// <summary>
        /// Определяется, является ли метод методом
        /// <see cref="Queryable.Count{TSource}(System.Linq.IQueryable{TSource})"/>
        /// </summary>
        public static bool IsQueryableCountMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            return IsQueryableMethodWithoutArgs(method, "Count");
        }

        /// <summary>
        /// Определяется, является ли метод методом
        /// <see cref="Queryable.LongCount{TSource}(System.Linq.IQueryable{TSource})"/>
        /// </summary>
        public static bool IsQueryableLongCountMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            return IsQueryableMethodWithoutArgs(method, "LongCount");
        }

        /// <summary>
        /// Определяется, является ли метод методом
        /// <see cref="Queryable.Take{TSource}"/>
        /// </summary>
        public static bool IsQueryableTakeMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            return IsQueryableMethod(method, "Take");
        }

        /// <summary>
        /// Проверка того, что тип является <see cref="Expression{TDelegate}"/>.
        /// И что делегат является обобщенным типом <paramref name="openDelegateType"/>.
        /// </summary>
        /// <param name="testedType">Тестируемый тип.</param>
        /// <param name="openDelegateType">Обобщенный тип делегата.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если тип яляется <see cref="Expression{TDelegate}"/>
        /// и тип-параметр делегата в <see cref="Expression{TDelegate}"/> является обощением
        /// <paramref name="openDelegateType"/>.
        /// В ином случае возвращает <c>true</c>.
        /// </returns>
        private static bool IsExpressionType(Type testedType, Type openDelegateType)
        {
            if (testedType.IsGenericType
                && (testedType.GetGenericTypeDefinition() == typeof(Expression<>)))
            {
                var parameterType = GetSingleParameterType(testedType);
                return (parameterType.GetGenericTypeDefinition() == openDelegateType);
            }

            return false;
        }

        #region Методы Enumerable

        /// <summary>
        /// Является ли методом <see cref="Enumerable.Contains{TSource}(System.Collections.Generic.IEnumerable{TSource},TSource)"/>
        /// </summary>
        public static bool IsEnumerableContainsMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            return method.DeclaringType == typeof(Enumerable)
                   && method.Name == "Contains"
                   && method.GetParameters().Length == 2;
        }

        public static bool IsEnumerableSelectMethod(MethodInfo method)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            return method.DeclaringType == typeof(Enumerable)
                   && method.Name == "Select"
                   && method.GetParameters().Length == 2;
        }

        #endregion

        #region Методы OneSDataRecord

        /// <summary>Метод <see cref="OneSDataRecord.GetString(string)"/>.</summary>
        public static MethodInfo DataRecordGetStringMethod
        {
            get { return _dataRecordGetStringMethod; }
        }
        private static readonly MethodInfo _dataRecordGetStringMethod = GetDataRecordMethod(r => r.GetString(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetChar(string)"/>.</summary>
        public static MethodInfo DataRecordGetCharMethod
        {
            get { return _dataRecordGetCharMethod; }
        }
        private static readonly MethodInfo _dataRecordGetCharMethod = GetDataRecordMethod(r => r.GetChar(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetByte(string)"/>.</summary>
        public static MethodInfo DataRecordGetByteMethod
        {
            get { return _dataRecordGetByteMethod; }
        }
        private static readonly MethodInfo _dataRecordGetByteMethod = GetDataRecordMethod(r => r.GetByte(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetInt16(string)"/>.</summary>
        public static MethodInfo DataRecordGetInt16Method
        {
            get { return _dataRecordGetInt16Method; }
        }
        private static readonly MethodInfo _dataRecordGetInt16Method = GetDataRecordMethod(r => r.GetInt16(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetInt32(string)"/>.</summary>
        public static MethodInfo DataRecordGetInt32Method
        {
            get { return _dataRecordGetInt32Method; }
        }
        private static readonly MethodInfo _dataRecordGetInt32Method = GetDataRecordMethod(r => r.GetInt32(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetInt64(string)"/>.</summary>
        public static MethodInfo DataRecordGetInt64Method
        {
            get { return _dataRecordGetInt64Method; }
        }
        private static readonly MethodInfo _dataRecordGetInt64Method = GetDataRecordMethod(r => r.GetInt64(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetFloat(string)"/>.</summary>
        public static MethodInfo DataRecordGetFloatMethod
        {
            get { return _dataRecordGetFloatMethod; }
        }
        private static readonly MethodInfo _dataRecordGetFloatMethod = GetDataRecordMethod(r => r.GetFloat(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetDouble(string)"/>.</summary>
        public static MethodInfo DataRecordGetDoubleMethod
        {
            get { return _dataRecordGetDoubleMethod; }
        }
        private static readonly MethodInfo _dataRecordGetDoubleMethod = GetDataRecordMethod(r => r.GetDouble(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetDecimal(string)"/>.</summary>
        public static MethodInfo DataRecordGetDecimalMethod
        {
            get { return _dataRecordGetDecimalMethod; }
        }
        private static readonly MethodInfo _dataRecordGetDecimalMethod = GetDataRecordMethod(r => r.GetDecimal(""));

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

        /// <summary>Метод <see cref="OneSDataRecord.GetGuid(string)"/>.</summary>
        public static MethodInfo DataRecordGetGuidMethod
        {
            get { return _dataRecordGetGuidMethod; }
        }
        private static readonly MethodInfo _dataRecordGetGuidMethod = GetDataRecordMethod(r => r.GetGuid(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetValue(string)"/>.</summary>
        public static MethodInfo DataRecordGetValueMethod
        {
            get {  return _dataRecordGetValueMethod; }
        }
        private static readonly MethodInfo _dataRecordGetValueMethod = GetDataRecordMethod(r => r.GetValue(""));

        /// <summary>Метод <see cref="OneSDataRecord.GetTablePartRecords(string)"/>.</summary>
        public static MethodInfo DataRecordGetTablePartRecords
        {
            get { return _dataRecordGetTablePartRecords; }
        }
        private static readonly MethodInfo _dataRecordGetTablePartRecords = GetDataRecordMethod(r => r.GetTablePartRecords(""));

        #endregion

        #region Методы IValueConverter

        /// <summary>Метод <see cref="IValueConverter.ToString(object)"/>.</summary>
        public static MethodInfo ValueConverterToStringMethod
        {
            get { return _valueConverterToStringMethod; }
        }
        private static readonly MethodInfo _valueConverterToStringMethod = GetValueConverterMethod(c => c.ToString(null));

        /// <summary>Метод <see cref="IValueConverter.ToChar(object)"/>.</summary>
        public static MethodInfo ValueConverterToCharMethod
        {
            get { return _valueConverterToCharMethod; }
        }
        private static readonly MethodInfo _valueConverterToCharMethod = GetValueConverterMethod(c => c.ToChar(null));

        /// <summary>Метод <see cref="IValueConverter.ToByte(object)"/>.</summary>
        public static MethodInfo ValueConverterToByteMethod
        {
            get { return _valueConverterToByteMethod; }
        }
        private static readonly MethodInfo _valueConverterToByteMethod = GetValueConverterMethod(c => c.ToByte(null));

        /// <summary>Метод <see cref="IValueConverter.ToInt16(object)"/>.</summary>
        public static MethodInfo ValueConverterToInt16Method
        {
            get { return _valueConverterToInt16Method; }
        }
        private static readonly MethodInfo _valueConverterToInt16Method = GetValueConverterMethod(c => c.ToInt16(null));

        /// <summary>Метод <see cref="IValueConverter.ToInt32"/>.</summary>
        public static MethodInfo ValueConverterToInt32Method
        {
            get { return _valueConverterToInt32Method; }
        }
        private static readonly MethodInfo _valueConverterToInt32Method = GetValueConverterMethod(c => c.ToInt32(null));

        /// <summary>Метод <see cref="IValueConverter.ToInt64"/>.</summary>
        public static MethodInfo ValueConverterToInt64Method
        {
            get { return _valueConverterToInt64Method; }
        }
        private static readonly MethodInfo _valueConverterToInt64Method = GetValueConverterMethod(c => c.ToInt64(null));

        /// <summary>Метод <see cref="IValueConverter.ToFloat"/>.</summary>
        public static MethodInfo ValueConverterToFloatMethod
        {
            get { return _valueConverterToFloatMethod; }
        }
        private static readonly MethodInfo _valueConverterToFloatMethod = GetValueConverterMethod(c => c.ToFloat(null));

        /// <summary>Метод <see cref="IValueConverter.ToDouble"/>.</summary>
        public static MethodInfo ValueConverterToDoubleMethod
        {
            get { return _valueConverterToDoubleMethod; }
        }
        private static readonly MethodInfo _valueConverterToDoubleMethod = GetValueConverterMethod(c => c.ToDouble(null));

        /// <summary>Метод <see cref="IValueConverter.ToDecimal"/>.</summary>
        public static MethodInfo ValueConverterToDecimalMethod
        {
            get { return _valueConverterToDecimalMethod; }
        }
        private static readonly MethodInfo _valueConverterToDecimalMethod = GetValueConverterMethod(c => c.ToDecimal(null));

        /// <summary>Метод <see cref="IValueConverter.ToBoolean"/>.</summary>
        public static MethodInfo ValueConverterToBooleanMethod
        {
            get { return _valueConverterToBooleanMethod; }
        }
        private static readonly MethodInfo _valueConverterToBooleanMethod = GetValueConverterMethod(c => c.ToBoolean(null));

        /// <summary>Метод <see cref="IValueConverter.ToDateTime"/>.</summary>
        public static MethodInfo ValueConverterToDateTimeMethod
        {
            get { return _valueConverterToDateTimeMethod; }
        }
        private static readonly MethodInfo _valueConverterToDateTimeMethod = GetValueConverterMethod(c => c.ToDateTime(null));

        /// <summary>
        /// Получение метода <see cref="IValueConverter"/>
        /// для конвертации значения к типу <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Тип к которому необходимо конвертировать значение.</param>
        public static MethodInfo GetValueConvertMethod(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            MethodInfo method;
            if (_valueConvertMethodsMap.TryGetValue(type, out method))
                return method;

            throw new InvalidOperationException(string.Format(
                "Для типа \"{0}\" не найден метод конвертации значения 1С.", type));
        }
        private static readonly Dictionary<Type, MethodInfo> _valueConvertMethodsMap = new Dictionary<Type, MethodInfo>
            {
                { typeof(string), ValueConverterToStringMethod },
                { typeof(char), ValueConverterToCharMethod },
                { typeof(byte), ValueConverterToByteMethod },
                { typeof(short), ValueConverterToInt16Method },
                { typeof(int), ValueConverterToInt32Method },
                { typeof(long), ValueConverterToInt64Method },
                { typeof(float), ValueConverterToFloatMethod },
                { typeof(double), ValueConverterToDoubleMethod },
                { typeof(decimal), ValueConverterToDecimalMethod },
                { typeof(bool), ValueConverterToBooleanMethod },
                { typeof(DateTime), ValueConverterToDateTimeMethod }
            };

        /// <summary>Получение конвертера, преобразующего сырое значение 1С в значение требуемого типа.</summary>
        /// <typeparam name="T">Требуемый тип.</typeparam>
        public static Func<IValueConverter, object, T> GetConverter<T>()
        {
            return (Func<IValueConverter, object, T>)GetConverter(typeof(T));
        }

        /// <summary>Получение конвертера, преобразующего сырое значение 1С в значение требуемого типа.</summary>
        private static Delegate GetConverter(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            Delegate result;
            if (_convertersMap.TryGetValue(type, out result))
                return result;

            throw new InvalidOperationException(string.Format(
                "Для типа \"{0}\" не найден метод конвертации значения 1С.", type));
        }
        private static readonly Dictionary<Type, Delegate> _convertersMap
            = CreateConvertersMap();

        /// <summary>Создание карты соответствия типа, конвертеру преобразующему к данному типу.</summary>
        private static Dictionary<Type, Delegate> CreateConvertersMap()
        {
            return _valueConvertMethodsMap
                .ToDictionary(p => p.Key, p => CreateConverter(p.Key, p.Value));
        }

        /// <summary>Создание конвертера по типу и методу <see cref="IValueConverter"/>.</summary>
        private static Delegate CreateConverter(Type type, MethodInfo method)
        {
            var delegateType = typeof(Func<,,>).MakeGenericType(typeof(IValueConverter), typeof(object), type);

            var valueConverter = Expression.Parameter(typeof(IValueConverter), "c");
            var value = Expression.Parameter(typeof(object), "o");

            var lambda = Expression.Lambda(delegateType,
                                           Expression.Call(valueConverter, method, value),
                                           valueConverter, value);

            return lambda.Compile();
        }

        #endregion


        #region Конструктор OneSValue

        /// <summary>Метод <see cref="OneSValue.Create"/>.</summary>
        public static MethodInfo OneSValueCreateMethod
        {
            get { return _oneSValueCreateMethod; }
        }
        private static readonly MethodInfo _oneSValueCreateMethod = GetOneSValueCreateMethod();

        private static MethodInfo GetOneSValueCreateMethod()
        {
            Expression<Func<OneSValue>> lambda = () => OneSValue.Create(null, null);

            return ((MethodCallExpression)lambda.Body).Method;
        }

        #endregion

        #region Методы OneSSqlFunctions

        /// <summary>
        /// Методы <see cref="OneSSqlFunctions"/>
        /// </summary>
        public enum SqlFunction
        {
            In,
            InHierarchy,
            ToBoolean,
            ToInt16,
            ToInt32,
            ToInt64,
            ToSingle,
            ToDouble,
            ToDecimal,
            ToString,
            ToDateTime,
            ToDataRecord,
            GetQuarter,
            GetWeek,
            GetDayWeek,
            BeginOfPeriod,
            EndOfPeriod,
            Like,
            Between
        }

        private static readonly Dictionary<string, SqlFunction> _sqlFunctions 
            = Enum.GetValues(typeof(SqlFunction))
                  .Cast<object>()
                  .ToDictionary(o => Enum.GetName(typeof(SqlFunction), o), o => (SqlFunction)o);

        /// <summary>
        /// Является ли метод методом <see cref="OneSSqlFunctions"/>.
        /// </summary>
        public static bool IsSqlFunction(MethodInfo method, out SqlFunction sqlFunction)
        {
            if (method.DeclaringType != typeof (OneSSqlFunctions))
            {
                sqlFunction = default(SqlFunction);
                return false;
            }

            return _sqlFunctions.TryGetValue(method.Name, out sqlFunction);
        }

        #endregion

        #region Методы конвертеров

        /// <summary>
        /// Метод <see cref="DayOfWeekConverter.FromInt32"/>.
        /// </summary>
        public static MethodInfo DayOfWeekFromInt32Method
        {
            get { return _dayOfWeekFromInt32Method; }
        }
        private static readonly MethodInfo _dayOfWeekFromInt32Method = typeof(DayOfWeekConverter).GetMethod("FromInt32");

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

        /// <summary>
        /// Генерация выражения получения типизированных записей.
        /// </summary>
        public static Expression GetTypedRecordsExpression<T>()
        {
            Contract.Ensures(Contract.Result<Expression>() != null);

            return Expression.Call(GetGetTypedRecordsMethodInfo<T>());
        }

        /// <summary>
        /// Определение того является ли метод методом <see cref="OneSQueryMethods.GetTypedRecords{T}"/>.
        /// </summary>
        /// <param name="method">Проверяемый метода.</param>
        /// <param name="dataType">Тип запрашиваемых записей.</param>
        public static bool IsGetTypedRecordsMethod(MethodInfo method, out Type dataType)
        {
            Contract.Requires<ArgumentNullException>(method != null);

            const string GET_TYPED_RECORDS_METHOD_NAME = "GetTypedRecords";

            if (method.DeclaringType == typeof(OneSQueryMethods)
                && method.IsStatic
                && method.Name == GET_TYPED_RECORDS_METHOD_NAME
                && method.IsGenericMethod)
            {
                dataType = method.GetGenericArguments()[0];
                return true;
            }

            dataType = default(Type);
            return false;
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
