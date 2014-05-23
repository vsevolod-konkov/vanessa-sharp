using System;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Простой запрос получения записей.</summary>
    internal sealed class DataRecordsQuery : SimpleQuery
    {
        /// <summary>Конструктор.</summary>
        /// <param name="source">Источник записей.</param>
        /// <param name="filter">Выражение фильтрации.</param>
        public DataRecordsQuery(string source, Expression<Func<OneSDataRecord, bool>> filter) 
            : base(source, filter)
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
