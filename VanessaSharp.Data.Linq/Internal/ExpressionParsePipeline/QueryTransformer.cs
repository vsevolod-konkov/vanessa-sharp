using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Преобразователь запроса в объект <see cref="ExpressionParseProduct"/>.</summary>
    internal static class QueryTransformer
    {
        /// <summary>Часть запроса, отвечающая за выборку записи.</summary>
        private static readonly Tuple<IList<string>, IItemReaderFactory<OneSDataRecord>>
            _recordSelectPart = new Tuple<IList<string>, IItemReaderFactory<OneSDataRecord>>(
                new[] { "*" }, OneSDataRecordReaderFactory.Default);
        
        /// <summary>
        /// Преобразование запроса в результат 
        /// разбора выражения.
        /// </summary>
        /// <typeparam name="T">Тип элементов результата.</typeparam>
        /// <param name="query">Запрос.</param>
        public static CollectionReadExpressionParseProduct<T> Transform<T>(CustomDataTypeQuery<T> query)
        {
            Contract.Requires<ArgumentNullException>(query != null);
            Contract.Ensures(Contract.Result<CollectionReadExpressionParseProduct<T>>() != null);

            return GetExpressionParseProduct(
                query,
                ParseSelectExpression(query.SelectExpression));
        }

        /// <summary>
        /// Преобразование запроса в результат 
        /// разбора выражения.
        /// </summary>
        /// <param name="query">Запрос.</param>
        public static CollectionReadExpressionParseProduct<OneSDataRecord> Transform(DataRecordsQuery query)
        {
            Contract.Requires<ArgumentNullException>(query != null);
            Contract.Ensures(Contract.Result<CollectionReadExpressionParseProduct<OneSDataRecord>>() != null);
            
            return GetExpressionParseProduct(
                query,
                _recordSelectPart);
        }

        /// <summary>Построение SQL-запроса.</summary>
        /// <param name="source">Источник.</param>
        /// <param name="fields">Поля.</param>
        internal static string BuildSql(string source, IList<string> fields)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(source));
            Contract.Requires<ArgumentNullException>(fields != null && fields.Count > 0);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return "SELECT " + ConcateFields(fields) + " FROM " + source;
        }

        private static CollectionReadExpressionParseProduct<T> GetExpressionParseProduct<T>(
            SimpleQuery query,
            Tuple<IList<string>, IItemReaderFactory<T>> selectPart)
        {
            return GetExpressionParseProduct(query.Source, selectPart);
        }

        private static CollectionReadExpressionParseProduct<T> GetExpressionParseProduct<T>(
            string source,
            Tuple<IList<string>, IItemReaderFactory<T>> selectPart)
        {
            return new CollectionReadExpressionParseProduct<T>(
                new SqlCommand(BuildSql(source, selectPart.Item1),
                    SqlParameter.EmptyCollection),
                selectPart.Item2);
        }

        private static Tuple<IList<string>, IItemReaderFactory<T>> ParseSelectExpression<T>(
            Expression<Func<OneSDataRecord, T>> selectExpression)
        {
            Contract.Requires<ArgumentNullException>(selectExpression != null);

            var part = SelectionVisitor.Parse(selectExpression);

            return new Tuple<IList<string>, IItemReaderFactory<T>>(
                part.Fields,
                new NoSideEffectItemReaderFactory<T>(part.SelectionFunc));
        }
        

        private static string ConcateFields(IEnumerable<string> fields)
        {
            return string.Join(", ", fields);
        }
    }
}
