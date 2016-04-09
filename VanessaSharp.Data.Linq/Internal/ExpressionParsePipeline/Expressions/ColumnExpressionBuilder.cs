using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Построитель выражений для запроса колонок.</summary>
    internal sealed class ColumnExpressionBuilder
    {
        /// <summary>Список выражений колонок.</summary>
        private readonly List<SqlExpression> _columns = new List<SqlExpression>();

        /// <summary>Параметр для результирующего делегата создания элемента - конвертер значений.</summary>
        public ParameterExpression ConverterParameter
        {
            get
            {
                Contract.Ensures(Contract.Result<ParameterExpression>() != null);
                Contract.Ensures(Contract.Result<ParameterExpression>().Type == typeof(IValueConverter));

                return _converterParameter;
            }
        }
        private readonly ParameterExpression _converterParameter = Expression.Parameter(typeof(IValueConverter), "valueConverter");

        /// <summary>Параметр для результирующего делегата создания элемента - массив вычитанных значений.</summary>
        public ParameterExpression ValuesParameter
        {
            get
            {
                Contract.Ensures(Contract.Result<ParameterExpression>() != null);
                Contract.Ensures(Contract.Result<ParameterExpression>().Type == typeof(object[]));

                return _valuesParameter;
            }
        }
        private readonly ParameterExpression _valuesParameter = Expression.Parameter(typeof(object[]), "values");

        /// <summary>
        /// Получение выражения получения значения колонки, которое является полем записи.
        /// </summary>
        /// <param name="fieldName">Имя поля.</param>
        /// <param name="columnType">Тип, который требуется для колонки.</param>
        public Expression GetColumnAccessExpression(string fieldName, Type columnType)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fieldName));
            Contract.Requires<ArgumentNullException>(columnType != null);

            return GetColumnAccessExpression(
                new SqlFieldExpression(SqlDefaultTableExpression.Instance, fieldName),
                columnType);
        }

        /// <summary>
        /// Получение выражения получения значения колонки записи.
        /// </summary>
        /// <param name="columnExpression">Выражение колонки.</param>
        /// <param name="columnType">Тип, который требуется для колонки.</param>
        public Expression GetColumnAccessExpression(SqlExpression columnExpression, Type columnType)
        {
            Contract.Requires<ArgumentNullException>(columnExpression != null);
            Contract.Requires<ArgumentNullException>(columnType != null);

            if (columnType.IsAssignableFrom(typeof(OneSValue)))
            {
                var getOneSValueExpression = GetColumnAccessAndOneSValueExpression(columnExpression);
                return (columnType == typeof(OneSValue))
                    ? getOneSValueExpression
                    : Expression.Convert(getOneSValueExpression, columnType);
            }

            if (columnType == typeof(DayOfWeek))
            {
                return Expression.Call(
                    null,
                    OneSQueryExpressionHelper.DayOfWeekFromInt32Method,
                    GetColumnAccessExpression(columnExpression, typeof(int)));
            }

            if (columnType == typeof(Guid))
                return GetSimpleConvertExpression(columnExpression, columnType);

            //if (columnType == typeof(IEnumerable<OneSDataRecord>))
            //{
            //    var sqlResultReaderType = typeof(ISqlResultReader);

            //    return Expression.Convert(
            //            Expression.New(
            //                typeof(ItemEnumerable<OneSDataRecord>).GetConstructor(new[] { sqlResultReaderType, typeof(IItemReaderFactory<OneSDataRecord>) }),
            //                GetSimpleConvertExpression(columnExpression, sqlResultReaderType),
            //                Expression.Constant(OneSDataRecordReaderFactory.Default, typeof(IItemReaderFactory<OneSDataRecord>))),
            //            columnType);
            //}

            return GetColumnAccessAndConvertExpression(
                columnExpression,
                OneSQueryExpressionHelper.GetValueConvertMethod(columnType));
        }

        public Expression GetTablePartColumnAccessExpression(
            SqlExpression tablePartExpression, Type itemType, object itemReaderFactory)
        {
            Contract.Requires<ArgumentNullException>(tablePartExpression != null);
            Contract.Requires<ArgumentNullException>(itemType != null);
            Contract.Requires<ArgumentNullException>(itemReaderFactory != null);
            Contract.Requires<ArgumentException>(
                typeof(IItemReaderFactory<>).MakeGenericType(itemType).IsAssignableFrom(itemReaderFactory.GetType()));
            Contract.Ensures(Contract.Result<Expression>() != null);

            var sqlResultReaderType = typeof(ISqlResultReader);
            var enumerableType = typeof(ItemEnumerable<>).MakeGenericType(itemType);
            var itemReaderFactoryType = typeof(IItemReaderFactory<>).MakeGenericType(itemType);

            return Expression.New(
                enumerableType.GetConstructor(new[] {sqlResultReaderType, itemReaderFactoryType}),
                GetSimpleConvertExpression(tablePartExpression, sqlResultReaderType),
                Expression.Constant(itemReaderFactory, itemReaderFactoryType));
        }

        public Expression GetTablePartColumnAccessExpression<TItem>(
            SqlExpression tablePartExpression, IItemReaderFactory<TItem> itemReaderFactory)
        {
            Contract.Requires<ArgumentNullException>(tablePartExpression != null);
            Contract.Requires<ArgumentNullException>(itemReaderFactory != null);
            Contract.Ensures(Contract.Result<Expression>() != null);

            return GetTablePartColumnAccessExpression(tablePartExpression, typeof(TItem), itemReaderFactory);
        }

        /// <summary>Получение выражения простой конвертации значения.</summary>
        /// <param name="columnExpression">SQL-выражение.</param>
        /// <param name="columnType">Тип колонки.</param>
        private Expression GetSimpleConvertExpression(SqlExpression columnExpression, Type columnType)
        {
            return Expression.Convert(
                    GetValueExpression(columnExpression), columnType
                    );
        }

        /// <summary>
        /// Получение выражения получения значения колонки записи с конвертацией.
        /// </summary>
        /// <param name="columnExpression">Выражение колонки.</param>
        /// <param name="valueConverterMethod">Метод конвертации к нужному типу.</param>
        private Expression GetColumnAccessAndConvertExpression(SqlExpression columnExpression, MethodInfo valueConverterMethod)
        {
            return Expression.Call(
                ConverterParameter, 
                valueConverterMethod, 
                GetValueExpression(columnExpression));
        }

        /// <summary>
        /// Получение выражения получения значения <see cref="OneSValue"/> колонки записи.
        /// </summary>
        /// <param name="columnExpression">Выражение колонки.</param>
        private Expression GetColumnAccessAndOneSValueExpression(SqlExpression columnExpression)
        {
            return Expression.Call(
                null,
                OneSQueryExpressionHelper.OneSValueCreateMethod,
                GetValueExpression(columnExpression),
                ConverterParameter);
        }

        /// <summary>
        /// Получение выражения получения значения колонки записи.
        /// </summary>
        /// <param name="columnExpression">Выражение колонки.</param>
        private Expression GetValueExpression(SqlExpression columnExpression)
        {
            var index = GetColumnIndex(columnExpression);
            return Expression.ArrayIndex(ValuesParameter, Expression.Constant(index));
        }

        /// <summary>Получение индекса колонки по его выражению.</summary>
        /// <param name="columnExpression">Имя поля.</param>
        private int GetColumnIndex(SqlExpression columnExpression)
        {
            var index = _columns.IndexOf(columnExpression);
            if (index == -1)
                return AddNewColumn(columnExpression);

            return index;
        }

        /// <summary>Добавление вычитки новой колонки.</summary>
        private int AddNewColumn(SqlExpression columnExpression)
        {
            _columns.Add(columnExpression);

            return _columns.Count - 1;
        }

        /// <summary>Вычитываемые колонки.</summary>
        public ReadOnlyCollection<SqlExpression> Columns
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<SqlExpression>>() != null);
                
                return new ReadOnlyCollection<SqlExpression>(_columns);
            }
        }
    }
}
