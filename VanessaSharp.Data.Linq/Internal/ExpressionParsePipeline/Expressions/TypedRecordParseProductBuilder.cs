using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Построитель конструкций запроса для типизированных записей.</summary>
    internal sealed class TypedRecordParseProductBuilder
    {
        private readonly IOneSMappingProvider _mappingProvider;

        /// <summary>Конструктор.</summary>
        /// <param name="mappingProvider">
        /// Поставщик соответствия типов CLR и источников данных 1С.
        /// </param>
        public TypedRecordParseProductBuilder(IOneSMappingProvider mappingProvider)
        {
            _mappingProvider = mappingProvider;
        }

        /// <summary>Получение имени источника данных для типизированной записи.</summary>
        /// <typeparam name="T">Тип записи.</typeparam>
        public string GetTypedRecordSourceName<T>()
        {
            return _mappingProvider.GetTypeMapping(typeof(T)).SourceName;
        }

        /// <summary>Преобразование получения типизированных записей.</summary>
        /// <typeparam name="T">Тип записей.</typeparam>
        public SelectionPartParseProduct<T> GetSelectPartParseProductForTypedRecord<T>()
        {
            var columnBuilder = new ColumnExpressionBuilder();

            var readerLambda = Expression.Lambda<Func<IValueConverter, object[], T>>(
                GetReaderExpression(SqlDefaultTableExpression.Instance, columnBuilder, typeof(T)), 
                columnBuilder.ConverterParameter, 
                columnBuilder.ValuesParameter);

            return new SelectionPartParseProduct<T>(
                columnBuilder.Columns,
                readerLambda.Compile());
        }

        /// <summary>
        /// Получение выражения читателя объекта данных.
        /// </summary>
        /// <param name="table">Выражение определяющее таблицу.</param>
        /// <param name="columnBuilder">Построитель выражений доступа к колонкам читателя данных.</param>
        /// <param name="dataObjectType">Тип объекта данных.</param>
        public Expression GetReaderExpression(
            SqlExpression table,
            ColumnExpressionBuilder columnBuilder,
            Type dataObjectType)
        {
            Contract.Requires<ArgumentNullException>(table != null);
            Contract.Requires<ArgumentNullException>(columnBuilder != null);
            Contract.Requires<ArgumentNullException>(dataObjectType != null);
            Contract.Ensures(Contract.Result<Expression>() != null);

            return GetReaderExpression(
                table,
                columnBuilder,
                dataObjectType,
                _mappingProvider.GetTypeMapping(dataObjectType).FieldMappings);
        }


        private static Expression GetReaderExpression(
            SqlExpression table,
            ColumnExpressionBuilder columnBuilder,
            Type type,
            IEnumerable<OneSFieldMapping> fieldMappings)
        {
            return Expression.MemberInit(
                Expression.New(type), 
                fieldMappings
                    .Select(fm => GetMemberBinding(table, columnBuilder, fm))
                    .ToArray());
        }

        private static MemberBinding GetMemberBinding(
            SqlExpression table,
            ColumnExpressionBuilder columnBuilder,
            OneSFieldMapping fieldMapping)
        {
            var fieldExpression = new SqlFieldExpression(table, fieldMapping.FieldName);
            
            return Expression.Bind(
                fieldMapping.MemberInfo,
                columnBuilder.GetColumnAccessExpression(
                    fieldExpression,
                    fieldMapping.MemberInfo.GetMemberType()));
        }
    }
}
