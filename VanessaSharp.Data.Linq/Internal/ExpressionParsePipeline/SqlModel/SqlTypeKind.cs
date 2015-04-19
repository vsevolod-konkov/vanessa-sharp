namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Виды SQL-типов.
    /// </summary>
    internal enum SqlTypeKind
    {
        /// <summary>Булев.</summary>
        Boolean,

        /// <summary>Числовой.</summary>
        Number,

        /// <summary>Строковый.</summary>
        String,

        /// <summary>Дата.</summary>
        Date,

        /// <summary>Табличный.</summary>
        Table
    }
}