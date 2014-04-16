using System;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Простой запрос получения записей.</summary>
    internal sealed class DataRecordsQuery : SimpleQuery
    {
        /// <summary>Конструктор.</summary>
        /// <param name="source">Источник записей.</param>
        public DataRecordsQuery(string source) 
            : base(source)
        {}

        /// <summary>Тип элемента.</summary>
        public override Type ItemType
        {
            get { return typeof(OneSDataRecord); }
        }

        /// <summary>Преобразование.</summary>
        public override ExpressionParseProduct Transform()
        {
            return QueryTransformer.Transform(this);
        }
    }
}
