namespace VanessaSharp.Proxy.Common
{
    /// <summary>Массив 1С.</summary>
    [OneSObjectMapping(WrapType = typeof(OneSArray))]
    public interface IOneSArray : IGlobalContextBound
    {
        /// <summary>Количество элементов в массиве.</summary>
        int Count();

        /// <summary>Получение элемента по индексу.</summary>
        /// <param name="index">Индекс.</param>
        object Get(int index);
    }
}
