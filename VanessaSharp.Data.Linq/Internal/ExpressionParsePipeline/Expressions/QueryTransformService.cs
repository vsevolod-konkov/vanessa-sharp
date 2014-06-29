using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Сервис для преобразования запросов.</summary>
    internal sealed class QueryTransformService : IQueryTransformService
    {
        /// <summary>
        ///  Методы преобразования LINQ-выражений методов-запросов.
        /// </summary>
        private readonly IExpressionTransformMethods _expressionTransformMethods;

        /// <summary>Конструктор использования.</summary>
        /// <param name="mappingProvider">Поставщик соответствий типам источников данных 1С.</param>
        public QueryTransformService(IOneSMappingProvider mappingProvider)
            : this(new ExpressionTransformMethods(mappingProvider))
        {
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
        }

        /// <summary>Конструктор.</summary>
        /// <param name="expressionTransformMethods">
        ///  Методы преобразования LINQ-выражений методов-запросов.
        /// </param>
        internal QueryTransformService(IExpressionTransformMethods expressionTransformMethods)
        {
            Contract.Requires<ArgumentNullException>(expressionTransformMethods != null);

            _expressionTransformMethods = expressionTransformMethods;
        }

        /// <summary>Создание преобразователя запросов.</summary>
        public IQueryTransformer CreateTransformer()
        {
            return new QueryTransformer(_expressionTransformMethods);
        }
    }
}
