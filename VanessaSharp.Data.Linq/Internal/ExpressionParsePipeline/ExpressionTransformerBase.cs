using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Базовый класс трансформации LINQ-выражения в объект другого типа.
    /// </summary>
    internal abstract class ExpressionTransformerBase : ExpressionVisitorBase
    {
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

        protected override Expression VisitMethodCall(MethodCallExpression node)
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

        /// <summary>Посещение бинарного узла выражения по умолчанию.</summary>
        /// <param name="node">Посещаемый узел.</param>
        protected Expression DefaultVisitBinary(BinaryExpression node)
        {
            return base.VisitBinary(node);
        }

        /// <summary>Посещение узла выражения унарной операции по умолчанию.</summary>
        /// <param name="node">Посещаемый узел.</param>
        protected Expression DefaultVisitUnary(UnaryExpression node)
        {
            return base.VisitUnary(node);
        }

        /// <summary>
        /// Посещение узла выражения операции выражения и типа по умолчанию.
        /// </summary>
        /// <param name="node">Посещаемый узел.</param>
        protected Expression DefaultVisitTypeBinary(TypeBinaryExpression node)
        {
            return base.VisitTypeBinary(node);
        }
    }
}
