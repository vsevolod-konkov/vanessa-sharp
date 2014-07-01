using System;
using System.Linq;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.UnitTests.Utility
{
    /// <summary>Вспомогательный класс для генерации linq-выражений.</summary>
    internal sealed class TestHelperQueryProvider : IQueryProvider
    {
        /// <summary>Построение тестового выражения запроса данных.</summary>
        /// <typeparam name="T">Тип элементов выходной последовательности.</typeparam>
        /// <param name="queryable">Объект запроса.</param>
        public static Expression BuildTestQueryExpression<T>(IQueryable<T> queryable)
        {
            return Expression.Call(
                queryable.Expression,
                OneSQueryExpressionHelper.GetGetEnumeratorMethodInfo<T>());
        }

        public static IQueryable<OneSDataRecord> QueryOfDataRecords(string sourceName)
        {
            return new TestHelperQueryProvider()
                .CreateQuery<OneSDataRecord>(
                    OneSQueryExpressionHelper.GetRecordsExpression(sourceName));
        }

        /// <summary>
        /// Создание запроса получения типизированных записей.
        /// </summary>
        /// <typeparam name="T">Тип записей.</typeparam>
        public static IQueryable<T> QueryOf<T>()
        {
            return new TestHelperQueryProvider()
                .CreateQuery<T>(
                    OneSQueryExpressionHelper.GetTypedRecordsExpression<T>());
        }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Создает объект <see cref="T:System.Linq.IQueryable`1"/>, который позволяет вычислить запрос, представленный заданным деревом выражения.
        /// </summary>
        /// <returns>
        /// Объект <see cref="T:System.Linq.IQueryable`1"/>, который позволяет вычислить запрос, представленный заданным деревом выражения.
        /// </returns>
        /// <param name="expression">Дерево выражения, представляющее запрос LINQ.</param><typeparam name="TElement">Тип элементов возвращаемого объекта <see cref="T:System.Linq.IQueryable`1"/>.</typeparam>
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