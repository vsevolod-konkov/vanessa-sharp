using System;
using System.Linq;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.UnitTests.Utility
{
    /// <summary>Вспомогательный класс для генерации linq-выражений.</summary>
    internal sealed class TestHelperQueryProvider : IQueryProvider
    {
        public static Expression BuildTestQueryExpression<T>(string source,
                                                             Func<IQueryable<OneSDataRecord>, IQueryable<T>> queryAction)
        {
            var provider = new TestHelperQueryProvider();
            var query = provider.CreateQuery<OneSDataRecord>(
                OneSQueryExpressionHelper.GetRecordsExpression(source));

            var resultQuery = queryAction(query);

            return Expression.Call(
                resultQuery.Expression,
                OneSQueryExpressionHelper.GetGetEnumeratorMethodInfo<T>());
        }

        public static Expression BuildTestQueryExpression(string source)
        {
            return BuildTestQueryExpression(source, q => q);
        }
        
        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotSupportedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestHelperQuery<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            throw new NotSupportedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            throw new NotSupportedException();
        }
    }
}