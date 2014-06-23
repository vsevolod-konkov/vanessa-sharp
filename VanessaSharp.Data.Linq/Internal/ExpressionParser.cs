using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
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
        /// в <see cref="SimpleQuery"/>.
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
        }

        /// <summary>Конструктор для инициализации экзеемпляра по умолчанию.</summary>
        private ExpressionParser() : this(QueryableExpressionTransformer.Default, new OneSMappingProvider())
        {}

        /// <summary>
        /// Преобразователь linq-выражения <see cref="IQueryable{T}"/>
        /// в <see cref="SimpleQuery"/>.
        /// </summary>
        private readonly IQueryableExpressionTransformer _queryableExpressionTransformer;

        /// <summary>
        /// Поставщик соответствия типам CLR
        /// данным 1С.
        /// </summary>
        private readonly IOneSMappingProvider _mappingProvider;

        /// <summary>Разбор выражения.</summary>
        /// <param name="expression">Выражение.</param>
        public ExpressionParseProduct Parse(Expression expression)
        {
            return _queryableExpressionTransformer
                .Transform(expression)
                .Transform(_mappingProvider);
        }

        /// <summary>
        /// Проверка типа на корректность использования его в виде 
        /// типа записи данных из 1С.
        /// </summary>
        /// <param name="dataType">Тип данных.</param>
        public void CheckDataType(Type dataType)
        {
            _mappingProvider.CheckDataType(dataType);
        }
    }
}
