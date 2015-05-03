using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Фабрика создания объекта запроса.</summary>
    internal static class QueryFactory
    {
        /// <summary>Создание запроса последовательности <see cref="OneSDataRecord"/>.</summary>
        /// <param name="sourceName">Имя источника.</param>
        /// <param name="filter">Выражение фильтрации.</param>
        /// <param name="sorters">Коллекция выражений сортировки.</param>
        /// <param name="isDistinct">Выборка различных.</param>
        /// <param name="maxCount">Максимальное количество строк.</param>
        /// <returns>Созданный запрос.</returns>
        public static IQuery CreateQuery(string sourceName, LambdaExpression filter,
                                         ReadOnlyCollection<SortExpression> sorters,
                                         bool isDistinct,
                                         int? maxCount)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));
            
            Contract.Requires<ArgumentException>(
                (filter == null) || IsInputTypeForLambda(filter, typeof(OneSDataRecord)));

            Contract.Requires<ArgumentNullException>(sorters != null);
            Contract.Requires<ArgumentException>(
                sorters.All(s => IsInputTypeForLambda(s.KeyExpression, typeof(OneSDataRecord))));

            return new Query<OneSDataRecord, OneSDataRecord>(
                new ExplicitSourceDescription(sourceName),
                null,
                (Expression<Func<OneSDataRecord, bool>>)filter,
                sorters, isDistinct, maxCount);
        }

        /// <summary>Создание запроса последовательности элементов проекции.</summary>
        private static IQuery CreateQuery(Type outputType, string sourceName, LambdaExpression selector,
                                          LambdaExpression filter, ReadOnlyCollection<SortExpression> sorters,
                                          bool isDistinct, int? maxCount)
        {
            var type = typeof(Query<,>).MakeGenericType(typeof(OneSDataRecord), outputType);

            return (IQuery)Activator.CreateInstance(type, new ExplicitSourceDescription(sourceName), selector, filter, sorters, isDistinct, maxCount);
        }

        /// <summary>Создание запроса последовательности проекций из <see cref="OneSDataRecord"/>.</summary>
        /// <param name="sourceName">Имя источника.</param>
        /// <param name="selector">Выражение выборки - проекции <see cref="OneSDataRecord"/>.</param>
        /// <param name="filter">Выражение фильтрации.</param>
        /// <param name="sorters">Коллекция выражений сортировки.</param>
        /// <param name="isDistinct">Выборка различных.</param>
        /// <param name="maxCount">Максимальное количество строк.</param>
        /// <returns>Созданный запрос.</returns>
        public static IQuery CreateQuery(string sourceName, LambdaExpression selector, LambdaExpression filter,
                                         ReadOnlyCollection<SortExpression> sorters, bool isDistinct, int? maxCount)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));

            Contract.Requires<ArgumentNullException>(selector != null);
            Contract.Requires<ArgumentException>(IsInputTypeForLambda(selector, typeof(OneSDataRecord)));

            Contract.Requires<ArgumentException>(
                (filter == null) || IsInputTypeForLambda(filter, typeof(OneSDataRecord)));

            Contract.Requires<ArgumentNullException>(sorters != null);
            Contract.Requires<ArgumentException>(
                sorters.All(s => IsInputTypeForLambda(s.KeyExpression, typeof(OneSDataRecord))));

            var outputType = selector.Type.GetGenericArguments()[1];

            return CreateQuery(outputType, sourceName, selector, filter, sorters, isDistinct, maxCount);
        }

        /// <summary>Создание запроса для типизированных записей.</summary>
        private static IQuery CreateQuery(Type inputType, Type outputType,
                                          LambdaExpression selector, LambdaExpression filter,
                                          ReadOnlyCollection<SortExpression> sorters,
                                          bool isDistinct, int? maxCount)
        {
            var type = typeof(Query<,>).MakeGenericType(inputType, outputType);
            return (IQuery)Activator.CreateInstance(type, selector, filter, sorters, isDistinct, maxCount);
        }

        /// <summary>Создание запроса последовательности элементов типизированных записей.</summary>
        /// <param name="itemType">Тип элементов типизированных записей.</param>
        /// <param name="filter">Выражение фильтрации записей.</param>
        /// <param name="sorters">Выражения сортировки записей.</param>
        /// <param name="isDistinct">Выборка различных.</param>
        /// <param name="maxCount">Максимальное количество строк.</param>
        /// <returns>Созданный запрос.</returns>
        public static IQuery CreateQuery(Type itemType, LambdaExpression filter,
                                         ReadOnlyCollection<SortExpression> sorters,
                                         bool isDistinct, int? maxCount)
        {
            Contract.Requires<ArgumentNullException>(itemType != null);
            Contract.Requires<ArgumentNullException>(itemType != typeof(OneSDataRecord));

            Contract.Requires<ArgumentException>(
                (filter == null) || IsInputTypeForLambda(filter, itemType));

            Contract.Requires<ArgumentNullException>(sorters != null);
            Contract.Requires<ArgumentException>(
                sorters.All(s => IsInputTypeForLambda(s.KeyExpression, itemType)));

            return CreateQuery(itemType, itemType, null, filter, sorters, isDistinct, maxCount);
        }

        /// <summary>Создание запроса последовательности элементов проекций типизированных записей.</summary>
        /// <param name="selector">Выражение выборки типизированных записей.</param>
        /// <param name="filter">Выражение фильтрации типизированных записей.</param>
        /// <param name="sorters">Выражения сортировки типизированных записей.</param>
        /// <param name="isDistinct">Выборка различных.</param>
        /// <param name="maxCount">Максимальное количество строк.</param>
        /// <returns>Созданный запрос.</returns>
        public static IQuery CreateQuery(LambdaExpression selector, LambdaExpression filter,
                                         ReadOnlyCollection<SortExpression> sorters,
                                         bool isDistinct, int? maxCount)
        {
            Contract.Requires<ArgumentNullException>(selector != null);
            Contract.Requires<ArgumentException>(
                selector.Type.IsGenericType
                && selector.Type.GetGenericTypeDefinition() == typeof(Func<,>)
                && selector.Type.GetGenericArguments()[0] != typeof(OneSDataRecord));

            Contract.Requires<ArgumentException>(
                (filter == null) || IsInputTypeForLambda(filter, selector.Type.GetGenericArguments()[0]));

            Contract.Requires<ArgumentNullException>(sorters != null);
            Contract.Requires<ArgumentException>(
                sorters.All(s => IsInputTypeForLambda(s.KeyExpression, selector.Type.GetGenericArguments()[0])));

            var selectorArgs = selector.Type.GetGenericArguments();
            return CreateQuery(selectorArgs[0], selectorArgs[1], selector, filter, sorters, isDistinct, maxCount);
        }

        private static IQuery CreateScalarQuery(
            Type inputItemType, Type outputItemType, Type scalarType,
            LambdaExpression selector, LambdaExpression filter,
            ReadOnlyCollection<SortExpression> sorters, bool isDistinct,
            AggregateFunction aggregateFunction)
        {
            var type = typeof(ScalarQuery<,,>).MakeGenericType(inputItemType, outputItemType, scalarType);
            return (IQuery)Activator.CreateInstance(type, selector, filter, sorters, isDistinct, aggregateFunction);
        }

        private static IQuery CreateScalarQuery(
            string sourceName, Type outputItemType, Type scalarType,
            LambdaExpression selector, LambdaExpression filter,
            ReadOnlyCollection<SortExpression> sorters, bool isDistinct,
            AggregateFunction aggregateFunction)
        {
            var type = typeof(ScalarQuery<,,>).MakeGenericType(typeof(OneSDataRecord), outputItemType, scalarType);
            return (IQuery)Activator.CreateInstance(type, new ExplicitSourceDescription(sourceName), selector, filter, sorters, isDistinct, aggregateFunction);
        }

        /// <summary>Создание скалярного запроса.</summary>
        /// <param name="sourceName">Имя источника данных.</param>
        /// <param name="filter">Выражение фильтрации записей.</param>
        /// <param name="sorters">Выражения сортировки записей.</param>
        /// <param name="scalarType">Тип скалярного значения.</param>
        /// <returns>Созданный запрос.</returns>
        public static IQuery CreateCountQuery(
            string sourceName, LambdaExpression filter,
            ReadOnlyCollection<SortExpression> sorters,
            Type scalarType)
        {
            Contract.Requires<ArgumentException>(
                (filter == null) || IsInputTypeForLambda(filter, typeof(OneSDataRecord)));

            Contract.Requires<ArgumentNullException>(sorters != null);
            Contract.Requires<ArgumentException>(
                sorters.All(s => IsInputTypeForLambda(s.KeyExpression, typeof(OneSDataRecord))));


            return CreateScalarQuery(
                sourceName, typeof(OneSDataRecord), scalarType, null, filter, sorters, false, AggregateFunction.Count);
        }

        /// <summary>Создание скалярного запроса.</summary>
        /// <param name="itemType">Тип элементов последовательности.</param>
        /// <param name="filter">Выражение фильтрации записей.</param>
        /// <param name="sorters">Выражения сортировки записей.</param>
        /// <param name="scalarType">Тип скалярного значения.</param>
        /// <returns>Созданный запрос.</returns>
        public static IQuery CreateCountQuery(
            Type itemType, LambdaExpression filter,
            ReadOnlyCollection<SortExpression> sorters,
            Type scalarType)
        {
            Contract.Requires<ArgumentException>(
                (filter == null) || IsInputTypeForLambda(filter, itemType));

            Contract.Requires<ArgumentNullException>(sorters != null);
            Contract.Requires<ArgumentException>(
                sorters.All(s => IsInputTypeForLambda(s.KeyExpression, itemType)));


            return CreateScalarQuery(
                itemType, itemType, scalarType, null, filter, sorters, false, AggregateFunction.Count);
        }
        
        /// <summary>Создание скалярного запроса.</summary>
        /// <param name="selector">Выражение выборки записей.</param>
        /// <param name="filter">Выражение фильтрации записей.</param>
        /// <param name="sorters">Выражения сортировки записей.</param>
        /// <param name="isDistinct">Выборка различных.</param>
        /// <param name="aggregateFunction">Функция агрегирования.</param>
        /// <param name="scalarType">Тип скалярного значения.</param>
        /// <returns>Созданный запрос.</returns>
        public static IQuery CreateScalarQuery(
            LambdaExpression selector, LambdaExpression filter,
            ReadOnlyCollection<SortExpression> sorters,
            bool isDistinct,
            AggregateFunction aggregateFunction,
            Type scalarType)
        {
            Contract.Requires<ArgumentNullException>(aggregateFunction == AggregateFunction.Count || selector != null);
            Contract.Requires<ArgumentNullException>(aggregateFunction == AggregateFunction.Count || !isDistinct);
            Contract.Requires<ArgumentException>(
                selector.Type.IsGenericType
                && selector.Type.GetGenericTypeDefinition() == typeof(Func<,>)
                && selector.Type.GetGenericArguments()[0] != typeof(OneSDataRecord));
            Contract.Requires<ArgumentException>(
                (filter == null) || IsInputTypeForLambda(filter, selector.Type.GetGenericArguments()[0]));

            Contract.Requires<ArgumentNullException>(sorters != null);
            Contract.Requires<ArgumentException>(
                sorters.All(s => IsInputTypeForLambda(s.KeyExpression, selector.Type.GetGenericArguments()[0])));

            return CreateScalarQuery(
                selector.Type.GetGenericArguments()[0],
                selector.Type.GetGenericArguments()[1],
                scalarType,

                selector, filter, sorters, isDistinct, aggregateFunction);
        }

        /// <summary>Создание скалярного запроса.</summary>
        /// <param name="sourceName">Имя источника.</param>
        /// <param name="selector">Выражение выборки записей.</param>
        /// <param name="filter">Выражение фильтрации записей.</param>
        /// <param name="sorters">Выражения сортировки записей.</param>
        /// <param name="isDistinct">Выборка различных.</param>
        /// <param name="aggregateFunction">Функция агрегирования.</param>
        /// <param name="scalarType">Тип скалярного значения.</param>
        /// <returns>Созданный запрос.</returns>
        public static IQuery CreateScalarQuery(
            string sourceName, LambdaExpression selector, LambdaExpression filter,
            ReadOnlyCollection<SortExpression> sorters, bool isDistinct,
            AggregateFunction aggregateFunction, Type scalarType)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));

            Contract.Requires<ArgumentNullException>(aggregateFunction == AggregateFunction.Count || selector != null);
            Contract.Requires<ArgumentNullException>(aggregateFunction == AggregateFunction.Count || !isDistinct);
            
            Contract.Requires<ArgumentException>(
                (selector == null) || IsInputTypeForLambda(selector, typeof(OneSDataRecord)));

            Contract.Requires<ArgumentException>(
                (filter == null) || IsInputTypeForLambda(filter, typeof(OneSDataRecord)));

            Contract.Requires<ArgumentNullException>(sorters != null);
            Contract.Requires<ArgumentException>(
                sorters.All(s => IsInputTypeForLambda(s.KeyExpression, typeof(OneSDataRecord))));

            var itemInputType = selector.Type.GetGenericArguments()[0];
            var itemOutputType = selector.Type.GetGenericArguments()[1];

            var type = typeof(ScalarQuery<,,>).MakeGenericType(itemInputType, itemOutputType, scalarType);
            return (IQuery)Activator.CreateInstance(type, new ExplicitSourceDescription(sourceName), selector, filter, sorters, isDistinct, aggregateFunction);
        }

        /// <summary>
        /// Проверка того, что в выражении выборки входной тип является заданным <paramref name="type"/>.
        /// </summary>
        /// <param name="lambda">Проверяемое выражение.</param>
        /// <param name="type">Требуемый входной тип.</param>
        /// <returns>
        /// Возвращает <c>true</c>, если выражение является выражением делегата <see cref="Func{TInput,TOutput}"/>
        /// и что первый тип-параметр делегата равен <paramref name="type"/>.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        [Pure]
        public static bool IsInputTypeForLambda(LambdaExpression lambda, Type type)
        {
            var lambdaType = lambda.Type;

            return lambdaType.IsGenericType
                   && lambdaType.GetGenericTypeDefinition() == typeof (Func<,>)
                   && lambdaType.GetGenericArguments()[0] == type;
        }

        /// <summary>Запрос типизированных кортежей.</summary>
        /// <typeparam name="TInput">Тип элементов входящей последовательности.</typeparam>
        /// <typeparam name="TOutput">Тип элементов выходящей последовательности.</typeparam>
        private sealed class Query<TInput, TOutput> : QueryBase<TInput, TOutput>
        {
            /// <summary>Конструктор.</summary>
            /// <param name="selector">Выражение выборки.</param>
            /// <param name="filter">Выражение фильтрации.</param>
            /// <param name="sorters">Выражения сортировки.</param>
            /// <param name="isDistinct">Выборка различных.</param>
            /// <param name="maxCount">Максимальное количество строк.</param>
            public Query(Expression<Func<TInput, TOutput>> selector,
                         Expression<Func<TInput, bool>> filter,
                         ReadOnlyCollection<SortExpression> sorters,
                         bool isDistinct,
                         int? maxCount)
            : this(SourceDescriptionByType<TInput>.Instance, selector, filter, sorters, isDistinct, maxCount)
            {
            }

            /// <summary>Конструктор.</summary>
            /// <param name="source">Описание источника данных.</param>
            /// <param name="selector">Выражение выборки.</param>
            /// <param name="filter">Выражение фильтрации.</param>
            /// <param name="sorters">Выражения сортировки.</param>
            /// <param name="isDistinct">Выборка различных.</param>
            /// <param name="maxCount">Максимальное количество строк.</param>
            public Query(
                ISourceDescription source,
                Expression<Func<TInput, TOutput>> selector,
                Expression<Func<TInput, bool>> filter,
                ReadOnlyCollection<SortExpression> sorters,
                bool isDistinct,
                int? maxCount)
                : base(source, selector, filter, sorters, isDistinct, maxCount)
            {}

            /// <summary>Преобразование результат парсинга запроса, готового к выполенению.</summary>
            protected override ExpressionParseProduct Transform(IQueryTransformer transformer)
            {
                return transformer.Transform(this);
            }
        }

        private sealed class ScalarQuery<TItemInput, TItemOutput, TResult>
            : QueryBase<TItemInput, TItemOutput>, IScalarQuery<TItemInput, TItemOutput, TResult>
        {
            public ScalarQuery(
                ISourceDescription source,
                Expression<Func<TItemInput, TItemOutput>> selector,
                Expression<Func<TItemInput, bool>> filter,
                ReadOnlyCollection<SortExpression> sorters,
                bool isDistinct,
                AggregateFunction aggregateFunction)
                : base(source, selector, filter, sorters, isDistinct, null)
            {
                Contract.Requires<ArgumentNullException>(
                    aggregateFunction == AggregateFunction.Count ||
                    selector != null);

                Contract.Requires<ArgumentNullException>(
                    aggregateFunction == AggregateFunction.Count ||
                    !isDistinct);
                
                _aggregateFunction = aggregateFunction;
            }

            public ScalarQuery(
                Expression<Func<TItemInput, TItemOutput>> selector,
                Expression<Func<TItemInput, bool>> filter,
                ReadOnlyCollection<SortExpression> sorters,
                bool isDistinct,
                AggregateFunction aggregateFunction)
                : this(SourceDescriptionByType<TItemInput>.Instance, selector, filter, sorters, isDistinct, aggregateFunction)
            {
                _aggregateFunction = aggregateFunction;
            }

            protected override ExpressionParseProduct Transform(IQueryTransformer transformer)
            {
                return transformer.TransformScalar(this);
            }

            public AggregateFunction AggregateFunction
            {
                get { return _aggregateFunction; }
            }
            private readonly AggregateFunction _aggregateFunction;
        }
    }
}
