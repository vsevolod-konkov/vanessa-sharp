using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VanessaSharp.Data.Linq
{
    /// <summary>Вспомогательные методы для linq-выражений.</summary>
    internal static class OneSQueryExpressionHelper
    {
        private static MethodInfo ExtractMethodInfo(LambdaExpression expression)
        {
            return ((MethodCallExpression)expression.Body).Method;
        }

        private static MethodInfo ExtractMethodInfo<T>(Expression<T> expression)
        {
            return ExtractMethodInfo((LambdaExpression)expression);
        }
        
        /// <summary>
        /// Метод <see cref="OneSQueryMethods.GetRecords"/>.
        /// </summary>
        public static MethodInfo GetRecordsMethodInfo
        {
            get { return _getRecordsMethodInfo; }
        }
        private static readonly MethodInfo _getRecordsMethodInfo = GetGetRecordsMethodInfo();

        /// <summary>
        /// Получение метода <see cref="OneSQueryMethods.GetRecords"/>.
        /// </summary>
        /// <returns></returns>
        private static MethodInfo GetGetRecordsMethodInfo()
        {
            return ExtractMethodInfo<Func<IQueryable<OneSDataRecord>>>(
                () => OneSQueryMethods.GetRecords("[sourceName]"));
        }

        /// <summary>
        /// Генерация выражения получения записей.
        /// </summary>
        /// <param name="sourceName">Имя источника.</param>
        public static Expression GetRecordsExpression(string sourceName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));
            Contract.Ensures(Contract.Result<Expression>() != null);

            return Expression.Call(GetRecordsMethodInfo,
                    Expression.Constant(sourceName));
        }

        /// <summary>Получение типа результата выражения.</summary>
        /// <param name="expression">Выражение.</param>
        public static Type GetResultType(this Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            var lambdaExpression = expression as LambdaExpression;

            return (lambdaExpression == null)
                ? expression.Type
                : lambdaExpression.ReturnType;
        }

        public static MethodInfo GetGetEnumeratorMethodInfo<T>()
        {
            return ItemMethods<T>.GetEnumeratorMethodInfo;
        }

        private static class ItemMethods<T>
        {
            public static MethodInfo GetEnumeratorMethodInfo
            {
                get { return _getEnumeratorMethodInfo; }
            }
            private static readonly MethodInfo _getEnumeratorMethodInfo = GetGetEnumeratorMethodInfo();

            private static MethodInfo GetGetEnumeratorMethodInfo()
            {
                return ExtractMethodInfo<Func<IEnumerable<T>, IEnumerator<T>>>(
                    e => e.GetEnumerator());
            }
        }
    }
}
