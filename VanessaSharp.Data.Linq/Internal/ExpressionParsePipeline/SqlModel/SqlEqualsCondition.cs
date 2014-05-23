namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>Предикат равенства в SQL.</summary>
    internal sealed class SqlEqualsCondition : SqlCondition
    {
        /// <summary>Первый операнд.</summary>
        public SqlExpression FirstOperand { get; set; }

        /// <summary>Второй операнд.</summary>
        public SqlExpression SecondOperand { get; set; }
    }
}
