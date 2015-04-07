using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Контекст разбора запроса.
        /// </summary>
        private readonly QueryParseContext _context;

        /// <summary>
        /// Распознаватель доступа к полю записи. 
        /// </summary>
        private readonly FieldAccessRecognizer _fieldAccessRecognizer;

        /// <summary>
        /// Стековая машина.
        /// </summary>
        private readonly StackEngine _stackEngine = new StackEngine();

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
            _context = context;
            _fieldAccessRecognizer = FieldAccessRecognizer.Create(recordExpression, mappingProvider);
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

        /// <summary>
        /// Обработка выражения вызова метода.
        /// </summary>
        public bool HandleMethodCall(MethodCallExpression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);
            
            string fieldName;
            
            var result = _fieldAccessRecognizer
                .HandleMethodCall(node, out fieldName);

            if (result)
                _stackEngine.Field(fieldName);

            return result;
        }

        /// <summary>
        /// Обработка выражения получения члена объекта.
        /// </summary>
        public bool HandleMember(MemberExpression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);
            
            string fieldName;

            var result = _fieldAccessRecognizer
                .HandleMember(node, out fieldName);

            if (result)
                _stackEngine.Field(fieldName);

            return result;
        }

        /// <summary>
        /// Обработка константного выражения.
        /// </summary>
        public bool HandleConstant(ConstantExpression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);
            
            if (node.Value == null)
            {
                _stackEngine.Null();
            }
            else
            {
                var parameterName = _context.Parameters.GetOrAddNewParameterName(node.Value);
                _stackEngine.Parameter(parameterName);    
            }

            return true;
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.BinaryExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        public bool HandleBinary(BinaryExpression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);

            if (node.Type == typeof(bool))
            {
                if (node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual)
                {
                    var values = _stackEngine.Peek2();

                    var leftIsNull = values.Item2 == null;
                    var rightIsNull = values.Item1 == null;

                    if (leftIsNull || rightIsNull)
                    {
                        Contract.Assert(!(leftIsNull && rightIsNull),
                                        "Нарушение условия предвычислений. Оказалось null op null");

                        _stackEngine.TestIsNull(node.NodeType == ExpressionType.Equal);

                        return true;
                    }
                }

                var operationType = GetSqlBinaryOperationType(node.NodeType);
                if (operationType.HasValue)
                {
                    _stackEngine.BinaryOperation(operationType.Value);
                    return true;
                }
            }

            var relationType = GetSqlBinaryRelationType(node.NodeType);
            if (relationType.HasValue)
            {
                _stackEngine.BinaryRelation(relationType.Value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.UnaryExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        public bool HandleUnary(UnaryExpression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);
            
            if (node.NodeType == ExpressionType.Not)
            {
                _stackEngine.NotOperation();
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

        #region Вспомогательные типы

        /// <summary>
        /// Стековая машина для разбора выражений.
        /// </summary>
        private sealed class StackEngine
        {
            /// <summary>Стек выражений.</summary>
            private readonly Stack<object> _stack = new Stack<object>();

            /// <summary>Вытягивание типизированного объекта из стека.</summary>
            /// <typeparam name="T">Тип объекта.</typeparam>
            private T Pop<T>()
                where T : class
            {
                if (_stack.Count == 0)
                {
                    throw new InvalidOperationException(string.Format(
                        "В стеке ожидался объект типа \"{0}\", но стек оказался пуст.", typeof(T)));
                }

                var obj = _stack.Pop();
                var result = obj as T;

                if (result == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "В стеке ожидался объект типа \"{0}\", но оказался объект \"{1}\".", typeof(T), obj));
                }

                return result;
            }

            /// <summary>
            /// Добавление выражения доступа к параметру запроса в стек.
            /// </summary>
            /// <param name="parameterName">Имя параметра.</param>
            public void Parameter(string parameterName)
            {
               _stack.Push(
                    new SqlParameterExpression(parameterName));
            }

            /// <summary>
            /// Добавление выражения доступа к полю в стек.
            /// </summary>
            /// <param name="fieldName">Имя поля.</param>
            public void Field(string fieldName)
            {
                _stack.Push(
                    new SqlFieldExpression(fieldName));
            }

            /// <summary>Вытягивание двух выражений из стека и положение в стек созданного условия бинарного отношения.</summary>
            /// <param name="relationType">Тип бинарного отношения.</param>
            public void BinaryRelation(SqlBinaryRelationType relationType)
            {
                var secondOperand = Pop<SqlExpression>();
                var firstOperand = Pop<SqlExpression>();

                var condition = new SqlBinaryRelationCondition(relationType, firstOperand, secondOperand);
                _stack.Push(condition);
            }

            /// <summary>
            /// Вытягивание двух условий из стека и положение в стек созданного условия бинарной логической операции.
            /// </summary>
            /// <param name="operationType">Тип бинарной логической операции.</param>
            public void BinaryOperation(SqlBinaryLogicOperationType operationType)
            {
                var secondOperand = Pop<SqlCondition>();
                var firstOperand = Pop<SqlCondition>();

                var condition = new SqlBinaryOperationCondition(operationType, firstOperand, secondOperand);
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
            /// Вставка <c>NULL</c> в стек.
            /// </summary>
            public void Null()
            {
                _stack.Push(null);
            }

            /// <summary>
            /// Вытягивание выражения из стека и запихивание в стек условия проверки на NULL или NOT NULL.
            /// </summary>
            /// <param name="isNull">Добавление условия проверки на NULL.</param>
            public void TestIsNull(bool isNull)
            {
                SqlExpression operand;
                if (_stack.Peek() is SqlExpression)
                {
                    operand = Pop<SqlExpression>();

                    Contract.Assert(_stack.Pop() == null);
                }
                else
                {
                    Contract.Assert(_stack.Pop() == null);

                    operand = Pop<SqlExpression>();
                }


                var condition = new SqlIsNullCondition(operand, isNull);
                _stack.Push(condition);
            }

            /// <summary>
            /// Получение выражения.
            /// </summary>
            public SqlExpression GetExpression()
            {
                var result = Pop<SqlExpression>();

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
        }

        /// <summary>
        /// Распознаватель доступа к полям записей.
        /// </summary>
        private abstract class FieldAccessRecognizer
        {
            /// <summary>Создание стратегии.</summary>
            /// <param name="recordExpression">Выражение записи.</param>
            /// <param name="mappingProvider">Поставщик соответствий типам CLR источников данных 1С.</param>
            public static FieldAccessRecognizer Create(
                ParameterExpression recordExpression, IOneSMappingProvider mappingProvider)
            {
                Contract.Requires<ArgumentNullException>(recordExpression != null);
                Contract.Requires<ArgumentNullException>(mappingProvider != null);

                return (recordExpression.Type == typeof(OneSDataRecord))
                           ? (FieldAccessRecognizer)new DataRecordFieldAccessRecognizer(recordExpression)
                           : new TypedRecordFieldAccessRecognizer(recordExpression,
                                                     mappingProvider.GetTypeMapping(recordExpression.Type));
            }
            
            /// <summary>
            /// Конструктор.
            /// </summary>
            /// <param name="recordExpression">Выражение записи.</param>
            protected FieldAccessRecognizer(ParameterExpression recordExpression)
            {
                Contract.Requires<ArgumentNullException>(recordExpression != null);

                _recordExpression = recordExpression;
            }
            
            /// <summary>Выражение записи данных.</summary>
            protected ParameterExpression RecordExpression
            {
                get { return _recordExpression; }
            }
            private readonly ParameterExpression _recordExpression;
            
            /// <summary>Обработчик вызова метода.</summary>
            /// <param name="node">Обрабатываемый узел выражения.</param>
            /// <param name="fieldName">
            /// Имя поля к которому производится доступ в выражении.
            /// Будет возвращен <c>null</c>, в случае если выражение не является доступом к полю записи.
            /// </param>
            /// <returns>
            /// Возвращает <c>true</c> в случае если узел выражает доступ к полю записи.
            /// В ином случае возвращается <c>false</c>.
            /// </returns>
            public virtual bool HandleMethodCall(MethodCallExpression node, out string fieldName)
            {
                Contract.Requires<ArgumentNullException>(node != null);

                fieldName = null;
                return false;
            }

            /// <summary>Обработчик получения члена.</summary>
            /// <param name="node">Обрабатываемый узел выражения.</param>
            /// <param name="fieldName">
            /// Имя поля к которому производится доступ в выражении.
            /// Будет возвращен <c>null</c>, в случае если выражение не является доступом к полю записи.
            /// </param>
            /// <returns>
            /// Возвращает <c>true</c> в случае если узел выражает доступ к полю записи.
            /// В ином случае возвращается <c>false</c>.
            /// </returns>
            public virtual bool HandleMember(MemberExpression node, out string fieldName)
            {
                Contract.Requires<ArgumentNullException>(node != null);

                fieldName = null;
                return false;
            }
        }

        /// <summary>
        /// Распознаватель доступа к полям записей,
        /// ассоциируемых с нетипизированной записью <see cref="OneSDataRecord"/>.
        /// </summary>
        private sealed class DataRecordFieldAccessRecognizer : FieldAccessRecognizer
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
            
            public DataRecordFieldAccessRecognizer(ParameterExpression recordExpression)
                : base(recordExpression)
            {
                Contract.Requires<ArgumentNullException>(recordExpression != null);
            }


            /// <summary>Обработчик вызова метода.</summary>
            /// <param name="node">Обрабатываемый узел выражения.</param>
            /// <param name="fieldName">
            /// Имя поля к которому производится доступ в выражении.
            /// Будет возвращен <c>null</c>, в случае если выражение не является доступом к полю записи.
            /// </param>
            /// <returns>
            /// Возвращает <c>true</c> в случае если узел выражает доступ к полю записи.
            /// В ином случае возвращается <c>false</c>.
            /// </returns>
            public override bool HandleMethodCall(MethodCallExpression node, out string fieldName)
            {
                if (node.Object == RecordExpression)
                {
                    if (_getValueMethods.Contains(node.Method))
                    {
                        fieldName = node.Arguments[0].GetConstant<string>();
                        return true;
                    }
                }

                return base.HandleMethodCall(node, out fieldName);
            }
        }

        /// <summary>
        /// Распознаватель доступа к полям записей,
        /// ассоциируемых с типизированными записями.
        /// </summary>
        private sealed class TypedRecordFieldAccessRecognizer : FieldAccessRecognizer
        {
            /// <summary>Карта соответствия типизированной записи источнику данных 1С.</summary>
            private readonly OneSTypeMapping _typeMapping;

            public TypedRecordFieldAccessRecognizer(ParameterExpression recordExpression, OneSTypeMapping typeMapping)
                : base(recordExpression)
            {
                Contract.Requires<ArgumentNullException>(recordExpression != null);
                Contract.Requires<ArgumentNullException>(typeMapping != null);

                _typeMapping = typeMapping;
            }

            /// <summary>Обработчик получения члена.</summary>
            /// <param name="node">Обрабатываемый узел выражения.</param>
            /// <param name="fieldName">
            /// Имя поля к которому производится доступ в выражении.
            /// Будет возвращен <c>null</c>, в случае если выражение не является доступом к полю записи.
            /// </param>
            /// <returns>
            /// Возвращает <c>true</c> в случае если узел выражает доступ к полю записи.
            /// В ином случае возвращается <c>false</c>.
            /// </returns>
            public override bool HandleMember(MemberExpression node, out string fieldName)
            {
                if (node.Expression == RecordExpression)
                {
                    fieldName = _typeMapping.GetFieldNameByMemberInfo(node.Member);
                    if (fieldName != null)
                        return true;

                    throw node.CreateExpressionNotSupportedException();
                }

                return base.HandleMember(node, out fieldName);
            }
        }

        #endregion
    }
}
