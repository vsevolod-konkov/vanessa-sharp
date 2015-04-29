namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Интерфейс скалярного запроса.
    /// </summary>
    /// <typeparam name="TItemInput">Тип элементов входной последовательности.</typeparam>
    /// <typeparam name="TItemOutput">Тип элементов выходной последовательности.</typeparam>
    /// <typeparam name="TResult">Тип скалярного результата.</typeparam>
    internal interface IScalarQuery<TItemInput, TItemOutput, TResult> : IQuery<TItemInput, TItemOutput>
    {
        /// <summary>Функция агрегирования.</summary>
        AggregateFunction AggregateFunction { get; }
    }
}
