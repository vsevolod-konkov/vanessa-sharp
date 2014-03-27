using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Стандартная реализация <see cref="IExpressionParser"/>.</summary>
    internal sealed class ExpressionParser : IExpressionParser
    {
        /// <summary>Экземпляр по умолчанию.</summary>
        public static ExpressionParser Default
        {
            get { return _default; }
        }
        private static readonly ExpressionParser _default = new ExpressionParser();
        
        /// <summary>Разбор выражения.</summary>
        /// <param name="expression">Выражение.</param>
        public ExpressionParseProduct Parse(Expression expression)
        {
            var getEnumeratorExpr = VerifyMethodCallExpression(
                                    expression,
                                    OneSQueryExpressionHelper.GetGetEnumeratorMethodInfo<OneSDataRecord>());

            var getRecordsExpr = VerifyMethodCallExpression(
                                    getEnumeratorExpr.Object,
                                    OneSQueryExpressionHelper.GetRecordsMethodInfo);

            var sourceName = VerifyConstantExpression(getRecordsExpr.Arguments[0]);

            var sql = "SELECT * FROM " + sourceName;

            return new CollectionReadExpressionParseProduct<OneSDataRecord>(
                new SqlCommand(sql, SqlParameter.EmptyCollection),
                OneSDataRecordReaderFactory.Default);
        }

        private static MethodCallExpression VerifyMethodCallExpression(Expression expression, MethodInfo methodInfo)
        {
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Requires<ArgumentNullException>(methodInfo != null);
            
            var callExpression = expression as MethodCallExpression;
            if (callExpression == null || callExpression.Method != methodInfo)
                throw CreateExpressionNotSupportedException(expression);

            return callExpression;
        }

        private static object VerifyConstantExpression(Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            var constExpression = expression as ConstantExpression;
            if (constExpression == null)
                throw CreateExpressionNotSupportedException(expression);

            return constExpression.Value;
        }

        private static Exception CreateExpressionNotSupportedException(Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);
            
            return new NotSupportedException(string.Format("Выражение \"{0}\" не поддерживается.", expression));
        }
    }
}
