using System;
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
    /// Преобразователь выражения метода Select в SQL-инструкцию SELECT и в делегат для вычитки элемента данных из записи.
    /// </summary>
    internal sealed class SelectExpressionTransformer : ExpressionVisitorBase
    {
        /// <summary>Приватный конструктор для инициализаии параметра метода.</summary>
        private SelectExpressionTransformer(QueryParseContext context, ParameterExpression recordExpression, IOneSMappingProvider mappingProvider, OneSDataLevel level)
        {
            _mappingProvider = mappingProvider;
            _queryParseContext = context;
            _expressionBuilder = new SqlObjectBuilder(context, recordExpression, mappingProvider, level);
            _columnExpressionBuilder = new ColumnExpressionBuilderWrapper(mappingProvider);
        }

        /// <summary>Преобразование LINQ-выражения метода Select.</summary>
        /// <typeparam name="TInput">Тип элементов исходной последовательности.</typeparam>
        /// <typeparam name="TOutput">Тип элементов выходной последовательности - результатов выборки.</typeparam>
        /// <param name="mappingProvider">Поставщик соответствий типов источникам данных 1С.</param>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="expression">Преобразуемое выражение.</param>
        /// <param name="level">Уровень преобразуемого запроса.</param>
        public static SelectionPartParseProduct<TOutput> Transform<TInput, TOutput>(
            IOneSMappingProvider mappingProvider, 
            QueryParseContext context, 
            Expression<Func<TInput, TOutput>> expression,
            OneSDataLevel level)
        {
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Ensures(Contract.Result<SelectionPartParseProduct<TOutput>>() != null);

            return new SelectExpressionTransformer(
                context, expression.Parameters[0], mappingProvider, level)
                
                .TransformLambdaBody<TOutput>(expression.Body);
        }

        /// <summary>Преобразование LINQ-выражения метода Select.</summary>
        /// <param name="mappingProvider">Поставщик соответствий типов источникам данных 1С.</param>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="expression">Преобразуемое выражение.</param>
        /// <param name="level">Уровень преобразуемого запроса.</param>
        private static ISelectionPartParseProduct Transform(IOneSMappingProvider mappingProvider, QueryParseContext context, LambdaExpression expression, OneSDataLevel level)
        {
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Requires<ArgumentException>(expression.Parameters.Count == 1);
            Contract.Requires<ArgumentException>(expression.ReturnType != typeof(void));

            Contract.Ensures(Contract.Result<ISelectionPartParseProduct>() != null);

            var method = typeof (SelectExpressionTransformer).GetMethod("Transform",
                                                                        BindingFlags.Static | BindingFlags.Public);
            Contract.Assert(method != null);
            Contract.Assert(method.IsGenericMethodDefinition);

            var inputType = expression.Parameters[0].Type;
            var outputType = expression.ReturnType;

            var genericMethod = method.MakeGenericMethod(new[] {inputType, outputType });

            return (ISelectionPartParseProduct)
                genericMethod.Invoke(null, new object[] {mappingProvider, context, expression, level});
        }

        private readonly IOneSMappingProvider _mappingProvider;

        private readonly QueryParseContext _queryParseContext;

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

            // Обработка дочерних узлов зависит от метода и его аргументов,
            var args = new HandledNodeInfo[node.Arguments.Count];
            
            // Проведем обработку специальных методов
            Expression result;
            int handledArgsCount;
            if (HandleCallSpecialMethod(node, args, out result, out handledArgsCount))
                return result;

            // Дообработаем оставшиеся узлы
            for (var argIndex = handledArgsCount; argIndex < node.Arguments.Count; argIndex++)
                args[argIndex] = HandleNode(node.Arguments[argIndex]);


            if(_expressionBuilder.HandleMethodCall(node))
            {
                _hasSql = true;
                return node;
            }

            return node.Update(
                obj == null ? null: obj.GetTransformedNode(),
                args.Select(a => a.GetTransformedNode()));
        }

        /// <summary>Обработка специальных методов в выражении.</summary>
        /// <param name="node">Узел вызова метода.</param>
        /// <param name="handledArgs">Список обработанных узлов аргументов. Размер должен быть равен количеству аргументов.</param>
        /// <param name="result">Результрующие выражение.</param>
        /// <param name="handledArgsCount">Количество обработанных аргументов.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если результирующее выражение было получено.
        /// </returns>
        private bool HandleCallSpecialMethod(MethodCallExpression node, IList<HandledNodeInfo> handledArgs,
                                             out Expression result, out int handledArgsCount)
        {
            handledArgsCount = 0;

            if (OneSQueryExpressionHelper.IsEnumerableMethod(node.Method) && node.Arguments.Count > 0)
            {
                // Специальная обработка Linq-запросов
                var source = HandleNode(node.Arguments[handledArgsCount]);
                handledArgs[handledArgsCount++] = source;

                if (source.IsTablePart)
                {
                    result = TransformLinqTablePartExpression(source, node.Method, node.Arguments.Skip(handledArgsCount));
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>Преобразование LINQ-запроса к табличной части.</summary>
        /// <param name="tablePartNode">Обработанный узел табличной части.</param>
        /// <param name="linqMethod">LINQ-метод.</param>
        /// <param name="args">Аргументы LINQ-метода.</param>
        private Expression TransformLinqTablePartExpression(
            HandledNodeInfo tablePartNode, MethodInfo linqMethod, IEnumerable<Expression> args)
        {
            // Преобразование в SQL-запрос поддерживается только для Select-выражение
            if (OneSQueryExpressionHelper.IsEnumerableSelectMethod(linqMethod))
            {
                return TransformSelectFromTablePartExpression(
                    tablePartNode, (LambdaExpression)args.Single());
            }

            throw new NotSupportedException(string.Format(
                "Не поддерживается вызов linq-метода \"{0}\" для табличной части \"{1}\". Аргументы: \"{2}\".",
                linqMethod,
                tablePartNode,
                args.Select(a => a.ToString()).Aggregate("[", (r, a) => r + ", " + a, r => r + "]")));
        }

        /// <summary>Преобразование выражения выборки из табличной части.</summary>
        /// <param name="tablePartNode">Обработанный узел табличной части.</param>
        /// <param name="selectExpression">Выражение выборки.</param>
        private Expression TransformSelectFromTablePartExpression(
            HandledNodeInfo tablePartNode, LambdaExpression selectExpression)
        {
            var selectionParseProduct = Transform(_mappingProvider, _queryParseContext, selectExpression, OneSDataLevel.TablePart);

            _hasSql = false;
            return tablePartNode.GetTablePartTransformedNode(selectionParseProduct);
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

            var isTablePart = _expressionBuilder.IsTablePart;
            _expressionBuilder.ClearIsTablePart();

            return new HandledNodeInfo(
                _columnExpressionBuilder,
                handledNode,
                node.Type,
                _hasSql ? _expressionBuilder.PeekExpression() : null,
                isTablePart);
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

            private readonly bool _isTablePart;

            public HandledNodeInfo(
                ColumnExpressionBuilderWrapper columnExpressionBuilder,
                Expression handledNode,
                Type type,
                SqlObjectBuilder.ISqlExpressionProvider sqlExpressionProvider,
                bool isTablePart)
            {
                Contract.Requires<ArgumentNullException>(columnExpressionBuilder != null);
                Contract.Requires<ArgumentNullException>(handledNode != null);
                Contract.Requires<ArgumentNullException>(type != null);
                
                _columnExpressionBuilder = columnExpressionBuilder;
                _handledNode = handledNode;
                _type = type;
                _sqlExpressionProvider = sqlExpressionProvider;
                _isTablePart = isTablePart;
            }

            public bool HasSql { get { return _sqlExpressionProvider != null; } }

            public bool IsTablePart { get { return _isTablePart; } }
            
            /// <summary>
            /// Получение преобразованного выражения, считвыающего данные из колонки запроса.
            /// </summary>
            private Expression GetColumnAccessExpression()
            {
                return _isTablePart
                           ? _columnExpressionBuilder.GetTablePartColumnAccessExpression(_sqlExpressionProvider.GetExpression())
                           : _columnExpressionBuilder.GetColumnAccessExpression(_sqlExpressionProvider.GetExpression(), _type);
            }

            /// <summary>
            /// Получение преобразованного значения выражения.
            /// </summary>
            /// <returns></returns>
            public Expression GetTransformedNode()
            {
                return HasSql ? GetColumnAccessExpression() : _handledNode;
            }

            public Expression GetTablePartTransformedNode(ISelectionPartParseProduct selectionPartParseProduct)
            {
                Contract.Requires<ArgumentNullException>(selectionPartParseProduct != null);
                Contract.Requires<InvalidOperationException>(IsTablePart);
                Contract.Ensures(Contract.Result<Expression>() != null);

                return _columnExpressionBuilder.GetTablePartColumnAccessExpression(
                    _sqlExpressionProvider.GetExpression(),
                    selectionPartParseProduct);
            }

            public override string ToString()
            {
                return _handledNode.ToString();
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

                if (type == typeof(OneSDataRecord))
                {
                    throw new InvalidOperationException(
                        "Недопустимо использовать запись данных в качестве члена в выходной структуре. Можно использовать в выражении запись только для доступа к ее полям.");
                }

                return _mappingProvider.IsDataType(OneSDataLevel.Root, type)
                    ? _typedRecordParseProductBuilder.GetRootReaderExpression(column, _columnExpressionBuilder, type)
                    : _columnExpressionBuilder.GetColumnAccessExpression(column, type);
            }

            public Expression GetTablePartColumnAccessExpression(SqlExpression column)
            {
                Contract.Requires<ArgumentNullException>(column != null);
                Contract.Ensures(Contract.Result<Expression>() != null);

                return _columnExpressionBuilder
                    .GetTablePartColumnAccessExpression(column, OneSDataRecordReaderFactory.Default);
            }

            public Expression GetTablePartColumnAccessExpression(SqlExpression column, ISelectionPartParseProduct selectionPartParseProduct)
            {
                Contract.Requires<ArgumentNullException>(column != null);
                Contract.Requires<ArgumentNullException>(selectionPartParseProduct != null);
                Contract.Ensures(Contract.Result<Expression>() != null);

                return selectionPartParseProduct
                    .GetTablePartColumnAccessExpression(column, _columnExpressionBuilder);
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
