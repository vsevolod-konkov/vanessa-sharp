namespace VanessaSharp.Data.Linq.Internal.SqlExpressions
{
    /// <summary>Построитель SQL-выражений.</summary>
    internal interface ISqlBuilder
    {
        void WriteLiteral(string literal);
        void WriteBeginSelect();
        void WriteBeginColumn();
        void WriteEndColumn();
        void WriteEndSelect();
        void WriteBeginFrom();
        void WriteEndFrom();
        void WriteAsterix();
    }
}
