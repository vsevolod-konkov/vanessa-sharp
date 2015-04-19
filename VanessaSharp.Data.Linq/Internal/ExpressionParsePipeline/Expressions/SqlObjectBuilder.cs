using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Построитель SQL-выражение\условия.
    /// </summary>
    internal sealed class SqlObjectBuilder
    {
        /// <summary>Набор методов доступа к полям записи.</summary>
        private static readonly ISet<MethodInfo>
            _getValueMethods = new HashSet<MethodInfo>
            {
                OneSQueryExpressionHelper.DataRecordGetCharMethod,
                OneSQueryExpressionHelper.DataRecordGetStringMethod,
                OneSQueryExpressionHelper.DataRecordGetByteMethod,
                OneSQueryExpressionHelper.DataRecordGetInt16Method,
                OneSQueryExpressionHelper.DataRecordGetInt32Method,
                OneSQueryExpressionHelper.DataRecordGetInt64Method,
                OneSQueryExpressionHelper.DataRecordGetFloatMethod,
                OneSQueryExpressionHelper.DataRecordGetDoubleMethod,
                OneSQueryExpressionHelper.DataRecordGetDecimalMethod,
                OneSQueryExpressionHelper.DataRecordGetDateTimeMethod,
                OneSQueryExpressionHelper.DataRecordGetBooleanMethod,
                OneSQueryExpressionHelper.DataRecordGetValueMethod
            };
            

        /// <summary>
        /// Параметр выражения - запись данных.
        /// </summary>
        private readonly ParameterExpression _recordExpression;

        /// <summary>
        /// Поставщик карт соответствия типов CLR структурам 1С.
        /// </summary>
        private readonly IOneSMappingProvider _mappingProvider;

        /// <summary>
        /// Стековая машина.
        /// </summary>
        private readonly StackEngine _stackEngine;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="recordExpression">Выражение записи.</param>
        /// <param name="mappingProvider">Поставщик соответствий типам CLR источников данных 1С.</param>
        public SqlObjectBuilder(
            QueryParseContext context,
            ParameterExpression recordExpression,
            IOneSMappingProvider mappingProvider)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(recordExpression != null);
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            
            _stackEngine = new StackEngine(context);
            _recordExpression = recordExpression;
            _mappingProvider = mappingProvider;
        }

        /// <summary>
        /// Получения выражения.
        /// </summary>
        public SqlExpression GetExpression()
        {
            Contract.Ensures(Contract.Result<SqlExpression>() != null);
            
            return _stackEngine.GetExpression();
        }

        /// <summary>Получение условия.</summary>
        public SqlCondition GetCondition()
        {
            Contract.Ensures(Contract.Result<SqlCondition>() != null);
            
            return _stackEngine.GetCondition();
        }

        /// <summary>Просмотр выражения.</summary>
        public ISqlExpressionProvider PeekExpression()
        {
            return _stackEngine.PeekExpression();
        }

        /// <summary>Очистка состояния построителя.</summary>
        public void Clear()
        {
            _stackEngine.Clear();
        }
        
        /// <summary>
        /// Обработка выражения вызова метода после посещения дочерних узлов.
        /// </summary>
        public bool HandleMethodCall(MethodCallExpression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);

            if (_getValueMethods.Contains(node.Method))
            {
                var fieldName = _stackEngine.GetValue<string>();
                _stackEngine.Field(fieldName);
                
                return true;
            }

            if (OneSQueryExpressionHelper.IsEnumerableContainsMethod(node.Method))
            {
                _stackEngine.Swap();
                _stackEngine.InValuesListCondition();
                return true;
            }

            OneSQueryExpressionHelper.SqlFunction sqlFunction;
            if (OneSQueryExpressionHelper.IsSqlFunction(node.Method, out sqlFunction))
            {
                int? length;
                
                switch (sqlFunction)
                {
                    case OneSQueryExpressionHelper.SqlFunction.ToInt16:
                    case OneSQueryExpressionHelper.SqlFunction.ToInt32:
                    case OneSQueryExpressionHelper.SqlFunction.ToInt64:
                        length = _stackEngine.GetValue<int?>();
                        _stackEngine.Cast(SqlTypeDescription.Number(length));
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.ToSingle:
                    case OneSQueryExpressionHelper.SqlFunction.ToDouble:
                    case OneSQueryExpressionHelper.SqlFunction.ToDecimal:
                        var precision = _stackEngine.GetValue<int?>();
                        length = _stackEngine.GetValue<int?>();
                        _stackEngine.Cast(SqlTypeDescription.Number(length, precision));
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.ToString:
                        length = _stackEngine.GetValue<int?>();
                        _stackEngine.Cast(SqlTypeDescription.String(length));
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.ToDataRecord:
                        var tableName = _stackEngine.GetValue<string>();
                        _stackEngine.Cast(SqlTypeDescription.Table(tableName));
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.In:
                        _stackEngine.InValuesListCondition();
                        return true;
                    case OneSQueryExpressionHelper.SqlFunction.InHierarchy:
                        _stackEngine.InValuesListCondition(true);
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.ToBoolean:
                        _stackEngine.Cast(SqlTypeDescription.Boolean);
                        return true;
                    case OneSQueryExpressionHelper.SqlFunction.ToDateTime:
                        _stackEngine.Cast(SqlTypeDescription.Date);
                        return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Обработка выражения получения члена объекта.
        /// </summary>
        public bool HandleMember(MemberExpression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);

            var fieldName = _mappingProvider
                .GetTypeMapping(node.Expression.Type)
                .GetFieldNameByMemberInfo(node.Member);

            if (fieldName != null)
            {
                _stackEngine.Field(fieldName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Обработка константного выражения.
        /// </summary>
        public bool HandleConstant(ConstantExpression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);

            _stackEngine.Value(node.Value);

            return true;
        }

        /// <summary>
        /// Обрабатывает выражение <see cref="T:System.Linq.Expressions.BinaryExpression"/>.
        /// </summary>
        public bool HandleBinary(BinaryExpression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);

            if (node.Type == typeof(bool))
            {
                if (node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual)
                {
                    var values = _stackEngine.Peek2();

                    var leftIsNull = IsNullValue(values.Item2);
                    var rightIsNull = IsNullValue(values.Item1);

                    if (leftIsNull || rightIsNull)
                    {
                        Contract.Assert(!(leftIsNull && rightIsNull),
                                        "Нарушение условия предвычислений. Оказалось null op null");

                        _stackEngine.TestIsNull(node.NodeType == ExpressionType.Equal);

                        return true;
                    }
                }

                var logicOperationType = GetSqlBinaryOperationType(node.NodeType);
                if (logicOperationType.HasValue)
                {
                    _stackEngine.BinaryLogicOperation(logicOperationType.Value);
                    return true;
                }
            }

            var relationType = GetSqlBinaryRelationType(node.NodeType);
            if (relationType.HasValue)
            {
                _stackEngine.BinaryRelation(relationType.Value);
                return true;
            }

            var arithmeticOperationType = GetSqlBinaryArithmeticOperationType(node.NodeType);
            if (arithmeticOperationType.HasValue)
            {
                _stackEngine.BinaryArithmeticOperation(arithmeticOperationType.Value);
                return true;
            }

            return false;
        }

        /// <summary>Является ли объект стека нулевым значением.</summary>
        /// <param name="obj">Проверяемый объект стека.</param>
        private static bool IsNullValue(object obj)
        {
            var valueHolder = obj as ValueHolder;

            return (valueHolder != null)
                   && valueHolder.IsNull;
        }

        /// <summary>
        /// Обрабатывает выражение <see cref="T:System.Linq.Expressions.UnaryExpression"/>.
        /// </summary>
        public bool HandleUnary(UnaryExpression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);

            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    _stackEngine.NotOperation();
                    return true;

                case ExpressionType.Negate:
                    _stackEngine.NegateOperation();
                    return true;

                case ExpressionType.Convert:
                    return HandleConvert(node.Type);
            }

            return false;
        }

        /// <summary>
        /// Обрабатывает выражение приведения к типу.
        /// </summary>
        /// <param name="type">Тип, к которому приводится выражение.</param>
        private bool HandleConvert(Type type)
        {
            SqlTypeDescription sqlTypeDescription;
            var result = TryGetSqlTypeDescription(type, out sqlTypeDescription);

            if (result)
                _stackEngine.Cast(sqlTypeDescription);

            return result;
        }

        /// <summary>
        /// Обрабатывает выражение параметра.
        /// </summary>
        public bool HandleParameter(ParameterExpression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);

            if (_recordExpression == node)
            {
                _stackEngine.DefaultTable();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Попытка получения описания SQL-типа по типу CLR.
        /// </summary>
        private bool TryGetSqlTypeDescription(Type type, out SqlTypeDescription result)
        {
            if (type == typeof(bool))
            {
                result = SqlTypeDescription.Boolean;
                return true;
            }

            if (type == typeof(DateTime))
            {
                result = SqlTypeDescription.Date;
                return true;
            }

            if (type == typeof(string))
            {
                result = SqlTypeDescription.String();
                return true;
            }

            if (_numberTypes.Contains(type))
            {
                result = SqlTypeDescription.Number();
                return true;
            }

            if (_mappingProvider.IsDataType(type))
            {
                var mapping = _mappingProvider.GetTypeMapping(type);
                result = SqlTypeDescription.Table(mapping.SourceName);
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Поддерживаемые числовые типы.
        /// </summary>
        private static readonly Type[] _numberTypes = new[]
            {typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)};

        /// <summary>
        /// Обрабатывает выражение <see cref="TypeBinaryExpression"/>.
        /// </summary>
        public bool HandleVisitTypeBinary(TypeBinaryExpression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);

            if (node.NodeType == ExpressionType.TypeIs)
            {
                var dataSourceName = _mappingProvider
                    .GetTypeMapping(node.TypeOperand)
                    .SourceName;

                _stackEngine.Refs(dataSourceName);
                return true;
            }

            return false;
        }

        /// <summary>Получение типа бинарного отношения в зависимости от типа узла выражения.</summary>
        private static SqlBinaryRelationType? GetSqlBinaryRelationType(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Equal:
                    return SqlBinaryRelationType.Equal;
                case ExpressionType.NotEqual:
                    return SqlBinaryRelationType.NotEqual;
                case ExpressionType.GreaterThan:
                    return SqlBinaryRelationType.Greater;
                case ExpressionType.GreaterThanOrEqual:
                    return SqlBinaryRelationType.GreaterOrEqual;
                case ExpressionType.LessThan:
                    return SqlBinaryRelationType.Less;
                case ExpressionType.LessThanOrEqual:
                    return SqlBinaryRelationType.LessOrEqual;
                default:
                    return null;
            }
        }

        /// <summary>Получение типа бинарной логической операции в зависимости от типа узла выражения.</summary>
        private static SqlBinaryLogicOperationType? GetSqlBinaryOperationType(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    return SqlBinaryLogicOperationType.And;

                case ExpressionType.OrElse:
                case ExpressionType.Or:
                    return SqlBinaryLogicOperationType.Or;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Получение типа бинарной арифметической операции в зависимости от типа узла выражения.
        /// </summary>
        private static SqlBinaryArithmeticOperationType? GetSqlBinaryArithmeticOperationType(
            ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Add:
                    return SqlBinaryArithmeticOperationType.Add;
                case ExpressionType.Subtract:
                    return SqlBinaryArithmeticOperationType.Subtract;
                case ExpressionType.Multiply:
                    return SqlBinaryArithmeticOperationType.Multiply;
                case ExpressionType.Divide:
                    return SqlBinaryArithmeticOperationType.Divide;
                default:
                    return null;
            }
        }

        #region Вспомогательные типы

        /// <summary>
        /// Интерфейс поставщика выражения SQL.
        /// </summary>
        public interface ISqlExpressionProvider
        {
            /// <summary>
            /// Получение выражение.
            /// </summary>
            /// <remarks>
            /// Вызов метода может изменить контекст запроса.
            /// </remarks>
            SqlExpression GetExpression();
        }

        /// <summary>
        /// Реализация <see cref="ISqlExpressionProvider"/>
        /// хранящая готовое выражение.
        /// </summary>
        private sealed class SqlExpressionHolder : ISqlExpressionProvider
        {
            private readonly SqlExpression _expression;

            public SqlExpressionHolder(SqlExpression expression)
            {
                Contract.Requires<ArgumentNullException>(expression != null);
                
                _expression = expression;
            }

            public SqlExpression GetExpression()
            {
                return _expression;
            }
        }

        /// <summary>
        /// Реализация <see cref="ISqlExpressionProvider"/>
        /// создающее выражение из хранимого значения.
        /// </summary>
        private sealed class SqlExpressionFromValueProvider : ISqlExpressionProvider
        {
            private readonly QueryParseContext _context;
            private readonly ValueHolder _valueHolder;

            public SqlExpressionFromValueProvider(QueryParseContext context, ValueHolder valueHolder)
            {
                Contract.Requires<ArgumentNullException>(context != null);
                Contract.Requires<ArgumentNullException>(valueHolder != null);
                
                _context = context;
                _valueHolder = valueHolder;
            }

            public SqlExpression GetExpression()
            {
                return _valueHolder.GetExpression(_context);
            }
        }

        /// <summary>
        /// Стековая машина для разбора выражений.
        /// </summary>
        private sealed class StackEngine
        {
            /// <summary>Стек выражений.</summary>
            private readonly Stack<object> _stack = new Stack<object>();

            /// <summary>
            /// Контекст разбора запроса.
            /// </summary>
            private readonly QueryParseContext _context;

            /// <summary>Конструктор.</summary>
            /// <param name="context">Контекст разбора запроса.</param>
            public StackEngine(QueryParseContext context)
            {
                Contract.Requires<ArgumentNullException>(context != null);

                _context = context;
            }

            /// <summary>
            /// Вытягивание объекта из стека с кастингом.
            /// </summary>
            private T Pop<T>(Func<object, T> castingAction)
                where T : class 
            {
                if (_stack.Count == 0)
                {
                    throw new InvalidOperationException(string.Format(
                        "В стеке ожидался объект типа \"{0}\", но стек оказался пуст.", typeof(T)));
                }

                var obj = _stack.Pop();

                var result = castingAction(obj);

                if (result == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "В стеке ожидался объект типа \"{0}\", но оказался объект \"{1}\".", typeof(T), obj));
                }

                return result;
            }


            /// <summary>Вытягивание типизированного объекта из стека.</summary>
            /// <typeparam name="T">Тип объекта.</typeparam>
            private T Pop<T>()
                where T : class
            {
                return Pop(o => o as T);
            }

            /// <summary>Вытягивание <see cref="SqlExpression"/> из стека.</summary>
            private SqlExpression PopExpression()
            {
                return Pop(o =>
                    {
                        var expression = o as SqlExpression;

                        if (expression != null)
                            return expression;

                        var valueHolder = o as ValueHolder;

                        if (valueHolder != null)
                            return valueHolder.GetExpression(_context);

                        return null;
                    });
            }

            /// <summary>Вытягивание списка <see cref="SqlExpression"/> из стека.</summary>
            private IList<SqlExpression> PopValueList()
            {
                return Pop(o =>
                    {
                        var valueHolder = o as ValueHolder;

                        if (valueHolder != null && valueHolder.IsList)
                            return valueHolder.GetExpressionList(_context);

                        return null;
                    });
            }

            /// <summary>
            /// Перемена местами первых двух элементов стека.
            /// </summary>
            public void Swap()
            {
                var first = _stack.Pop();
                var second = _stack.Pop();

                _stack.Push(first);
                _stack.Push(second);
            }

            /// <summary>
            /// Добавление значения в стек.
            /// </summary>
            public void Value(object value)
            {
                _stack.Push(new ValueHolder(value));
            }

            /// <summary>
            /// Добавление выражения таблицы по умолчанию в стек.
            /// </summary>
            public void DefaultTable()
            {
                _stack.Push(SqlDefaultTableExpression.Instance);
            }

            /// <summary>
            /// Добавление выражения доступа к полю в стек.
            /// </summary>
            /// <param name="fieldName">Имя поля.</param>
            public void Field(string fieldName)
            {
                var table = PopExpression();
                
                _stack.Push(
                    new SqlFieldExpression(table, fieldName));
            }

            /// <summary>Вытягивание двух выражений из стека и положение в стек созданного условия бинарного отношения.</summary>
            /// <param name="relationType">Тип бинарного отношения.</param>
            public void BinaryRelation(SqlBinaryRelationType relationType)
            {
                var secondOperand = PopExpression();
                var firstOperand = PopExpression();

                var condition = new SqlBinaryRelationCondition(relationType, firstOperand, secondOperand);
                _stack.Push(condition);
            }

            /// <summary>
            /// Вытягивание двух условий из стека и положение в стек созданного условия бинарной логической операции.
            /// </summary>
            /// <param name="operationType">Тип бинарной логической операции.</param>
            public void BinaryLogicOperation(SqlBinaryLogicOperationType operationType)
            {
                var secondOperand = Pop<SqlCondition>();
                var firstOperand = Pop<SqlCondition>();

                var condition = new SqlBinaryOperationCondition(operationType, firstOperand, secondOperand);
                _stack.Push(condition);
            }

            /// <summary>
            /// Вытягивание двух выражений из стека и положение в стек созданного условия бинарной арифментической операции.
            /// </summary>
            /// <param name="operationType">Тип бинарной арифметической операции.</param>
            public void BinaryArithmeticOperation(SqlBinaryArithmeticOperationType operationType)
            {
                var secondOperand = PopExpression();
                var firstOperand = PopExpression();

                var condition = new SqlBinaryOperationExpression(operationType, firstOperand, secondOperand);
                _stack.Push(condition);
            }

            /// <summary>
            /// Вытягивание условия из стека и заталкивания в него созданного условия отрицания.
            /// </summary>
            public void NotOperation()
            {
                var operand = Pop<SqlCondition>();

                var condition = new SqlNotCondition(operand);
                _stack.Push(condition);
            }

            /// <summary>
            /// Вытягивание выражения из стека и заталкивания в него созданного условия отрицания.
            /// </summary>
            public void NegateOperation()
            {
                var operand = PopExpression();

                var condition = new SqlNegateExpression(operand);
                _stack.Push(condition);
            }

            /// <summary>
            /// Вытягивание выражения из стека и запихивание в стек условия проверки на NULL или NOT NULL.
            /// </summary>
            /// <param name="isNull">Добавление условия проверки на NULL.</param>
            public void TestIsNull(bool isNull)
            {
                SqlExpression operand;
                if (IsNullValue(_stack.Peek()))
                {
                    _stack.Pop();
                    
                    operand = PopExpression();
                }
                else
                {
                    operand = PopExpression();

                    Contract.Assert(IsNullValue(_stack.Pop()));
                }

                var condition = new SqlIsNullCondition(operand, isNull);
                _stack.Push(condition);
            }

            /// <summary>
            /// Вытягивает выражение из стека и запихивает условие проверки ссылки на источник данных.
            /// </summary>
            /// <param name="dataSourceName">Имя источника данных, проверка на ссылку которого создается.</param>
            public void Refs(string dataSourceName)
            {
                var operand = PopExpression();
                var condition = new SqlRefsCondition(operand, dataSourceName);
                _stack.Push(condition);
            }

            /// <summary>
            /// Вытягивает из стека выражение и список параметров.
            /// Запихивает в стек условие IN.
            /// </summary>
            public void InValuesListCondition(bool isHierarchy = false)
            {
                var valuesList = new ReadOnlyCollection<SqlExpression>(PopValueList());
                var operand = PopExpression();

                var condition = new SqlInValuesListCondition(operand, valuesList, true, isHierarchy);
                _stack.Push(condition);
            }


            /// <summary>
            /// Вытягивание выражения из стека и запихивание выражения кастинга.
            /// </summary>
            /// <param name="sqlType">Тип к которому делается приведение.</param>
            public void Cast(SqlTypeDescription sqlType)
            {
                Contract.Requires<ArgumentNullException>(sqlType != null);

                var operand = PopExpression();

                _stack.Push(
                    new SqlCastExpression(operand, sqlType));
            }

            /// <summary>
            /// Получение выражения.
            /// </summary>
            public SqlExpression GetExpression()
            {
                var result = PopExpression();

                Contract.Assert(_stack.Count == 0, "Остались не собранные узлы SQL-модели");

                return result;
            }

            /// <summary>Получение условия.</summary>
            public SqlCondition GetCondition()
            {
                var result = Pop<SqlCondition>();

                Contract.Assert(_stack.Count == 0, "Остались не собранные узлы SQL-модели");

                return result;
            }

            /// <summary>
            /// Просмотр двух элементов стека.
            /// </summary>
            /// <returns></returns>
            public Tuple<object, object> Peek2()
            {
                return Tuple.Create(_stack.ElementAt(0), _stack.ElementAt(1));
            }

            /// <summary>
            /// Считывание из стека выражения, без его удаления.
            /// </summary>
            /// <returns></returns>
            public ISqlExpressionProvider PeekExpression()
            {
                if (_stack.Count == 0)
                {
                    throw new InvalidOperationException(string.Format(
                        "В стеке ожидался объект типа \"{0}\", но стек оказался пуст.", typeof(SqlExpression)));
                }

                var obj = _stack.Peek();

                var expression = obj as SqlExpression;

                if (expression != null)
                    return new SqlExpressionHolder(expression);

                var valueHolder = obj as ValueHolder;

                if (valueHolder != null)
                    return new SqlExpressionFromValueProvider(_context, valueHolder);

                throw new InvalidOperationException(string.Format(
                        "В стеке ожидался объект типа \"{0}\", но оказался объект \"{1}\".", typeof(SqlExpression), obj));
            }

            /// <summary>Получение значения заданного типа.</summary>
            public T GetValue<T>()
            {
                var obj = _stack.Pop();

                var valueHolder = obj as ValueHolder;
                if (valueHolder != null)
                    return valueHolder.GetValue<T>();

                throw new InvalidOperationException(string.Format(
                        "В стеке ожидался объект типа \"{0}\", но оказался объект \"{1}\".", typeof(ValueHolder), obj));
            }

            /// <summary>Очистка стека.</summary>
            public void Clear()
            {
                _stack.Clear();
            }
        }

        /// <summary>
        /// Держатель значения.
        /// </summary>
        private sealed class ValueHolder
        {
            /// <summary>
            /// Значение.
            /// </summary>
            private readonly object _value;

            public ValueHolder(object value)
            {
                _value = value;
            }

            /// <summary>Является ли значение списком.</summary>
            public bool IsList
            {
                get
                {
                    var type = _value.GetType();

                    return type != typeof(string)
                           && typeof(IEnumerable).IsAssignableFrom(type);
                }
            }

            /// <summary>Получение выражения из значения.</summary>
            private static SqlExpression GetExpression(object value, QueryParseContext context)
            {
                if (value is bool)
                    return SqlLiteralExpression.Create((bool)value);

                if (value is sbyte)
                    return SqlLiteralExpression.Create((sbyte)value);

                if (value is short)
                    return SqlLiteralExpression.Create((short)value);

                if (value is int)
                    return SqlLiteralExpression.Create((int)value);

                if (value is long)
                    return SqlLiteralExpression.Create((long)value);

                if (value is byte)
                    return SqlLiteralExpression.Create((byte)value);

                if (value is ushort)
                    return SqlLiteralExpression.Create((ushort)value);

                if (value is uint)
                    return SqlLiteralExpression.Create((uint)value);

                if (value is ulong)
                    return SqlLiteralExpression.Create((ulong)value);

                if (value is float)
                    return SqlLiteralExpression.Create((float)value);

                if (value is double)
                    return SqlLiteralExpression.Create((double)value);

                if (value is decimal)
                    return SqlLiteralExpression.Create((decimal)value);

                if (value is string)
                    return SqlLiteralExpression.Create((string)value);

                if (value is DateTime)
                    return SqlLiteralExpression.Create((DateTime)value);

                var parameterName = context.Parameters.GetOrAddNewParameterName(value);

                return new SqlParameterExpression(parameterName);
            }

            /// <summary>Получение выражения из значения.</summary>
            public SqlExpression GetExpression(QueryParseContext context)
            {
                return GetExpression(_value, context);
            }

            /// <summary>
            /// Получение списка выражений из значения.
            /// </summary>
            public IList<SqlExpression> GetExpressionList(QueryParseContext context)
            {
                Contract.Assert(IsList);
                
                var expressions = new List<SqlExpression>();
                    
                foreach (var value in (IEnumerable)_value)
                    expressions.Add(GetExpression(value, context));

                return expressions;
            }

            /// <summary>Получение значение заданного типа.</summary>
            public T GetValue<T>()
            {
                try
                {
                    return (T)_value;
                }
                catch (InvalidCastException e)
                {
                    throw new InvalidOperationException(string.Format(
                        "Вместо значения \"{0}\" ожидалось значение типа \"{1}\".",
                        _value,
                        typeof(T)), e);
                }
            }

            /// <summary>Является ли значение <c>null</c>.</summary>
            public bool IsNull
            {
                get { return _value == null; }
            }
        }

        #endregion
    }
}
