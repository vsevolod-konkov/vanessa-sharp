using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
                GetReaderBody(columnBuilder, typeof(T)), 
                columnBuilder.ConverterParameter, 
                columnBuilder.ValuesParameter);

            return new SelectionPartParseProduct<T>(
                columnBuilder.Columns,
                readerLambda.Compile());
        }

        private Expression GetReaderBody(
            ColumnExpressionBuilder columnBuilder,
            Type type)
        {
            return GetReaderBody(
                columnBuilder, 
                type, 
                _mappingProvider.GetTypeMapping(type).FieldMappings);
        }

        private static Expression GetReaderBody(
            ColumnExpressionBuilder columnBuilder,
            Type type,
            IEnumerable<OneSFieldMapping> fieldMappings)
        {
            return Expression.MemberInit(
                Expression.New(type), 
                fieldMappings
                    .Select(fm => GetMemberBinding(columnBuilder, fm))
                    .ToArray());
        }

        private static MemberBinding GetMemberBinding(
            ColumnExpressionBuilder columnBuilder,
            OneSFieldMapping fieldMapping)
        {
            return Expression.Bind(
                fieldMapping.MemberInfo,
                columnBuilder.GetColumnAccessExpression(
                    fieldMapping.FieldName,
                    fieldMapping.MemberInfo.GetMemberType()));

        }
    }
}
