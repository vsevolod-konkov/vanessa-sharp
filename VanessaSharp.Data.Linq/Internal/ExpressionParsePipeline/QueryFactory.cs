using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

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
        /// <returns>Созданный запрос.</returns>
        public static IQuery CreateQuery(string sourceName, LambdaExpression filter,
                                         ReadOnlyCollection<SortExpression> sorters,
                                         bool isDistinct)
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
                sorters, isDistinct);
        }

        /// <summary>Создание запроса последовательности элементов проекции.</summary>
        private static IQuery CreateQuery(Type outputType, string sourceName, LambdaExpression selector,
                                          LambdaExpression filter, ReadOnlyCollection<SortExpression> sorters,
                                          bool isDistinct)
        {
            var type = typeof(Query<,>).MakeGenericType(typeof(OneSDataRecord), outputType);

            return (IQuery)Activator.CreateInstance(type, new ExplicitSourceDescription(sourceName), selector, filter, sorters, isDistinct);
        }

        /// <summary>Создание запроса последовательности проекций из <see cref="OneSDataRecord"/>.</summary>
        /// <param name="sourceName">Имя источника.</param>
        /// <param name="selector">Выражение выборки - проекции <see cref="OneSDataRecord"/>.</param>
        /// <param name="filter">Выражение фильтрации.</param>
        /// <param name="sorters">Коллекция выражений сортировки.</param>
        /// <param name="isDistinct">Выборка различных.</param>
        /// <returns>Созданный запрос.</returns>
        public static IQuery CreateQuery(string sourceName, LambdaExpression selector, LambdaExpression filter,
                                         ReadOnlyCollection<SortExpression> sorters, bool isDistinct)
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

            return CreateQuery(outputType, sourceName, selector, filter, sorters, isDistinct);
        }

        /// <summary>Создание запроса для типизированных записей.</summary>
        private static IQuery CreateQuery(Type inputType, Type outputType,
                                          LambdaExpression selector, LambdaExpression filter,
                                          ReadOnlyCollection<SortExpression> sorters,
                                          bool isDistinct)
        {
            var type = typeof(Query<,>).MakeGenericType(inputType, outputType);
            return (IQuery)Activator.CreateInstance(type, selector, filter, sorters, isDistinct);
        }

        /// <summary>Создание запроса последовательности элементов типизированных записей.</summary>
        /// <param name="itemType">Тип элементов типизированных записей.</param>
        /// <param name="filter">Выражение фильтрации записей.</param>
        /// <param name="sorters">Выражения сортировки записей.</param>
        /// <param name="isDistinct">Выборка различных.</param>
        /// <returns>Созданный запрос.</returns>
        public static IQuery CreateQuery(Type itemType, LambdaExpression filter,
                                         ReadOnlyCollection<SortExpression> sorters,
                                         bool isDistinct)
        {
            Contract.Requires<ArgumentNullException>(itemType != null);
            Contract.Requires<ArgumentNullException>(itemType != typeof(OneSDataRecord));

            Contract.Requires<ArgumentException>(
                (filter == null) || IsInputTypeForLambda(filter, itemType));

            Contract.Requires<ArgumentNullException>(sorters != null);
            Contract.Requires<ArgumentException>(
                sorters.All(s => IsInputTypeForLambda(s.KeyExpression, itemType)));

            return CreateQuery(itemType, itemType, null, filter, sorters, isDistinct);
        }

        /// <summary>Создание запроса последовательности элементов проекций типизированных записей.</summary>
        /// <param name="selector">Выражение выборки типизированных записей.</param>
        /// <param name="filter">Выражение фильтрации типизированных записей.</param>
        /// <param name="sorters">Выражения сортировки типизированных записей.</param>
        /// <param name="isDistinct">Выборка различных.</param>
        /// <returns>Созданный запрос.</returns>
        public static IQuery CreateQuery(LambdaExpression selector, LambdaExpression filter,
                                         ReadOnlyCollection<SortExpression> sorters,
                                         bool isDistinct)
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
            return CreateQuery(selectorArgs[0], selectorArgs[1], selector, filter, sorters, isDistinct);
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
            public Query(Expression<Func<TInput, TOutput>> selector,
                         Expression<Func<TInput, bool>> filter,
                         ReadOnlyCollection<SortExpression> sorters,
                         bool isDistinct)
            : this(SourceDescriptionByType<TInput>.Instance, selector, filter, sorters, isDistinct)
            {
            }

            /// <summary>Конструктор.</summary>
            /// <param name="source">Описание источника данных.</param>
            /// <param name="selector">Выражение выборки.</param>
            /// <param name="filter">Выражение фильтрации.</param>
            /// <param name="sorters">Выражения сортировки.</param>
            /// <param name="isDistinct">Выборка различных.</param>
            public Query(
                ISourceDescription source,
                Expression<Func<TInput, TOutput>> selector,
                Expression<Func<TInput, bool>> filter,
                ReadOnlyCollection<SortExpression> sorters,
                bool isDistinct)
            {
                Contract.Requires<ArgumentNullException>(source != null);
                Contract.Requires<ArgumentException>(
                    ((typeof(TInput) == typeof(OneSDataRecord)) && (source is ExplicitSourceDescription))
                    ||
                    ((typeof(TInput) != typeof(OneSDataRecord)) && (source is SourceDescriptionByType<TInput>))
                    );

                Contract.Requires<ArgumentException>(
                    (typeof(TInput) == typeof(TOutput) && selector == null)
                    ||
                    (typeof(TInput) == typeof(TOutput) || selector != null));

                Contract.Requires<ArgumentNullException>(sorters != null);
                Contract.Requires<ArgumentException>(sorters.All(s =>
                {
                    var type = s.KeyExpression.Type;

                    return type.IsGenericType
                        && type.GetGenericTypeDefinition() == typeof(Func<,>)
                        && type.GetGenericArguments()[0] == typeof(TInput);
                }));

                _source = source;
                _selector = selector;
                _filter = filter;
                _sorters = sorters;
                _isDistinct = isDistinct;
            }

            /// <summary>Описание источника данных 1С.</summary>
            public override ISourceDescription Source
            {
                get { return _source; }
            }
            private readonly ISourceDescription _source;

            /// <summary>Выражение выборки.</summary>
            public override Expression<Func<TInput, TOutput>> Selector
            {
                get { return _selector; }
            }
            private readonly Expression<Func<TInput, TOutput>> _selector;

            /// <summary>Выражение фильтрации.</summary>
            public override Expression<Func<TInput, bool>> Filter
            {
                get { return _filter; }
            }
            private readonly Expression<Func<TInput, bool>> _filter;

            /// <summary>Выражения сортировки.</summary>
            public override ReadOnlyCollection<SortExpression> Sorters
            {
                get { return _sorters; }
            }

            /// <summary>Выборка различных.</summary>
            public override bool IsDistinct
            {
                get { return _isDistinct; }
            }
            private readonly bool _isDistinct;

            private readonly ReadOnlyCollection<SortExpression> _sorters;
        }
    }
}
