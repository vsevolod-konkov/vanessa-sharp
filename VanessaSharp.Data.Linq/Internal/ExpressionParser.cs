using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Стандартная реализация <see cref="IExpressionParser"/>.</summary>
    internal sealed class ExpressionParser : IExpressionParser
    {
        /// <summary>Экземпляр по умолчанию.</summary>
        public static ExpressionParser Default
        {
            get { return _default; }
        }
        private static readonly ExpressionParser _default = new ExpressionParser();

        /// <summary>Конструктор используемый для тестирования.</summary>
        /// <param name="queryableExpressionTransformer">
        /// Преобразователь linq-выражения <see cref="IQueryable{T}"/>
        /// в <see cref="IQuery"/>.
        /// </param>
        /// <param name="mappingProvider">
        /// Поставщик соответствия типам CLR
        /// данным 1С.
        /// </param>
        internal ExpressionParser(
            IQueryableExpressionTransformer queryableExpressionTransformer,
            IOneSMappingProvider mappingProvider)
        {
            Contract.Requires<ArgumentNullException>(queryableExpressionTransformer != null);
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            
            _queryableExpressionTransformer = queryableExpressionTransformer;
            _mappingProvider = mappingProvider;
            _queryTransformService = new QueryTransformService(_mappingProvider);
        }

        /// <summary>Конструктор для инициализации экзеемпляра по умолчанию.</summary>
        private ExpressionParser() : this(QueryableExpressionTransformer.Default, new OneSMappingProvider())
        {}

        /// <summary>
        /// Преобразователь linq-выражения <see cref="IQueryable{T}"/>
        /// в <see cref="IQuery"/>.
        /// </summary>
        private readonly IQueryableExpressionTransformer _queryableExpressionTransformer;

        /// <summary>
        /// Поставщик соответствия типам CLR
        /// данным 1С.
        /// </summary>
        private readonly IOneSMappingProvider _mappingProvider;

        /// <summary>
        /// Сервис для преобразования запросов.
        /// </summary>
        private readonly IQueryTransformService _queryTransformService;

        /// <summary>Разбор выражения.</summary>
        /// <param name="expression">Выражение.</param>
        public ExpressionParseProduct Parse(Expression expression)
        {
            // Конвейер преобразования
            
            return _queryableExpressionTransformer
                .Transform(expression) // Queryable-выражение преобразуется в объект запроса
                .Transform(_queryTransformService); // Объект запроса преобразовуется в конечный результат - SQL-команду и объект вычитки данных из резултата запроса
        }

        /// <summary>
        /// Проверка типа на корректность использования его в виде 
        /// типа записи данных из 1С.
        /// </summary>
        /// <param name="dataType">Тип данных.</param>
        public void CheckDataType(Type dataType)
        {
            _mappingProvider.CheckDataType(OneSDataLevel.Root, dataType);
        }
    }
}
