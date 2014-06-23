using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            // TODO Рефакторить
            // TODO Копипаста с SelectionTransformer
            
            var fields = new List<string>();
            var members = new List<MemberInfo>();

            var typeMapping = _mappingProvider.GetTypeMapping(typeof(T));
            foreach (var fieldMapping in typeMapping.FieldMappings)
            {
                fields.Add(fieldMapping.FieldName);
                members.Add(fieldMapping.MemberInfo);
            }

            return new SelectionPartParseProduct<T>(
                GetColumnExpressions(fields), 
                CreateReader<T>(members));
        }

        private static ReadOnlyCollection<SqlExpression> GetColumnExpressions(IEnumerable<string> fieldNames)
        {
            return new ReadOnlyCollection<SqlExpression>(
                fieldNames
                    .Select(f => (SqlExpression)new SqlFieldExpression(f))
                    .ToArray());
        }

        private static Func<IValueConverter, object[], T> CreateReader<T>(IEnumerable<MemberInfo> members)
        {
            var converterParameter = Expression.Parameter(typeof(IValueConverter));
            var valuesParameter = Expression.Parameter(typeof(object[]));

            return Expression.Lambda<Func<IValueConverter, object[], T>>(
                CreateReaderExpression(converterParameter, valuesParameter, typeof(T), members.ToArray()),
                converterParameter, valuesParameter)
                .Compile();
        }

        private static Expression CreateReaderExpression(
            ParameterExpression converterParameter,
            ParameterExpression valuesParameter, 
            Type type,
            IList<MemberInfo> members)
        {
            var createObjectExpression = Expression.New(type);
            var memberBindings = new List<MemberBinding>();

            for (var index = 0; index < members.Count; index++)
            {
                var memberBinding = Expression.Bind(
                    members[index],
                    CreateGetValueExpression(converterParameter, valuesParameter, GetMemberType(members[index]), index));
                memberBindings.Add(memberBinding);
            }

            return Expression.MemberInit(createObjectExpression, memberBindings);
        }

        private static Type GetMemberType(MemberInfo memberInfo)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.PropertyType;

            return ((FieldInfo)memberInfo).FieldType;
        }

        private static Expression CreateGetValueExpression(
            ParameterExpression converterParameter,
            ParameterExpression valuesParameter,
            Type memberType,
            int index)
        {
            var valueExpression = Expression.ArrayIndex(valuesParameter, Expression.Constant(index));

            return Expression.Call(
                converterParameter, 
                OneSQueryExpressionHelper.GetValueConvertMethod(memberType), 
                valueExpression);
        }
    }
}
