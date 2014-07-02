﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Построитель выражений для запроса колонок.</summary>
    internal sealed class ColumnExpressionBuilder : IFieldAccessVisitor
    {
        /// <summary>Список выражений колонок.</summary>
        private readonly List<SqlExpression> _columns = new List<SqlExpression>();

        /// <summary>Параметр для результирующего делегата создания элемента - конвертер значений.</summary>
        public ParameterExpression ConverterParameter
        {
            get
            {
                Contract.Ensures(Contract.Result<ParameterExpression>() != null);

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
                new SqlFieldExpression(fieldName),
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

            return GetColumnAccessExpression(
                columnExpression,
                OneSQueryExpressionHelper.GetValueConvertMethod(columnType));
        }

        /// <summary>
        /// Получение выражения получения значения колонки записи.
        /// </summary>
        /// <param name="columnExpression">Выражение колонки.</param>
        /// <param name="valueConverterMethod">Метод конвертации к нужному типу.</param>
        private Expression GetColumnAccessExpression(SqlExpression columnExpression, MethodInfo valueConverterMethod)
        {
            var index = GetColumnIndex(columnExpression);

            var valueExpression = Expression.ArrayIndex(ValuesParameter, Expression.Constant(index));
            return Expression.Call(ConverterParameter, valueConverterMethod, valueExpression);
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

        /// <summary>Посещение узла доступа к полю записи.</summary>
        /// <param name="fieldExpression">SQL-Выражение поля.</param>
        /// <param name="fieldType">Тип поля.</param>
        Expression IFieldAccessVisitor.VisitFieldAccess(SqlFieldExpression fieldExpression, Type fieldType)
        {
            return GetColumnAccessExpression(fieldExpression, fieldType);
        }
    }
}
