using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            return _mappingProvider.GetRootTypeMapping(typeof(T)).SourceName;
        }

        /// <summary>Преобразование получения типизированных записей.</summary>
        /// <typeparam name="T">Тип записей.</typeparam>
        public SelectionPartParseProduct<T> GetSelectPartParseProductForTypedRecord<T>()
        {
            var columnBuilder = new ColumnExpressionBuilder();

            var readerLambda = Expression.Lambda<Func<IValueConverter, object[], T>>(
                GetRootReaderExpression(SqlDefaultTableExpression.Instance, columnBuilder, typeof(T)), 
                columnBuilder.ConverterParameter, 
                columnBuilder.ValuesParameter);

            return new SelectionPartParseProduct<T>(
                columnBuilder.Columns,
                readerLambda.Compile());
        }

        private ISelectionPartParseProduct GetSelectPartParseProductForTypedRecord(Type type, OneSDataLevel level)
        {
            _mappingProvider.CheckDataType(level, type);

            var delegateType = typeof(Func<,,>).MakeGenericType(typeof(IValueConverter), typeof(object[]), type);

            var columnBuilder = new ColumnExpressionBuilder();

            var readerLambda = Expression.Lambda(
                delegateType,
                GetReaderExpression(SqlDefaultTableExpression.Instance, columnBuilder, type, level),
                columnBuilder.ConverterParameter,
                columnBuilder.ValuesParameter);

            var resultType = typeof(SelectionPartParseProduct<>).MakeGenericType(type);

            return (ISelectionPartParseProduct)Activator.CreateInstance(resultType, columnBuilder.Columns, readerLambda.Compile());
        }

        /// <summary>
        /// Получение выражения читателя объекта данных.
        /// </summary>
        /// <param name="table">Выражение определяющее таблицу.</param>
        /// <param name="columnBuilder">Построитель выражений доступа к колонкам читателя данных.</param>
        /// <param name="dataObjectType">Тип объекта данных.</param>
        public Expression GetRootReaderExpression(SqlExpression table, ColumnExpressionBuilder columnBuilder, Type dataObjectType)
        {
            Contract.Requires<ArgumentNullException>(table != null);
            Contract.Requires<ArgumentNullException>(columnBuilder != null);
            Contract.Requires<ArgumentNullException>(dataObjectType != null);
            Contract.Ensures(Contract.Result<Expression>() != null);

            return GetReaderExpression(
                table,
                columnBuilder,
                dataObjectType,
                OneSDataLevel.Root);
        }

        private IEnumerable<OneSFieldMapping> GetFieldMappings(OneSDataLevel level, Type dataObjectType)
        {
            return (level == OneSDataLevel.Root)
                       ? _mappingProvider.GetRootTypeMapping(dataObjectType).FieldMappings
                       : _mappingProvider.GetTablePartTypeMappings(dataObjectType);
        }

        private Expression GetReaderExpression(
            SqlExpression table,
            ColumnExpressionBuilder columnBuilder,
            Type type,
            OneSDataLevel level)
        {
            return GetReaderExpression(table, columnBuilder, type, GetFieldMappings(level, type));
        }

        private Expression GetReaderExpression(
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

        private MemberBinding GetMemberBinding(
            SqlExpression table,
            ColumnExpressionBuilder columnBuilder,
            OneSFieldMapping fieldMapping)
        {
            var field = new SqlFieldExpression(table, fieldMapping.FieldName);
            var columnExpression = GetColumnAccessExpression(
                field, fieldMapping.MemberInfo, fieldMapping.DataColumnKind, columnBuilder);

            return Expression.Bind(
                    fieldMapping.MemberInfo,
                    columnExpression);    
        }

        private Expression GetColumnAccessExpression(
            SqlFieldExpression field, MemberInfo memberInfo, OneSDataColumnKind kind, ColumnExpressionBuilder columnBuilder)
        {
            if (kind == OneSDataColumnKind.TablePart)
            {
                var memberType = memberInfo.GetMemberType();
                Type itemType;
                if (!OneSQueryExpressionHelper.IsEnumerable(memberType, out itemType))
                {
                    throw new NotSupportedException(string.Format(
                        "{0} является членом вида табличной части. Для данного вида тип {1} не поддерживается. Поддерживается только обобщенный тип {2}",
                        memberInfo, memberType, typeof(IEnumerable<>)));
                }

                var parseProduct = GetSelectPartParseProductForTypedRecord(itemType, OneSDataLevel.TablePart);
                return parseProduct.GetTablePartColumnAccessExpression(field, columnBuilder);
            }
            else
            {
                return columnBuilder.GetColumnAccessExpression(
                        field,
                        memberInfo.GetMemberType());
            }
        }
    }
}
