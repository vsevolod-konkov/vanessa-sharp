﻿using System;
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
        internal IOneSMappingProvider MappingProvider
        {
            get { return _mappingProvider; }    
        }
        private readonly IOneSMappingProvider _mappingProvider;

        /// <summary>Построитель конструкций запроса для типизированных записей.</summary>
        private readonly TypedRecordParseProductBuilder _typedRecordParseProductBuilder;

        /// <summary>Преобразование LINQ-выражения метода Select.</summary>
        /// <typeparam name="TInput">Тип элементов исходной последовательности.</typeparam>
        /// <typeparam name="TOutput">Тип элементов выходной последовательности - результатов выборки.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="selectExpression">Преобразуемое выражение.</param>
        public SelectionPartParseProduct<TOutput> TransformSelectExpression<TInput, TOutput>(
            QueryParseContext context, Expression<Func<TInput, TOutput>> selectExpression)
        {
            return SelectExpressionTransformer.Transform(_mappingProvider, context, selectExpression, OneSDataLevel.Root);
        }

        /// <summary>Преобразование выражения в SQL-условие WHERE.</summary>
        /// <typeparam name="T">Тип элементов.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="predicateExpression">Выражение фильтрации.</param>
        public SqlCondition TransformCondition<T>(
            QueryParseContext context, Expression<Func<T, bool>> predicateExpression)
        {
            return ConditionTransformer.Transform(_mappingProvider, context, predicateExpression);
        }

        /// <summary>Преобразование выражения получения ключа сортировки в SQL-выражения поля под выражением ORDER BY.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="expression">Выражение ключа сортировки.</param>
        public SqlExpression TransformExpression(
            QueryParseContext context, LambdaExpression expression)
        {
            return ExpressionTransformer.Transform(_mappingProvider, context, expression);
        }

        /// <summary>Получение имени источника данных для типизированной записи.</summary>
        /// <typeparam name="T">Тип записи.</typeparam>
        public string ResolveSourceNameForTypedRecord<T>()
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