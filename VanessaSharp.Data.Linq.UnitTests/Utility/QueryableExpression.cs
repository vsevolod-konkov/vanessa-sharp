using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.UnitTests.Utility
{
    /// <summary>
    /// Построение выражений с использованием интерфейса
    /// <see cref="IQueryable{T}"/>
    /// во fluent-стиле.
    /// </summary>
    internal static class QueryableExpression
    {
        /// <summary>
        /// Построение выражения запроса для записей <see cref="OneSDataRecord"/>.
        /// </summary>
        /// <param name="sourceName">Имя источника данных 1С.</param>
        /// <returns></returns>
        public static Builder<OneSDataRecord> ForDataRecords(string sourceName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));

            return new Builder<OneSDataRecord>(
                TestHelperQueryProvider.QueryOfDataRecords(sourceName));
        }

        /// <summary>
        /// Построение выражения запроса типизированных записей.
        /// </summary>
        /// <typeparam name="T">Тип записей.</typeparam>
        /// <returns></returns>
        public static Builder<T> For<T>()
        {
            Contract.Requires<ArgumentException>(typeof(T) != typeof(OneSDataRecord));

            return new Builder<T>(
                TestHelperQueryProvider.QueryOf<T>());
        }

        /// <summary>Класс построителя.</summary>
        /// <typeparam name="T">Тип записей в запросе.</typeparam>
        public sealed class Builder<T>
        {
            private readonly IQueryable<T> _queryable;

            public Builder(IQueryable<T> queryable)
            {
                Contract.Requires<ArgumentNullException>(queryable != null);

                _queryable = queryable;
            }

            /// <summary>
            /// Построение выражения запроса
            /// методом выполения методов linq над объектом
            /// <see cref="IQueryable{T}"/>.
            /// </summary>
            /// <typeparam name="TResult">Тип элементов результата запроса.</typeparam>
            /// <param name="queryAction">Действие над объектом запроса.</param>
            public Expression Query<TResult>(Func<IQueryable<T>, IQueryable<TResult>> queryAction)
            {
                Contract.Requires<ArgumentNullException>(queryAction != null);

                return TestHelperQueryProvider.BuildTestQueryExpression(
                    queryAction(_queryable));
            }

            /// <summary>
            /// Выражение текущего объекта запроса.
            /// </summary>
            public Expression Expression
            {
                get { return TestHelperQueryProvider.BuildTestQueryExpression(_queryable); }
            }
        }
    }
}
