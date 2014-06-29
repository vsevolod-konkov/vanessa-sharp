using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Явное описание источника данных.
    /// </summary>
    internal sealed class ExplicitSourceDescription : ISourceDescription
    {
        /// <summary>Конструктор.</summary>
        /// <param name="sourceName">Имя источника.</param>
        public ExplicitSourceDescription(string sourceName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));

            _sourceName = sourceName;
        }

        /// <summary>Имя источника данных.</summary>
        public string SourceName
        {
            get { return _sourceName; }
        }
        private readonly string _sourceName;

        /// <summary>Получение имени источника данных.</summary>
        /// <param name="sourceResolver">Ресолвер имен источников данных.</param>
        string ISourceDescription.GetSourceName(ISourceResolver sourceResolver)
        {
            return _sourceName;
        }
    }
}
