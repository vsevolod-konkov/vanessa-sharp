﻿using System;
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
                OneSQueryExpressionHelper.DataRecordGetGuidMethod,
                OneSQueryExpressionHelper.DataRecordGetValueMethod,
                
            };
            

        /// <summary>
        /// Параметр выражения - запись данных.
        /// </summary>
        private readonly ParameterExpression _recordExpression;

        /// <summary>
        /// Поставщик карт соответствия типов CLR структурам 1С.
        /// </summary>
        private readonly TypeMappingProvider _mappingProvider;

        /// <summary>
        /// Стековая машина.
        /// </summary>
        private readonly StackEngine _stackEngine;

        public bool IsTablePart { get; private set; }

        public void ClearIsTablePart()
        {
            IsTablePart = false;
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="recordExpression">Выражение записи.</param>
        /// <param name="mappingProvider">Поставщик соответствий типам CLR источников данных 1С.</param>
        /// <param name="level">Уровень данных</param>
        public SqlObjectBuilder(
            QueryParseContext context,
            ParameterExpression recordExpression,
            IOneSMappingProvider mappingProvider,
            OneSDataLevel level)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(recordExpression != null);
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            
            _stackEngine = new StackEngine(context);
            _recordExpression = recordExpression;
            _mappingProvider = TypeMappingProvider.Create(mappingProvider, level);
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
                var fieldName = _stackEngine.PopValue<string>();
                _stackEngine.Field(fieldName);
                
                return true;
            }

            if (node.Method == OneSQueryExpressionHelper.DataRecordGetTablePartRecords)
            {
                var fieldName = _stackEngine.PopValue<string>();
                _stackEngine.Field(fieldName);
                IsTablePart = true;

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
                    case OneSQueryExpressionHelper.SqlFunction.In:
                        _stackEngine.InValuesListCondition();
                        return true;
                    case OneSQueryExpressionHelper.SqlFunction.InHierarchy:
                        _stackEngine.InValuesListCondition(true);
                        return true;
                    
                    case OneSQueryExpressionHelper.SqlFunction.ToInt16:
                    case OneSQueryExpressionHelper.SqlFunction.ToInt32:
                    case OneSQueryExpressionHelper.SqlFunction.ToInt64:
                        length = _stackEngine.PopValue<int?>();
                        _stackEngine.Cast(SqlTypeDescription.Number(length));
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.ToSingle:
                    case OneSQueryExpressionHelper.SqlFunction.ToDouble:
                    case OneSQueryExpressionHelper.SqlFunction.ToDecimal:
                        var precision = _stackEngine.PopValue<int?>();
                        length = _stackEngine.PopValue<int?>();
                        _stackEngine.Cast(SqlTypeDescription.Number(length, precision));
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.ToString:
                        length = _stackEngine.PopValue<int?>();
                        _stackEngine.Cast(SqlTypeDescription.String(length));
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.ToDataRecord:
                        var tableName = _stackEngine.PopValue<string>();
                        _stackEngine.Cast(SqlTypeDescription.Table(tableName));
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.ToBoolean:
                        _stackEngine.Cast(SqlTypeDescription.Boolean);
                        return true;
                    case OneSQueryExpressionHelper.SqlFunction.ToDateTime:
                        _stackEngine.Cast(SqlTypeDescription.Date);
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.GetQuarter:
                        _stackEngine.CallQuarter();
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.GetWeek:
                        _stackEngine.CallWeek();
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.GetDayWeek:
                        _stackEngine.CallDayWeek();
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.BeginOfPeriod:
                        _stackEngine.CallBeginOfPeriod();
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.EndOfPeriod:
                        _stackEngine.CallEndOfPeriod();
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.Like:
                        _stackEngine.Like();
                        return true;

                    case OneSQueryExpressionHelper.SqlFunction.Between:
                        _stackEngine.Between();
                        return true;
                }
            }

            if (node.Method.DeclaringType == typeof(string) && node.Method.Name == "Substring" &&
                node.Arguments.Count == 2)
            {
                _stackEngine.CallSubstring();
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Обработка выражения получения члена объекта.
        /// </summary>
        public bool HandleMember(MemberExpression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);

            if (node.Member.DeclaringType == typeof(DateTime))
            {
                switch (node.Member.Name)
                {
                    case "Year":
                        _stackEngine.CallYear();
                        return true;
                    
                    case "Month":
                        _stackEngine.CallMonth();
                        return true;

                    case "DayOfYear":
                        _stackEngine.CallDayOfYear();
                        return true;

                    case "Day":
                        _stackEngine.CallDay();
                        return true;

                    case "DayOfWeek":
                        _stackEngine.CallDayWeek();
                        return true;

                    case "Hour":
                        _stackEngine.CallHour();
                        return true;

                    case "Minute":
                        _stackEngine.CallMinute();
                        return true;

                    case "Second":
                        _stackEngine.CallSecond();
                        return true;
                }
            }

            if (_mappingProvider.IsDataType(node.Expression.Type))
            {
                var fieldMapping = _mappingProvider
                    .GetFieldMapping(node.Expression.Type, node.Member);
                    
                if (fieldMapping != null)
                {
                    _stackEngine.Field(fieldMapping.FieldName);
                    IsTablePart = (fieldMapping.DataColumnKind == OneSDataColumnKind.TablePart);

                    return true;
                }
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

            if (node.NodeType == ExpressionType.Coalesce)
            {
                _stackEngine.Coalesce();
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
        /// Обрабатывает выражение тернарного оператора выбора.
        /// </summary>
        public bool HandleConditional(ConditionalExpression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);

            _stackEngine.Conditional();

            return true;
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

            if (_mappingProvider.IsTypeTableSource(type))
            {
                var sourceName = _mappingProvider.GetTableSourceName(type);
                result = SqlTypeDescription.Table(sourceName);
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

            if (node.NodeType == ExpressionType.TypeIs && _mappingProvider.IsTypeTableSource(node.TypeOperand))
            {
                var dataSourceName = _mappingProvider
                    .GetTableSourceName(node.TypeOperand);

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

        /// <summary>
        /// Преобразование условия в выражение.
        /// </summary>
        private static SqlExpression ConvertConditionToExpression(SqlCondition condition)
        {
            Contract.Requires<ArgumentNullException>(condition != null);

            return new SqlCaseExpression(condition,
                    SqlLiteralExpression.Create(true),
                    SqlLiteralExpression.Create(false));
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

        private sealed class SqlExpressionFromConditionProvider : ISqlExpressionProvider
        {
            private readonly SqlCondition _condition;

            public SqlExpressionFromConditionProvider(SqlCondition condition)
            {
                Contract.Requires<ArgumentNullException>(condition != null);
                
                _condition = condition;
            }

            public SqlExpression GetExpression()
            {
                return ConvertConditionToExpression(_condition);
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
                        var provider = CreateExpressionProvider(o);

                        return provider == null ? null : provider.GetExpression();
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
            /// Вызов функции подстроки.
            /// </summary>
            public void CallSubstring()
            {
                var length = PopExpression();
                var position = PopExpression();
                var str = PopExpression();

                var function = SqlEmbeddedFunctionExpression.Substring(str, position, length);

                _stack.Push(function);
            }

            private void CallDateFunction(Func<SqlExpression, SqlEmbeddedFunctionExpression> creator)
            {
                var date = PopExpression();
                var function = creator(date);

                _stack.Push(function);
            }

            /// <summary>
            /// Вызов функции получения года.
            /// </summary>
            public void CallYear()
            {
                CallDateFunction(SqlEmbeddedFunctionExpression.Year);
            }

            /// <summary>
            /// Вызов функции получения квартала.
            /// </summary>
            public void CallQuarter()
            {
                CallDateFunction(SqlEmbeddedFunctionExpression.Quarter);
            }

            /// <summary>
            /// Вызов функции получения месяца.
            /// </summary>
            public void CallMonth()
            {
                CallDateFunction(SqlEmbeddedFunctionExpression.Month);
            }

            /// <summary>
            /// Вызов функции получения дня года.
            /// </summary>
            public void CallDayOfYear()
            {
                CallDateFunction(SqlEmbeddedFunctionExpression.DayOfYear);
            }

            /// <summary>
            /// Вызов функции получения дня месяца.
            /// </summary>
            public void CallDay()
            {
                CallDateFunction(SqlEmbeddedFunctionExpression.Day);
            }

            /// <summary>
            /// Вызов функции получения недели года даты.
            /// </summary>
            public void CallWeek()
            {
                CallDateFunction(SqlEmbeddedFunctionExpression.Week);
            }

            /// <summary>
            /// Вызов функции получения дня недели даты.
            /// </summary>
            public void CallDayWeek()
            {
                CallDateFunction(SqlEmbeddedFunctionExpression.DayWeek);
            }

            /// <summary>
            /// Вызов функции получения часа даты.
            /// </summary>
            public void CallHour()
            {
                CallDateFunction(SqlEmbeddedFunctionExpression.Hour);
            }

            /// <summary>
            /// Вызов функции получения минуты даты.
            /// </summary>
            public void CallMinute()
            {
                CallDateFunction(SqlEmbeddedFunctionExpression.Minute);
            }

            /// <summary>
            /// Вызов функции получения секунды даты.
            /// </summary>
            public void CallSecond()
            {
                CallDateFunction(SqlEmbeddedFunctionExpression.Second);
            }

            private void CallDefineDatePeriodFunction(
                Func<SqlExpression, OneSTimePeriodKind, SqlEmbeddedFunctionExpression> creator)
            {
                var kind = PopValue<OneSTimePeriodKind>();
                var date = PopExpression();
                var function = creator(date, kind);

                _stack.Push(function);
            }

            /// <summary>
            /// Вызов функции получения начальной даты периода.
            /// </summary>
            public void CallBeginOfPeriod()
            {
                CallDefineDatePeriodFunction(SqlEmbeddedFunctionExpression.BeginOfPeriod);
            }

            /// <summary>
            /// Вызов функции получения конечной даты периода.
            /// </summary>
            public void CallEndOfPeriod()
            {
                CallDefineDatePeriodFunction(SqlEmbeddedFunctionExpression.EndOfPeriod);
            }

            /// <summary>
            /// Создание условие проверки соответствия строки шаблону.
            /// </summary>
            public void Like()
            {
                var escapeSymbol = PopValue<char?>();
                var pattern = PopValue<string>();
                var operand = PopExpression();

                var likeCondition = new SqlLikeCondition(operand, true, pattern, escapeSymbol);
                _stack.Push(likeCondition);
            }

            /// <summary>
            /// Создание условия проверки вхождения в диапазон.
            /// </summary>
            public void Between()
            {
                var end = PopExpression();
                var start = PopExpression();
                var operand = PopExpression();

                var betweenCondition = new SqlBetweenCondition(operand, true, start, end);
                _stack.Push(betweenCondition);
            }

            public void Coalesce()
            {
                var notNullValue = PopExpression();
                var value = PopExpression();

                var caseExpression = new SqlCaseExpression(
                    new SqlIsNullCondition(value, true),
                    notNullValue,
                    value);

                _stack.Push(caseExpression);
            }

            public void Conditional()
            {
                var ifFalse = PopExpression();
                var ifTrue = PopExpression();
                var test = Pop<SqlCondition>();

                var caseExpression = new SqlCaseExpression(test, ifTrue, ifFalse);
                _stack.Push(caseExpression);
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
            public ISqlExpressionProvider PeekExpression()
            {
                if (_stack.Count == 0)
                {
                    throw new InvalidOperationException(string.Format(
                        "В стеке ожидался объект типа \"{0}\", но стек оказался пуст.", typeof(SqlExpression)));
                }

                var obj = _stack.Peek();

                var result = CreateExpressionProvider(obj);

                if (result == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "В стеке ожидался объект типа \"{0}\", но оказался объект \"{1}\".", typeof (SqlExpression), obj));
                }

                return result;
            }

            private ISqlExpressionProvider CreateExpressionProvider(object obj)
            {
                var expression = obj as SqlExpression;
                if (expression != null)
                    return new SqlExpressionHolder(expression);

                var valueHolder = obj as ValueHolder;
                if (valueHolder != null)
                    return new SqlExpressionFromValueProvider(_context, valueHolder);

                var condition = obj as SqlCondition;
                if (condition != null)
                    return new SqlExpressionFromConditionProvider(condition);

                return null;
            }

            /// <summary>Получение значения заданного типа.</summary>
            public T PopValue<T>()
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

                if (value is DayOfWeek)
                    return SqlLiteralExpression.Create(DayOfWeekConverter.ToInt32((DayOfWeek)value));

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

        /// <summary>
        /// Поставщик карт соответствия типов CLR на данные 1С с учетом уровня данных.
        /// </summary>
        private abstract class TypeMappingProvider
        {
            /// <summary>
            /// Исходный поставщик карт соответствия.
            /// </summary>
            protected readonly IOneSMappingProvider _nakedProvider;

            protected TypeMappingProvider(IOneSMappingProvider nakedProvider)
            {
                Contract.Requires<ArgumentNullException>(nakedProvider != null);

                _nakedProvider = nakedProvider;
            }

            /// <summary>Создание экземпляра.</summary>
            /// <param name="nakedProvider">Исходный поставщик карт соответствия.</param>
            /// <param name="level">Уровень данных.</param>
            public static TypeMappingProvider Create(IOneSMappingProvider nakedProvider, OneSDataLevel level)
            {
                Contract.Requires<ArgumentNullException>(nakedProvider != null);
                Contract.Ensures(Contract.Result<TypeMappingProvider>() != null);

                return (level == OneSDataLevel.Root)
                           ? (TypeMappingProvider)new RootTypeMappingProvider(nakedProvider)
                           : new TablePartTypeMappingProvider(nakedProvider);
            }

            /// <summary>Уровень данных.</summary>
            protected abstract OneSDataLevel Level { get; }

            /// <summary>Получение карт соответствия полям источника данных 1С.</summary>
            /// <param name="type">Тип для которого надо получить карту соответствия.</param>
            protected abstract IEnumerable<OneSFieldMapping> GetFieldMappings(Type type);

            /// <summary>Является ли уровнем соответствующего уровня.</summary>
            /// <param name="type">Тип.</param>
            public bool IsDataType(Type type)
            {
                return _nakedProvider.IsDataType(Level, type);
            }

            /// <summary>Получение карты соответствия поля 1С.</summary>
            public OneSFieldMapping GetFieldMapping(Type type, MemberInfo member)
            {
                return GetFieldMappings(type).SingleOrDefault(m => m.MemberInfo.MetadataToken == member.MetadataToken);
            }

            /// <summary>Является ли тип табличным источником 1С.</summary>
            public bool IsTypeTableSource(Type type)
            {
                if (_nakedProvider.IsDataType(OneSDataLevel.Root, type))
                    return true;

                if (_nakedProvider.IsDataType(OneSDataLevel.TablePart, type))
                {
                    var mapping = _nakedProvider.GetTablePartTypeMappings(type);
                    if (mapping.OwnerType != null)
                        return _nakedProvider.IsDataType(OneSDataLevel.Root, mapping.OwnerType);
                }

                return false;
            }

            /// <summary>Получение имени табличного источника данных 1С.</summary>
            public string GetTableSourceName(Type type)
            {
                if (_nakedProvider.IsDataType(OneSDataLevel.Root, type))
                    return _nakedProvider.GetRootTypeMapping(type).SourceName;

                var mapping = _nakedProvider.GetTablePartTypeMappings(type);
                Contract.Assert(mapping.OwnerType != null);

                return _nakedProvider.GetRootTypeMapping(mapping.OwnerType).SourceName;
            }

            private sealed class RootTypeMappingProvider : TypeMappingProvider
            {
                public RootTypeMappingProvider(IOneSMappingProvider nakedProvider)
                    : base(nakedProvider)
                {}

                protected override OneSDataLevel Level
                {
                    get { return OneSDataLevel.Root; }
                }

                protected override IEnumerable<OneSFieldMapping> GetFieldMappings(Type type)
                {
                    return _nakedProvider.GetRootTypeMapping(type).FieldMappings;
                }
            }

            private sealed class TablePartTypeMappingProvider : TypeMappingProvider
            {
                public TablePartTypeMappingProvider(IOneSMappingProvider nakedProvider)
                    : base(nakedProvider)
                { }

                protected override OneSDataLevel Level
                {
                    get { return OneSDataLevel.TablePart; }
                }

                protected override IEnumerable<OneSFieldMapping> GetFieldMappings(Type type)
                {
                    return _nakedProvider.GetTablePartTypeMappings(type).FieldMappings;
                }
            }
        }

        #endregion
    }
}
