using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Преобразователь запроса в объект <see cref="ExpressionParseProduct"/>.</summary>
    internal sealed class QueryTransformer : IQueryTransformer
    {
        /// <summary>Часть запроса, отвечающая за выборку записи.</summary>
        private static readonly SelectPartInfo<OneSDataRecord>
            _recordSelectPart = new SelectPartInfo<OneSDataRecord>(
                SqlAllColumnsExpression.Instance, OneSDataRecordReaderFactory.Default);
        
        /// <summary>Контекст разбора запроса.</summary>
        private readonly QueryParseContext _context = new QueryParseContext();

        /// <summary>Конструктор использования.</summary>
        /// <param name="mappingProvider">Поставщик соответствий типам источников данных 1С.</param>
        public QueryTransformer(IOneSMappingProvider mappingProvider)
            : this(new ExpressionTransformMethods(mappingProvider))
        {}

        /// <summary>Конструктор для unit-тестирования.</summary>
        /// <param name="expressionTransformMethods">Преобразователь выражения выборки.</param>
        internal QueryTransformer(IExpressionTransformMethods expressionTransformMethods)
        {
            Contract.Requires<ArgumentNullException>(expressionTransformMethods != null);

            _expressionTransformMethods = expressionTransformMethods;
        }

        /// <summary>
        /// Методы преобразования выборки.
        /// </summary>
        internal IExpressionTransformMethods ExpressionTransformMethods
        {
            get { return _expressionTransformMethods; }
        }
        private readonly IExpressionTransformMethods _expressionTransformMethods;

        /// <summary>
        /// Преобразование запроса в результат 
        /// разбора выражения.
        /// </summary>
        /// <param name="query">Запрос.</param>
        public CollectionReadExpressionParseProduct<TOutput> Transform<TInput, TOutput>(IQuery<TInput, TOutput> query)
        {
            var selectPart = GetSelectPart(query);
            var source = query.Source.GetSourceName(_expressionTransformMethods);
            var whereStatement = ParseFilterExpression(query.Filter);
            var orderByStatement = ParseSortExpressions(query.Sorters);

            var queryStatement = new SqlQueryStatement(
                selectPart.GetStatement(query.IsDistinct), new SqlFromStatement(source), whereStatement, orderByStatement);

            return GetExpressionParseProduct(
                queryStatement, selectPart.ItemReaderFactory);
        }

        /// <summary>
        /// Получение информации по выборке.
        /// </summary>
        /// <typeparam name="TInput">Тип элементов входной последовательности.</typeparam>
        /// <typeparam name="TOutput">Тип элементов выходной последовательности запроса.</typeparam>
        /// <param name="query">Запрос.</param>
        private SelectPartInfo<TOutput> GetSelectPart<TInput, TOutput>(IQuery<TInput, TOutput> query)
        {
            SelectionPartParseProduct<TOutput> selectionPartParseProduct;
            
            if (query.Selector == null)
            {
                if (typeof(TOutput) == typeof(OneSDataRecord))
                    return (SelectPartInfo<TOutput>)(object)_recordSelectPart;

                selectionPartParseProduct = _expressionTransformMethods
                    .TransformSelectTypedRecord<TOutput>();
            }
            else
            {
                selectionPartParseProduct = _expressionTransformMethods
                    .TransformSelectExpression(_context, query.Selector);
            }

            return new SelectPartInfo<TOutput>(
                new SqlColumnListExpression(selectionPartParseProduct.Columns),
                new NoSideEffectItemReaderFactory<TOutput>(selectionPartParseProduct.SelectionFunc)
                );
        }

        /// <summary>
        /// Конструирование результата парсинга для запроса последовательности элементов.
        /// </summary>
        /// <typeparam name="T">Тип элементов выходной последовательности.</typeparam>
        /// <param name="queryStatement">Объект SQL-инструкции запроса.</param>
        /// <param name="itemReaderFactory">Фабрика читателей элементов.</param>
        private CollectionReadExpressionParseProduct<T> GetExpressionParseProduct<T>(
            SqlQueryStatement queryStatement,
            IItemReaderFactory<T> itemReaderFactory)
        {
            return new CollectionReadExpressionParseProduct<T>(
                new SqlCommand(
                        queryStatement.BuildSql(),
                        _context.Parameters.GetSqlParameters()),
                itemReaderFactory);
        }

        /// <summary>
        /// Парсинг и преобразование выражения фильтрации
        /// в часть where инструкции sql-запроса.
        /// </summary>
        /// <typeparam name="T">Тип элементов входной последовательности.</typeparam>
        /// <param name="filterExpression">Выражение фильтрации элементов.</param>
        private SqlWhereStatement ParseFilterExpression<T>(Expression<Func<T, bool>> filterExpression)
        {
            if (filterExpression == null)
                return null;

            return new SqlWhereStatement(
                _expressionTransformMethods.TransformWhereExpression(_context, filterExpression));
        }

        /// <summary>
        /// Парсинг и преобразование списка выражений соритровки
        /// в часть order by инструкции sql-запроса.
        /// </summary>
        /// <param name="sortExpressions">Список выражений сортировки.</param>
        private SqlOrderByStatement ParseSortExpressions(ReadOnlyCollection<SortExpression> sortExpressions)
        {
            Contract.Requires<ArgumentNullException>(sortExpressions != null);

            if (sortExpressions.Count == 0)
                return null;

            return new SqlOrderByStatement(
                new ReadOnlyCollection<SqlSortFieldExpression>(
                    sortExpressions
                        .Select(ParseSortExpression)
                        .ToArray()
                    ));
        }

        /// <summary>
        /// Парсинг и преобразование выражения сортировки
        /// в подвыражение order by инструкции sql-запроса.
        /// </summary>
        /// <param name="sortExpression">Выражение сортировки.</param>
        private SqlSortFieldExpression ParseSortExpression(SortExpression sortExpression)
        {
            Contract.Requires<ArgumentNullException>(sortExpression != null);

            return new SqlSortFieldExpression(
                _expressionTransformMethods.TransformOrderByExpression(_context, sortExpression.KeyExpression),
                sortExpression.Kind);
        }

        #region Вспомогательные типы

        /// <summary>Струтура с информацией о выборке данных.</summary>
        /// <typeparam name="T">Тип читаемых данных.</typeparam>
        private struct SelectPartInfo<T>
        {
            private readonly SqlColumnSetExpression _columns;

            public SelectPartInfo(SqlColumnSetExpression columns, IItemReaderFactory<T> itemReaderFactory) : this()
            {
                _columns = columns;
                _itemReaderFactory = itemReaderFactory;
            }

            /// <summary>
            /// SELECT часть инструкции SQL-запроса.
            /// </summary>
            public SqlSelectStatement GetStatement(bool isDistinct)
            {
                return new SqlSelectStatement(_columns, isDistinct);
            }

            /// <summary>
            /// Фабрика читателей элементов из последовательности.
            /// </summary>
            public IItemReaderFactory<T> ItemReaderFactory
            {
                get { return _itemReaderFactory; }
            }
            private readonly IItemReaderFactory<T> _itemReaderFactory;
        }

        #endregion
    }
}
