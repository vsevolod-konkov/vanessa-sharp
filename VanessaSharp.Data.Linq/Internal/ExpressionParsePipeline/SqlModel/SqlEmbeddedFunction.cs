namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Встроенные функции SQL.
    /// </summary>
    internal enum SqlEmbeddedFunction
    {
        Substring,
        Year,
        Quarter,
        Month,
        DayOfYear,
        Day,
        Week,
        DayWeek,
        Hour,
        Minute,
        Second,
        BeginOfPeriod,
        EndOfPeriod
    }
}
