using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Преобразователь запроса в объект <see cref="ExpressionParseProduct"/>.</summary>
    internal static class QueryTransformer
    {
        /// <summary>Часть запроса, отвечающая за выборку записи.</summary>
        private static readonly SelectPartInfo<OneSDataRecord>
            _recordSelectPart = new SelectPartInfo<OneSDataRecord>(
                SqlAllColumnsExpression.Instance, OneSDataRecordReaderFactory.Default);
        
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

        private static CollectionReadExpressionParseProduct<T> GetExpressionParseProduct<T>(
            SimpleQuery query, SelectPartInfo<T> selectPart)
        {
            return GetExpressionParseProduct(new SqlFromStatement(query.Source), selectPart);
        }

        private static CollectionReadExpressionParseProduct<T> GetExpressionParseProduct<T>(
            SqlFromStatement fromStatement, SelectPartInfo<T> selectPart)
        {
            var queryStatement = new SqlQueryStatement(
                                        selectPart.Statement,
                                        fromStatement, 
                                        null);

            return new CollectionReadExpressionParseProduct<T>(
                new SqlCommand(queryStatement.BuildSql(),
                    SqlParameter.EmptyCollection),
                selectPart.ItemReaderFactory);
        }

        private static SelectPartInfo<T> ParseSelectExpression<T>(
            Expression<Func<OneSDataRecord, T>> selectExpression)
        {
            Contract.Requires<ArgumentNullException>(selectExpression != null);

            var part = SelectionVisitor.Parse(selectExpression);

            return new SelectPartInfo<T>(
                new SqlColumnListExpression(part.Columns),
                new NoSideEffectItemReaderFactory<T>(part.SelectionFunc));
        }
        
        #region Вспомогательные типы

        /// <summary>Струтура с информацией о выборке данных.</summary>
        /// <typeparam name="T">Тип читаемых данных.</typeparam>
        private struct SelectPartInfo<T>
        {
            public SelectPartInfo(SqlColumnSetExpression columns, IItemReaderFactory<T> itemReaderFactory) : this()
            {
                _statement = new SqlSelectStatement(columns);
                _itemReaderFactory = itemReaderFactory;
            }

            public SqlSelectStatement Statement
            {
                get { return _statement; }
            }
            private readonly SqlSelectStatement _statement;

            public IItemReaderFactory<T> ItemReaderFactory
            {
                get { return _itemReaderFactory; }
            }
            private readonly IItemReaderFactory<T> _itemReaderFactory;
        }

        #endregion
    }
}
