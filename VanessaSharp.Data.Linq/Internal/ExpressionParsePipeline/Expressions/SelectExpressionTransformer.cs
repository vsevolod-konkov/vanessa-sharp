using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Преобразователь выражения метода Select в SQL-инструкцию SELECT и в делегат для вычитки элемента данных из записи.
    /// </summary>
    internal sealed class SelectExpressionTransformer : ExpressionVisitorBase
    {
        private static readonly IDictionary<MethodInfo, MethodInfo>
            _methods = new Dictionary<MethodInfo, MethodInfo>
            {
                { OneSQueryExpressionHelper.DataRecordGetStringMethod, OneSQueryExpressionHelper.ValueConverterToStringMethod },
                { OneSQueryExpressionHelper.DataRecordGetInt32Method, OneSQueryExpressionHelper.ValueConverterToInt32Method },
                { OneSQueryExpressionHelper.DataRecordGetDoubleMethod, OneSQueryExpressionHelper.ValueConverterToDoubleMethod },
                { OneSQueryExpressionHelper.DataRecordGetDateTimeMethod, OneSQueryExpressionHelper.ValueConverterToDateTimeMethod },
                { OneSQueryExpressionHelper.DataRecordGetBooleanMethod, OneSQueryExpressionHelper.ValueConverterToBooleanMethod },
                { OneSQueryExpressionHelper.DataRecordGetCharMethod, OneSQueryExpressionHelper.ValueConverterToCharMethod },
            };

        private static readonly IDictionary<Type, MethodInfo>
            _convertMethodsByType = new Dictionary<Type, MethodInfo>
                {
                    { typeof(string), OneSQueryExpressionHelper.ValueConverterToStringMethod },
                    { typeof(char), OneSQueryExpressionHelper.ValueConverterToCharMethod },
                    { typeof(byte), OneSQueryExpressionHelper.ValueConverterToByteMethod },
                    { typeof(short), OneSQueryExpressionHelper.ValueConverterToInt16Method },
                    { typeof(int), OneSQueryExpressionHelper.ValueConverterToInt32Method },
                    { typeof(long), OneSQueryExpressionHelper.ValueConverterToInt64Method },
                    { typeof(float), OneSQueryExpressionHelper.ValueConverterToFloatMethod },
                    { typeof(double), OneSQueryExpressionHelper.ValueConverterToDoubleMethod },
                    { typeof(decimal), OneSQueryExpressionHelper.ValueConverterToDecimalMethod },
                    { typeof(bool), OneSQueryExpressionHelper.ValueConverterToBooleanMethod },
                    { typeof(DateTime), OneSQueryExpressionHelper.ValueConverterToDateTimeMethod },
                };

        /// <summary>Приватный конструктор для инициализаии параметра метода.</summary>
        /// <param name="recordExpression">Параметр метода - выражение записи из которой производиться выборка.</param>
        /// <param name="typeMapping">Соответствие трансформируемого типа источнику данных.</param>
        private SelectExpressionTransformer(ParameterExpression recordExpression, OneSTypeMapping typeMapping)
        {
            Contract.Requires<ArgumentNullException>(recordExpression != null);

            _recordExpression = recordExpression;
            _typeMapping = typeMapping;
        }

        /// <summary>Преобразование LINQ-выражения метода Select.</summary>
        /// <typeparam name="TInput">Тип элементов исходной последовательности.</typeparam>
        /// <typeparam name="TOutput">Тип элементов выходной последовательности - результатов выборки.</typeparam>
        /// <param name="mappingProvider">Поставщик соответствий типов источникам данных 1С.</param>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="expression">Преобразуемое выражение.</param>
        public static SelectionPartParseProduct<TOutput> Transform<TInput, TOutput>(IOneSMappingProvider mappingProvider, QueryParseContext context, Expression<Func<TInput, TOutput>> expression)
        {
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Ensures(Contract.Result<SelectionPartParseProduct<TOutput>>() != null);

            var typeMapping = (typeof (TInput) == typeof(OneSDataRecord))
                                  ? null
                                  : mappingProvider.GetTypeMapping(typeof(TInput));

            return new SelectExpressionTransformer(
                expression.Parameters[0], 
                typeMapping
                )
                .TransformLambdaBody<TOutput>(expression.Body);
        }

        /// <summary>Преобразование тела лямбды метода выборки данных.</summary>
        /// <typeparam name="T">Тип элементов выборки.</typeparam>
        /// <param name="lambdaBody">Тело лямбды метода выборки.</param>
        /// <returns>
        /// Результат преобразования - набор колонок и делегат создания элемента из значений колонок.
        /// </returns>
        private SelectionPartParseProduct<T> TransformLambdaBody<T>(Expression lambdaBody)
        {
            var resultExpression = Visit(lambdaBody);

            return new SelectionPartParseProduct<T>(
                new ReadOnlyCollection<SqlExpression>(_columns),
                CreateItemReader<T>(resultExpression));
        }

        /// <summary>Выражение соответствующая записи данных из которой делается выборка.</summary>
        private readonly ParameterExpression _recordExpression;

        /// <summary>
        /// Соответствие трансформируемого типа источнику данных.
        /// </summary>
        private readonly OneSTypeMapping _typeMapping;

        /// <summary>Выражения колонок.</summary>
        private readonly List<SqlExpression> _columns = new List<SqlExpression>();

        /// <summary>Имена колонок полей.</summary>
        /// <remarks>Для быстрого поиска.</remarks>
        private readonly List<string> _fieldNames = new List<string>(); 

        /// <summary>Параметр для результирующего делегата создания элемента - конвертер значений.</summary>
        private readonly ParameterExpression _converterParameter 
            = Expression.Parameter(typeof(IValueConverter), "valueConverter");

        /// <summary>Параметр для результирующего делегата создания элемента - массив вычитанных значений.</summary>
        private readonly ParameterExpression _valuesParameter
            = Expression.Parameter(typeof(object[]), "values");

        /// <summary>
        /// Создание делегата создателя элемента вычитываемого из записи.
        /// </summary>
        /// <typeparam name="T">Тип элемента.</typeparam>
        /// <param name="body">Тело лямбда-выражения создаваемого делегата.</param>
        private Func<IValueConverter, object[], T> CreateItemReader<T>(Expression body)
        {
            return Expression
                .Lambda<Func<IValueConverter, object[], T>>(body, _converterParameter, _valuesParameter)
                .Compile();
        }

        /// <summary>Получение индекса поля по имени.</summary>
        /// <param name="fieldName">Имя поля.</param>
        private int GetFieldIndex(string fieldName)
        {
            var index = _fieldNames.IndexOf(fieldName);
            if (index == -1)
                return AddNewField(fieldName);

            return index;
        }

        /// <summary>Добавление вычитки нового поля.</summary>
        private int AddNewField(string fieldName)
        {
            _columns.Add(new SqlFieldExpression(fieldName));
            _fieldNames.Add(fieldName);

            return _fieldNames.Count - 1;
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MethodCallExpression"/>.
        /// </summary>
        /// <remarks>
        /// Заменяет методы получения значений из записи <see cref="OneSDataRecord"/>
        /// на получение значений из массива <see cref="_valuesParameter"/> вычитанных значений.
        /// </remarks>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Object == _recordExpression)
            {
                MethodInfo valueConverterMethod;
                if (_methods.TryGetValue(node.Method, out valueConverterMethod))
                {
                    var fieldName = GetConstant<string>(node.Arguments[0]);
                    return GetFieldAccessExpression(fieldName, valueConverterMethod);
                }

                throw CreateExpressionNotSupportedException(node);
            }
            
            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// Получение выражения получения значения поля записи.
        /// </summary>
        /// <param name="fieldName">Имя поля получаемого значения.</param>
        /// <param name="valueConverterMethod">Метод конвертации к нужному типу.</param>
        private Expression GetFieldAccessExpression(string fieldName, MethodInfo valueConverterMethod)
        {
            var index = GetFieldIndex(fieldName);

            var valueExpression = Expression.ArrayIndex(_valuesParameter, Expression.Constant(index));
            return Expression.Call(_converterParameter, valueConverterMethod, valueExpression);
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MemberExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (_typeMapping != null)
            {
                var fieldName = _typeMapping.GetFieldNameByMemberInfo(node.Member);
                if (fieldName != null)
                {
                    MethodInfo valueConverterMethod;
                    if (!_convertMethodsByType.TryGetValue(node.Type, out valueConverterMethod))
                    {
                        throw new InvalidOperationException(string.Format(
                            "Нельзя привести значение поля к типу \"{0}\". Для доступа к полю поддерживаются только следующие типы \"{1}\".",
                            node.Type,
                            string.Join(", ", _convertMethodsByType.Keys)));
                    }

                    return GetFieldAccessExpression(fieldName, valueConverterMethod);
                }
            }

            return base.VisitMember(node);
        }

        /// <summary>
        /// Просматривает выражение <see cref="T:System.Linq.Expressions.ParameterExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            throw new InvalidOperationException(
                "Недопустимо использовать запись данных в качестве члена в выходной структуре. Можно использовать в выражении запись только для доступа к ее полям.");
        }
    }
}
