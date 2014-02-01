namespace VanessaSharp.Proxy.Common
{
    /// <summary>Интерфейс типизированной обертки над массивом 1С.</summary>
    /// <typeparam name="T">Тип элементов в массиве.</typeparam>
    [OneSObjectMapping(WrapType = typeof(OneSArray<>))]
    public interface IOneSArray<out T> : IOneSArray
    {
        /// <summary>Получение элемента по индексу.</summary>
        /// <param name="index">Индекс.</param>
        new T Get(int index);
    }
}