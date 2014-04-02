using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Посетитель для парсинга выражения генерируемого 
    /// <see cref="Queryable"/>.
    /// </summary>
    internal sealed class QueryableExpressionVisitor : ExpressionVisitor
    {
        /// <summary>Обработчик выражений.</summary>
        private readonly IQueryableExpressionHandler _handler;
        
        /// <summary>Конструктор.</summary>
        /// <param name="handler">Обработчик выражений.</param>
        public QueryableExpressionVisitor(IQueryableExpressionHandler handler)
        {
            Contract.Requires<ArgumentNullException>(handler != null);

            _handler = handler;
        }

        private static Exception CreateExpressionNotSupportedException(Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            return new NotSupportedException(string.Format("Выражение \"{0}\" не поддерживается.", expression));
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MethodCallExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method == OneSQueryExpressionHelper.GetGetEnumeratorMethodInfo<OneSDataRecord>())
            {
                _handler.HandleGettingEnumerator(typeof(OneSDataRecord));
            }
            else if (node.Method == OneSQueryExpressionHelper.GetRecordsMethodInfo)
            {
                _handler.HandleGettingRecords(GetConstant<string>(node.Arguments[0]));
                return node;
            }
            else
            {
                throw CreateExpressionNotSupportedException(node);
            }
            
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitDefault(DefaultExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitExtension(Expression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitLabel(LabelExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitTry(TryExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            throw CreateExpressionNotSupportedException(node);
        }

        private static T GetConstant<T>(Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            var constExpression = expression as ConstantExpression;
            if (constExpression == null)
                throw CreateExpressionNotSupportedException(expression);

            return (T)constExpression.Value;
        }
    }
}
