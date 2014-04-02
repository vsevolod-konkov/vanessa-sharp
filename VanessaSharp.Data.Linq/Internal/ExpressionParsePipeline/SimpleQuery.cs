using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Обработанный запрос.</summary>
    internal sealed class SimpleQuery
    {
        /// <summary>Источник записей.</summary>
        private readonly string _source;

        /// <summary>Конструктор.</summary>
        /// <param name="source">Источник записей.</param>
        public SimpleQuery(string source)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(source));
            
            _source = source;
        }

        /// <summary>Источник записей.</summary>
        public string Source
        {
            get { return _source; }
        }

        //// <summary>Выражение выборки данных их записи.</summary>
        //public LambdaExpression SelectExpression { get; set; }
    }
}
