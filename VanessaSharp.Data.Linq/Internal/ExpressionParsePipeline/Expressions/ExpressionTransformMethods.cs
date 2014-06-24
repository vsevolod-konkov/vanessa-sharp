using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Стандартная реализация <see cref="IExpressionTransformMethods"/>.
    /// Методов преобразования LINQ-выражений методов-запросов.
    /// </summary>
    internal sealed class ExpressionTransformMethods : IExpressionTransformMethods
    {
        /// <summary>Конструктор.</summary>
        /// <param name="mappingProvider">
        /// Поставщик соответствий типов источникам данных.
        /// </param>
        public ExpressionTransformMethods(IOneSMappingProvider mappingProvider)
        {
            Contract.Requires<ArgumentNullException>(mappingProvider != null);

            _mappingProvider = mappingProvider;
            _typedRecordParseProductBuilder = new TypedRecordParseProductBuilder(mappingProvider);
        }

        /// <summary>
        /// Поставщик соответствий типам источников данных 1С.
        /// </summary>
        private readonly IOneSMappingProvider _mappingProvider;

        /// <summary>Построитель конструкций запроса для типизированных записей.</summary>
        private readonly TypedRecordParseProductBuilder _typedRecordParseProductBuilder;
        
        /// <summary>Преобразование LINQ-выражения метода Select.</summary>
        /// <typeparam name="T">Тип элементов последовательности - результатов выборки.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="selectExpression">Преобразуемое выражение.</param>
        public SelectionPartParseProduct<T> TransformSelectExpression<T>(
            QueryParseContext context, Expression<Func<OneSDataRecord, T>> selectExpression)
        {
            return SelectExpressionTransformer.Transform(context, selectExpression);
        }

        /// <summary>Преобразование выражения в SQL-условие WHERE.</summary>
        /// <typeparam name="T">Тип элементов.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="filterExpression">Выражение фильтрации.</param>
        public SqlCondition TransformWhereExpression<T>(
            QueryParseContext context, Expression<Func<T, bool>> filterExpression)
        {
            return WhereExpressionTransformer.Transform(_mappingProvider, context, filterExpression);
        }

        /// <summary>Преобразование выражения получения ключа сортировки в SQL-выражения поля под выражением ORDER BY.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="sortKeyExpression">Выражение ключа сортировки.</param>
        public SqlFieldExpression TransformOrderByExpression(
            QueryParseContext context, LambdaExpression sortKeyExpression)
        {
            return OrderByExpressionTransformer.Transform(_mappingProvider, context, sortKeyExpression);
        }

        /// <summary>Получение имени источника данных для типизированной записи.</summary>
        /// <typeparam name="T">Тип записи.</typeparam>
        public string GetTypedRecordSourceName<T>()
        {
            return _typedRecordParseProductBuilder.GetTypedRecordSourceName<T>();
        }

        /// <summary>Преобразование получения типизированных записей.</summary>
        /// <typeparam name="T">Тип записей.</typeparam>
        public SelectionPartParseProduct<T> TransformSelectTypedRecord<T>()
        {
            return _typedRecordParseProductBuilder.GetSelectPartParseProductForTypedRecord<T>();
        }
    }
}