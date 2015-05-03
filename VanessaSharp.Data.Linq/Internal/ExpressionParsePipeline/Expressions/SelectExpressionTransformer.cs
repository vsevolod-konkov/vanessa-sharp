using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Преобразователь выражения метода Select в SQL-инструкцию SELECT и в делегат для вычитки элемента данных из записи.
    /// </summary>
    internal sealed class SelectExpressionTransformer : ExpressionVisitorBase
    {
        /// <summary>Приватный конструктор для инициализаии параметра метода.</summary>
        private SelectExpressionTransformer(QueryParseContext context, ParameterExpression recordExpression, IOneSMappingProvider mappingProvider)
        {
            _expressionBuilder = new SqlObjectBuilder(context, recordExpression, mappingProvider);
            _columnExpressionBuilder = new ColumnExpressionBuilderWrapper(mappingProvider);
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

            return new SelectExpressionTransformer(
                context, expression.Parameters[0], mappingProvider)
                
                .TransformLambdaBody<TOutput>(expression.Body);
        }

        /// <summary>Построитель SQL-выражений.</summary>
        private readonly SqlObjectBuilder _expressionBuilder;

        /// <summary>Построитель выражений для колонок выборки.</summary>
        private readonly ColumnExpressionBuilderWrapper _columnExpressionBuilder;

        /// <summary>
        /// Флаг, указывающий результат посещения узла.  Удалось ли построить SQL-выражение.
        /// </summary>
        private bool _hasSql;

        /// <summary>Преобразование тела лямбды метода выборки данных.</summary>
        /// <typeparam name="T">Тип элементов выборки.</typeparam>
        /// <param name="lambdaBody">Тело лямбды метода выборки.</param>
        /// <returns>
        /// Результат преобразования - набор колонок и делегат создания элемента из значений колонок.
        /// </returns>
        private SelectionPartParseProduct<T> TransformLambdaBody<T>(Expression lambdaBody)
        {
            _hasSql = false;

            var resultExpression = Visit(lambdaBody);
            if (_hasSql)
            {
                resultExpression = _columnExpressionBuilder.GetColumnAccessExpression(
                    _expressionBuilder.GetExpression(), resultExpression.Type);
            }

            return new SelectionPartParseProduct<T>(
                _columnExpressionBuilder.Columns,
                CreateItemReader<T>(resultExpression));
        }

        /// <summary>
        /// Создание делегата создателя элемента вычитываемого из записи.
        /// </summary>
        /// <typeparam name="T">Тип элемента.</typeparam>
        /// <param name="body">Тело лямбда-выражения создаваемого делегата.</param>
        private Func<IValueConverter, object[], T> CreateItemReader<T>(Expression body)
        {
            return Expression
                .Lambda<Func<IValueConverter, object[], T>>(body, _columnExpressionBuilder.ConverterParameter, _columnExpressionBuilder.ValuesParameter)
                .Compile();
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MethodCallExpression"/>.
        /// </summary>
        /// <remarks>
        /// Заменяет методы получения значений из записи <see cref="OneSDataRecord"/>
        /// на получение значений из массива вычитанных значений.
        /// </remarks>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            HandledNodeInfo obj = null;
            if (node.Object != null)
                obj = HandleNode(node.Object);

            var args = node.Arguments.Select(HandleNode).ToArray();

            if(_expressionBuilder.HandleMethodCall(node))
            {
                _hasSql = true;
                return node;
            }

            return node.Update(
                obj == null ? null: obj.GetTransformedNode(),
                args.Select(a => a.GetTransformedNode()));
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
            HandledNodeInfo obj = null;
            if (node.Expression != null)
                obj = HandleNode(node.Expression);
            
            if (_expressionBuilder.HandleMember(node))
            {
                _hasSql = true;
                return node;
            }

            return node.Update(obj == null ? null : obj.GetTransformedNode());
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
            _hasSql = _expressionBuilder.HandleParameter(node);

            return node;
        }

        /// <summary>Обработка узла.</summary>
        /// <param name="node">Обрабатываемый узел.</param>
        private HandledNodeInfo HandleNode(Expression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);
            
            // Сброс флага
            _hasSql = false;

            // Обход узла
            var handledNode = Visit(node);

            return new HandledNodeInfo(
                _columnExpressionBuilder,
                handledNode,
                node.Type,
                _hasSql ? _expressionBuilder.PeekExpression() : null);
        }

        /// <summary>
        /// Очистка состояния преобразователя.
        /// </summary>
        private void Clear()
        {
            _hasSql = false;
            _expressionBuilder.Clear();
        }

        /// <summary>Преобразование узла.</summary>
        private Expression TransformNode(Expression node)
        {
            Contract.Requires<ArgumentNullException>(node != null);
            
            var result = HandleNode(node).GetTransformedNode();
            Clear();

            return result;
        }

        /// <summary>
        /// Преобразование последовательности узлов.
        /// </summary>
        private IList<Expression> TransformNodes(IEnumerable<Expression> nodes)
        {
            return nodes.Select(TransformNode).ToArray();
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.BinaryExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = HandleNode(node.Left);
            var right = HandleNode(node.Right);

            _hasSql = left.HasSql && right.HasSql && _expressionBuilder.HandleBinary(node);
            if (_hasSql)
                return node;

            Clear();

            return node.Update(
                    left.GetTransformedNode(),
                    node.Conversion,
                    right.GetTransformedNode()
                    );
        }

        /// <summary>
        /// Просматривает выражение <see cref="T:System.Linq.Expressions.ConstantExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            _hasSql = _expressionBuilder.HandleConstant(node);

            return node;
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.ConditionalExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            var test = HandleNode(node.Test);
            var ifTrue = HandleNode(node.IfTrue);
            var ifFalse = HandleNode(node.IfFalse);

            _hasSql = test.HasSql
                      && ifTrue.HasSql
                      && ifFalse.HasSql
                      && _expressionBuilder.HandleConditional(node);

            if (_hasSql)
                return node;

            Clear();

            return node.Update(
                test.GetTransformedNode(),
                ifTrue.GetTransformedNode(),
                ifFalse.GetTransformedNode());
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.ElementInit"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override ElementInit VisitElementInit(ElementInit node)
        {
            var args = TransformNodes(node.Arguments);

            return node.Update(args);
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.IndexExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitIndex(IndexExpression node)
        {
            var obj = TransformNode(node.Object);
            var args = TransformNodes(node.Arguments);

            return node.Update(obj, args);
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.ListInitExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitListInit(ListInitExpression node)
        {
            var newExpression = (NewExpression)TransformNode(node.NewExpression);
            var initializers = node.Initializers.Select(VisitElementInit).ToArray();

            return node.Update(
                newExpression,
                initializers
                );
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MemberAssignment"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            var value = TransformNode(node.Expression);

            return node.Update(value);
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MemberInitExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var newExpression = (NewExpression)TransformNode(node.NewExpression);
            var bindings = node.Bindings.Select(VisitMemberBinding).ToArray();

            return node.Update(newExpression, bindings);
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MemberListBinding"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            var initializers = node.Initializers
                                   .Select(VisitElementInit)
                                   .ToArray();

            return node.Update(initializers);
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MemberMemberBinding"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            var bindings = node.Bindings.Select(VisitMemberBinding).ToArray();

            return node.Update(bindings);
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.NewExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitNew(NewExpression node)
        {
            var args = TransformNodes(node.Arguments);

            return node.Update(args);
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.NewArrayExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var exprs = TransformNodes(node.Expressions);

            return node.Update(exprs);
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.TypeBinaryExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            var obj = HandleNode(node.Expression);

            _hasSql = obj.HasSql && _expressionBuilder.HandleVisitTypeBinary(node);
            
            if (_hasSql)
                return node;

            Clear();
            return node.Update(obj.GetTransformedNode());
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.UnaryExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            var operand = HandleNode(node.Operand);

            _hasSql = operand.HasSql && _expressionBuilder.HandleUnary(node);
            if (_hasSql)
                return node;

            Clear();
            return node.Update(operand.GetTransformedNode());
        }

        #region Вспомогательные типы

        /// <summary>
        /// Информация по обработки узла.
        /// </summary>
        private sealed class HandledNodeInfo
        {
            /// <summary>
            /// Построитель выражений для чтения из колонок.
            /// </summary>
            private readonly ColumnExpressionBuilderWrapper _columnExpressionBuilder;

            /// <summary>
            /// Выражение - результат обработки узла.
            /// </summary>
            private readonly Expression _handledNode;

            /// <summary>
            /// Тип возвращаемый выражением.
            /// </summary>
            private readonly Type _type;

            /// <summary>
            /// Поставщик SQL-выражения, которое можно построить после обработки узла.
            /// </summary>
            /// <remarks>Может быть <c>null</c>, если выражение нельзя построить.</remarks>
            private readonly SqlObjectBuilder.ISqlExpressionProvider _sqlExpressionProvider;

            public HandledNodeInfo(
                ColumnExpressionBuilderWrapper columnExpressionBuilder,
                Expression handledNode,
                Type type,
                SqlObjectBuilder.ISqlExpressionProvider sqlExpressionProvider)
            {
                Contract.Requires<ArgumentNullException>(columnExpressionBuilder != null);
                Contract.Requires<ArgumentNullException>(handledNode != null);
                Contract.Requires<ArgumentNullException>(type != null);
                
                _columnExpressionBuilder = columnExpressionBuilder;
                _handledNode = handledNode;
                _type = type;
                _sqlExpressionProvider = sqlExpressionProvider;
            }

            public bool HasSql { get { return _sqlExpressionProvider != null; } }
            
            /// <summary>
            /// Получение преобразованного выражения, считвыающего данные из колонки запроса.
            /// </summary>
            private Expression GetColumnAccessExpression()
            {
                return _columnExpressionBuilder.GetColumnAccessExpression(
                    _sqlExpressionProvider.GetExpression(), _type);
            }

            /// <summary>
            /// Получение преобразованного значения выражения.
            /// </summary>
            /// <returns></returns>
            public Expression GetTransformedNode()
            {
                return HasSql ? GetColumnAccessExpression() : _handledNode;
            }
        }

        /// <summary>
        /// Построитель выражения читающий из колонок читателя записей.
        /// Тип выражения необязательно примитивный тип, но и маппируемые на таблицы 1С типы.
        /// </summary>
        private sealed class ColumnExpressionBuilderWrapper
        {
            private readonly ColumnExpressionBuilder _columnExpressionBuilder = new ColumnExpressionBuilder();
            private readonly IOneSMappingProvider _mappingProvider;
            private readonly TypedRecordParseProductBuilder _typedRecordParseProductBuilder;

            /// <summary>Конструктор.</summary>
            /// <param name="mappingProvider">Поставщик соответствий типов источникам данных 1С.</param>
            public ColumnExpressionBuilderWrapper(IOneSMappingProvider mappingProvider)
            {
                Contract.Requires<ArgumentNullException>(mappingProvider != null);

                _mappingProvider = mappingProvider;
                _typedRecordParseProductBuilder = new TypedRecordParseProductBuilder(_mappingProvider);
            }

            /// <summary>
            /// Получение выражения получения значения колонки записи.
            /// </summary>
            /// <param name="column">Выражение колонки.</param>
            /// <param name="type">Тип, который требуется для колонки.</param>
            public Expression GetColumnAccessExpression(SqlExpression column, Type type)
            {
                Contract.Requires<ArgumentNullException>(column != null);
                Contract.Requires<ArgumentNullException>(type != null);
                Contract.Ensures(Contract.Result<Expression>() != null);

                if (type == typeof (OneSDataRecord))
                {
                    throw new InvalidOperationException(
                        "Недопустимо использовать запись данных в качестве члена в выходной структуре. Можно использовать в выражении запись только для доступа к ее полям.");
                }

                return _mappingProvider.IsDataType(type)
                    ? _typedRecordParseProductBuilder.GetReaderExpression(column, _columnExpressionBuilder, type)
                    : _columnExpressionBuilder.GetColumnAccessExpression(column, type);
            }

            /// <summary>Вычитываемые колонки.</summary>
            public ReadOnlyCollection<SqlExpression> Columns
            {
                get
                {
                    Contract.Ensures(Contract.Result<ReadOnlyCollection<SqlExpression>>() != null);

                    return _columnExpressionBuilder.Columns;
                }
            }

            /// <summary>Параметр для результирующего делегата создания элемента - конвертер значений.</summary>
            public ParameterExpression ConverterParameter
            {
                get
                {
                    Contract.Ensures(Contract.Result<ParameterExpression>() != null);
                    Contract.Ensures(Contract.Result<ParameterExpression>().Type == typeof(IValueConverter));

                    return _columnExpressionBuilder.ConverterParameter;
                }
            }

            /// <summary>Параметр для результирующего делегата создания элемента - массив вычитанных значений.</summary>
            public ParameterExpression ValuesParameter
            {
                get
                {
                    Contract.Ensures(Contract.Result<ParameterExpression>() != null);
                    Contract.Ensures(Contract.Result<ParameterExpression>().Type == typeof(object[]));

                    return _columnExpressionBuilder.ValuesParameter;
                }
            }
        }

        #endregion
    }
}
