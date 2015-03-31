using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Предвычислитель выражений.
    /// </summary>
    /// <remarks>
    /// Преобразует выражение таким образом, что все максимально возможные узлы не зависящие от параметров
    /// вычисляются и преобразуются в <see cref="ConstantExpression"/>.
    /// </remarks>
    internal sealed class PreEvaluator : ExpressionVisitor
    {
        private PreEvaluator()
        {}

        /// <summary>
        /// Преобразования узла путем вычисления максимально возможных подузлов независящих от параметров.
        /// </summary>
        /// <param name="node">Преобразуемое выражение.</param>
        /// <returns>Преобразованное выражение.</returns>
        public static TExpression Evaluate<TExpression>(TExpression node)
            where TExpression : Expression
        {
            Contract.Requires<ArgumentNullException>(node != null);
            Contract.Ensures(Contract.Result<Expression>() != null);

            var instance = new PreEvaluator();
            return instance.VisitAndConvert(node, MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        /// Флаг уаказывающий, что у обработанного выражение имеются параметры.
        /// </summary>
        private bool _hasParameter;

        #region Вспомогательные типы

        /// <summary>
        /// Интерфейс узла для обработки.
        /// </summary>
        private interface INode
        {
            /// <summary>
            /// Посещение узла, для определения есть ли в нем параметр.
            /// </summary>
            /// <param name="vistor">Посетитель.</param>
            INode Visit(PreEvaluator vistor);

            /// <summary>
            /// Вычисление узла.
            /// </summary>
            INode Evaluate();

            /// <summary>
            /// Проверка на равенство узлов.
            /// </summary>
            bool Equals(INode otherNode);
        }

        /// <summary>
        /// Базовая реализация <see cref="INode"/>.
        /// </summary>
        /// <typeparam name="T">Тип настоящего узла дерева выражений.</typeparam>
        private abstract class NodeBase<T> : INode
        {
            protected NodeBase(T originNode)
            {
                _originNode = originNode;
            }
            
            /// <summary>
            /// Настоящий узел дерева выражений.
            /// </summary>
            protected T OriginNode
            {
                get { return _originNode; }
            }
            private readonly T _originNode;

            /// <summary>
            /// Обертка настоящего узла.
            /// </summary>
            /// <param name="originNode">Настоящий узел.</param>
            protected abstract NodeBase<T> Wrap(T originNode);

            /// <summary>
            /// Посещение настоящего узла для определения, есть ли в нем параметр.
            /// </summary>
            protected abstract T Visit(PreEvaluator vistor, T originNode);

            /// <summary>
            /// Вычисление настоящего узла.
            /// </summary>
            protected abstract T Evaluate(T originNode);

            /// <summary>
            /// Посещение узла, для определения есть ли в нем параметр.
            /// </summary>
            /// <param name="vistor">Посетитель.</param>
            public INode Visit(PreEvaluator vistor)
            {
                return Wrap(Visit(vistor, _originNode));
            }

            /// <summary>
            /// Вычисление узла.
            /// </summary>
            public INode Evaluate()
            {
                return Wrap(Evaluate(_originNode));
            }

            /// <summary>
            /// Проверка на равенство узлов.
            /// </summary>
            public bool Equals(INode otherNode)
            {
                var otherTypedNode = otherNode as NodeBase<T>;

                return otherTypedNode != null
                       && ReferenceEquals(_originNode, otherTypedNode._originNode);
            }
        }

        /// <summary>
        /// Класс узла для <see cref="Expression"/>.
        /// </summary>
        private sealed class ExpressionNode : NodeBase<Expression>
        {
            public ExpressionNode(Expression expression)
                : base(expression)
            {}

            protected override NodeBase<Expression> Wrap(Expression originNode)
            {
                return new ExpressionNode(originNode);
            }

            protected override Expression Visit(PreEvaluator vistor, Expression originNode)
            {
                if (originNode == null)
                    return null;

                return vistor.Visit(originNode);
            }

            protected override Expression Evaluate(Expression originNode)
            {
                return EvaluateTransform(originNode);
            }

            public Expression Expression
            {
                get { return OriginNode; }
            }
        }

        /// <summary>
        /// Класс узла для <see cref="System.Linq.Expressions.NewExpression"/>.
        /// </summary>
        private sealed class NewExpressionNode : NodeBase<NewExpression>
        {
            public NewExpressionNode(NewExpression expression)
                : base(expression)
            {}

            protected override NodeBase<NewExpression> Wrap(NewExpression originNode)
            {
                return new NewExpressionNode(originNode);
            }

            protected override NewExpression Visit(PreEvaluator vistor, NewExpression originNode)
            {
                return vistor.VisitTypedNew(originNode);
            }

            protected override NewExpression Evaluate(NewExpression originNode)
            {
                return EvaluateTransform(
                    originNode,
                    originNode.Arguments,
                    (n, args) => n.Update(args));
            }

            public NewExpression NewExpression
            {
                get { return OriginNode; }
            }
        }

        /// <summary>
        /// Класс узла для <see cref="System.Linq.Expressions.ElementInit"/>.
        /// </summary>
        private sealed class ElementInitNode : NodeBase<ElementInit>
        {
            public ElementInitNode(ElementInit elementInit)
                : base(elementInit)
            {}

            protected override NodeBase<ElementInit> Wrap(ElementInit originNode)
            {
                return new ElementInitNode(originNode);
            }

            protected override ElementInit Visit(PreEvaluator vistor, ElementInit originNode)
            {
                return vistor.VisitElementInit(originNode);
            }

            protected override ElementInit Evaluate(ElementInit originNode)
            {
                return EvaluateTransform(originNode);
            }

            public ElementInit ElementInit
            {
                get { return OriginNode; }
            }
        }

        /// <summary>
        /// Класс узла для <see cref="System.Linq.Expressions.MemberBinding"/>.
        /// </summary>
        private sealed class MemberBindingNode : NodeBase<MemberBinding>
        {
            public MemberBindingNode(MemberBinding memberBinding)
                : base(memberBinding)
            {}

            protected override NodeBase<MemberBinding> Wrap(MemberBinding originNode)
            {
                return new MemberBindingNode(originNode);
            }

            protected override MemberBinding Visit(PreEvaluator vistor, MemberBinding originNode)
            {
                return vistor.VisitMemberBinding(originNode);
            }

            protected override MemberBinding Evaluate(MemberBinding originNode)
            {
                var assignment = originNode as MemberAssignment;
                if (assignment != null)
                {
                    return assignment.Update(
                        EvaluateTransform(assignment.Expression));
                }

                var list = originNode as MemberListBinding;
                if (list != null)
                {
                    return list.Update(
                        list.Initializers.Select(EvaluateTransform));
                }

                var member = (MemberMemberBinding)originNode;
                return member.Update(
                    member.Bindings.Select(Evaluate));
            }

            public MemberBinding MemberBinding
            {
                get { return OriginNode; }
            }
        }

        /// <summary>
        /// Информация об узле после посещения его.
        /// </summary>
        private sealed class VisitedNodeInfo
        {
            public VisitedNodeInfo(INode node, bool hasParameter)
            {
                _node = node;
                _hasParameter = hasParameter;
            }

            /// <summary>
            /// Посещенный узел.
            /// </summary>
            public INode Node
            {
                get { return _node; }
            }
            private readonly INode _node;

            /// <summary>
            /// Имеет ли узел параметр.
            /// </summary>
            public bool HasParameter
            {
                get { return _hasParameter; }
            }
            private readonly bool _hasParameter;
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Обработка дочерних узлов выражения одним массивом.
        /// </summary>
        /// <param name="nodes">Дочерние узлы.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если какой-либо дочерний узел был изменен,
        /// что потребует пересоздания родительского узла.
        /// В ином случае <c>false</c>.
        /// </returns>
        private bool HandleChildNodes(IList<INode> nodes)
        {
            Contract.Requires<ArgumentNullException>(nodes != null);

            var visitedInfos = Visit(nodes);

            // side effect метода!!!
            _hasParameter = visitedInfos.Any(i => i.HasParameter);
            if (!_hasParameter)
                return false;

            // Проверяются дочерние узлы у которых нет параметров в выражении
            // Это и есть максимальные узлы без параметров, которые надо вычислить и
            // преобразовать в константные выражения

            var wasChanged = false;
            for (var index = 0; index < nodes.Count; index++)
            {
                var visitedInfo = visitedInfos[index];
                var newNode = visitedInfo.Node;
                if (!visitedInfo.HasParameter)
                    newNode = newNode.Evaluate();

                if (!nodes[index].Equals(newNode))
                    wasChanged = true;

                nodes[index] = newNode;
            }

            return wasChanged;
        }

        /// <summary>
        /// Обработка узла визитором.
        /// </summary>
        /// <param name="node">Обрабатываемый узел.</param>
        private VisitedNodeInfo Visit(INode node)
        {
            // Сброс флага, визитор проставит, если у узла есть параметр.
            _hasParameter = false;

            var result = node.Visit(this);

            return new VisitedNodeInfo(result, _hasParameter);
        }

        /// <summary>
        /// Обработка узлов визитором.
        /// </summary>
        /// <param name="nodes">Обрабатываемые узлы.</param>
        private IList<VisitedNodeInfo> Visit(IList<INode> nodes)
        {
            var result = new VisitedNodeInfo[nodes.Count];

            for (var i = 0; i < nodes.Count; i++)
                result[i] = Visit(nodes[i]);

            return result;
        }

        /// <summary>
        /// Обработка дочерних подвыражений выражения одним массивом.
        /// </summary>
        /// <param name="nodes">Дочерние узлы.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если какой-либо дочерний узел был изменен,
        /// что потребует пересоздания родительского узла.
        /// В ином случае <c>false</c>.
        /// </returns>
        private bool HandleChildNodes(IList<Expression> nodes)
        {
            Contract.Requires<ArgumentNullException>(nodes != null);

            var wrapNodes = nodes.Select(expr => new ExpressionNode(expr)).ToArray();

            var wasChanged = HandleChildNodes(wrapNodes);

            if (wasChanged)
            {
                for (var index = 0; index < nodes.Count; index++)
                    nodes[index] = wrapNodes[index].Expression;
            }

            return wasChanged;
        }

        /// <summary>
        /// Вычисление узла в котором нет параметров и преобразование его в константный.
        /// </summary>
        /// <param name="node">Узел.</param>
        private static ConstantExpression EvaluateTransform(Expression node)
        {
            if (node == null)
                return null;

            var constantExpression = node as ConstantExpression;
            if (constantExpression != null)
                return constantExpression;

            var body = (node.Type == typeof(object))
                                  ? node
                                  : Expression.Convert(node, typeof(object));

            var lambda = Expression
                            .Lambda<Func<object>>(body)
                            .Compile();

            var value = lambda();

            return Expression.Constant(value, node.Type);
        }

        /// <summary>
        /// Вычисление и преобразование узла.
        /// </summary>
        private static T EvaluateTransform<T>(
            T node, IList<Expression> args, Func<T, IEnumerable<Expression>, T> updateAction)
            where T : class 
        {
            Contract.Requires<ArgumentNullException>(node != null);
            Contract.Requires<ArgumentNullException>(args != null);
            Contract.Requires<ArgumentNullException>(updateAction != null);

            var evaluatedArgs = new Expression[args.Count];
            for (var index = 0; index < args.Count; index++)
            {
                evaluatedArgs[index] = EvaluateTransform(args[index]);
            }

            return updateAction(node, evaluatedArgs);
        }

        /// <summary>
        /// Вычисление и преобразование инициализации элемента.
        /// </summary>
        private static ElementInit EvaluateTransform(ElementInit node)
        {
            Contract.Requires<ArgumentNullException>(node != null);

            return EvaluateTransform(node,
                node.Arguments, (n, args) => n.Update(args));
        }

        /// <summary>
        /// Обработка выражения, с одним дочерним узлом-подвыражением.
        /// </summary>
        /// <typeparam name="T">Тип выражения.</typeparam>
        /// <param name="node">Обрабатываемое выражение.</param>
        /// <param name="child">Дочерний узел.</param>
        /// <param name="updateAction">Действие по модификации исходного выражения.</param>
        private T TypedVisit<T>(
            T node,
            Expression child,
            Func<T, Expression, T> updateAction)
            where T : Expression
        {
            var children = new[] { child };
            var wasChanged = HandleChildNodes(children);

            return (wasChanged)
                       ? updateAction(node, children[0])
                       : node;
        }

        /// <summary>
        /// Обработка выражения, с тремя дочерними узлами-подвыражениями.
        /// </summary>
        /// <typeparam name="T">Тип выражения.</typeparam>
        /// <param name="node">Обрабатываемое выражение.</param>
        /// <param name="firstChild">Первый дочерний узел.</param>
        /// <param name="secondChild">Второй дочерний узел.</param>
        /// <param name="thirdChild">Третий дочерний узел.</param>
        /// <param name="updateAction">Действие по модификации исходного выражения.</param>
        private T TypedVisit<T>(
            T node, 
            Expression firstChild, Expression secondChild, Expression thirdChild,
            Func<T, Expression, Expression, Expression, T> updateAction)
            where T : Expression
        {
            var children = new[] { firstChild, secondChild, thirdChild };
            var wasChanged = HandleChildNodes(children);

            return (wasChanged)
                       ? updateAction(node, children[0], children[1], children[2])
                       : node;
        }

        private TOutput TypedVisit<TInput, TOutput>(
            TInput node,
            IEnumerable<Expression> arguments,
            Func<TInput, IEnumerable<Expression>, TOutput> updateAction)
            where TInput : TOutput
        {
            var children = arguments.ToArray();
            var wasChanged = HandleChildNodes(children);

            return (wasChanged)
                       ? updateAction(node, children)
                       : node;
        }

        private T TypedVisit<T>(
            T node,
            Expression instance, IEnumerable<Expression> arguments,
            Func<T, Expression, IEnumerable<Expression>, T> updateAction)
            where T : Expression
        {
            var children = Enumerable.Repeat(instance, 1).Concat(arguments).ToArray();
            var wasChanged = HandleChildNodes(children);

            return (wasChanged)
                       ? updateAction(node, children[0], children.Skip(1))
                       : node;
        }

        private T TypedVisit<T>(
            T node,
            NewExpression instance, IEnumerable<INode> elementNodes,
            Func<T, NewExpression, IEnumerable<INode>, T> updateAction)
            where T : Expression
        {
            var children = Enumerable
                .Repeat<INode>(new NewExpressionNode(instance), 1)
                .Concat(elementNodes)
                .ToArray();

            var wasChanged = HandleChildNodes(children);

            return (wasChanged)
                       ? updateAction(node, ((NewExpressionNode)children[0]).NewExpression, children.Skip(1))
                       : node;
        }

        private T TypedVisit<T, TElementNode>(
            T node,
            IEnumerable<TElementNode> elementNodes,
            Func<T, IEnumerable<TElementNode>, T> updateAction)
            where TElementNode : INode
        {
            var wrapNodes = (IList<TElementNode>)elementNodes.ToArray();

            var wasChanged = HandleChildNodes((IList<INode>)wrapNodes);

            return (wasChanged)
                ? updateAction(node, wrapNodes)
                : node;
        }

        #endregion

        #region Методы посещения

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return TypedVisit(
                node, 
                node.Test, node.IfTrue, node.IfFalse,
                (n, test, ifTrue, ifFalse) => n.Update(test, ifTrue, ifFalse));
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            return node;
        }

        protected override Expression VisitDefault(DefaultExpression node)
        {
            return node;
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            return TypedVisit(
                node,
                node.Arguments,
                (n, args) => n.Update(args));
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            return TypedVisit(
                node,
                node.Object, node.Arguments,
                (n, instance, args) => n.Update(instance, args));
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            return TypedVisit(
                node,
                node.Expression, node.Arguments,
                (n, instance, args) => n.Update(instance, args));
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return TypedVisit(
                node,
                node.Body,
                (n, child) => n.Update(child, n.Parameters));
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            return TypedVisit(
                node,
                node.NewExpression,
                node.Initializers.Select(i => new ElementInitNode(i)),
                (n, newExpression, initializerNodes) =>
                    n.Update(newExpression, initializerNodes.Cast<ElementInitNode>().Select(e => e.ElementInit))
                );
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            return TypedVisit(
                node,
                node.NewExpression,
                node.Bindings.Select(b => new MemberBindingNode(b)),
                (n, newExpression, bindingNodes) =>
                    n.Update(newExpression, bindingNodes.Cast<MemberBindingNode>().Select(e => e.MemberBinding))
                );
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            return TypedVisit(
                node,
                node.Initializers.Select(i => new ElementInitNode(i)),
                (n, initializers) => n.Update(initializers.Select(i => i.ElementInit))
                );
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            return TypedVisit(
                node,
                node.Bindings.Select(i => new MemberBindingNode(i)),
                (n, bindings) => n.Update(bindings.Select(i => i.MemberBinding))
                );
        }

        private NewExpression VisitTypedNew(NewExpression node)
        {
            return TypedVisit(
                node,
                node.Arguments,
                (n, args) => n.Update(args));
        }

        protected override Expression VisitNew(NewExpression node)
        {
            return VisitTypedNew(node);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            return TypedVisit(
                node,
                node.Expressions,
                (n, exprs) => n.Update(exprs));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return TypedVisit(
                node,
                node.Object, node.Arguments,
                (n, instance, args) => n.Update(instance, args));
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            return TypedVisit(
                node,
                node.Left, node.Conversion, node.Right,
                (n, left, conversion, right) => n.Update(left, (LambdaExpression)conversion, right));
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _hasParameter = true;
            return node;
        }

        #endregion
    }
}
