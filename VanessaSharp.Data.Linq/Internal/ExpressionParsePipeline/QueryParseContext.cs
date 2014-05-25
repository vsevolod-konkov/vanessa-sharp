namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Контекст парсинга запроса.</summary>
    internal sealed class QueryParseContext
    {
        /// <summary>Параметры запроса.</summary>
        public ParseParameterCollection Parameters
        {
            get { return _parameters; }
        }
        private readonly ParseParameterCollection _parameters = new ParseParameterCollection();
    }
}
