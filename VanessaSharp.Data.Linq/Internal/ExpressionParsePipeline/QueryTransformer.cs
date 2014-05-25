using System;
using System.Collections.ObjectModel;
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

            var context = new QueryParseContext();

            return GetExpressionParseProduct(
                context,
                query,
                ParseSelectExpression(context, query.SelectExpression));
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
                new QueryParseContext(),
                query,
                _recordSelectPart);
        }

        private static CollectionReadExpressionParseProduct<T> GetExpressionParseProduct<T>(
            QueryParseContext context,
            SimpleQuery query, SelectPartInfo<T> selectPart)
        {
            var whereStatement = ParseFilterExpression(context, query.Filter);

            return GetExpressionParseProduct(
                context.Parameters.GetSqlParameters(),
                new SqlFromStatement(query.Source), 
                whereStatement, 
                selectPart);
        }

        private static CollectionReadExpressionParseProduct<T> GetExpressionParseProduct<T>(
            ReadOnlyCollection<SqlParameter> parameters,
            SqlFromStatement fromStatement, 
            SqlWhereStatement whereStatement, 
            SelectPartInfo<T> selectPart)
        {
            var queryStatement = new SqlQueryStatement(
                                        selectPart.Statement,
                                        fromStatement, 
                                        whereStatement);

            return new CollectionReadExpressionParseProduct<T>(
                new SqlCommand(
                    queryStatement.BuildSql(),
                    parameters),
                selectPart.ItemReaderFactory);
        }

        private static SelectPartInfo<T> ParseSelectExpression<T>(QueryParseContext context, Expression<Func<OneSDataRecord, T>> selectExpression)
        {
            Contract.Requires<ArgumentNullException>(selectExpression != null);
            Contract.Requires<ArgumentNullException>(context != null);

            var part = SelectExpressionTransformer.Transform(context, selectExpression);

            return new SelectPartInfo<T>(
                new SqlColumnListExpression(part.Columns),
                new NoSideEffectItemReaderFactory<T>(part.SelectionFunc));
        }

        private static SqlWhereStatement ParseFilterExpression(QueryParseContext context, Expression<Func<OneSDataRecord, bool>> filterExpression)
        {
            Contract.Requires<ArgumentNullException>(context != null);

            if (filterExpression == null)
                return null;

            return new SqlWhereStatement(
                WhereExpressionTransformer.Transform(context, filterExpression));
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
