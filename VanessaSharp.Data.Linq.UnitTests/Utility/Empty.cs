using System.Collections.ObjectModel;

namespace VanessaSharp.Data.Linq.UnitTests.Utility
{
    /// <summary>Вспомогательные методы для пустой коллекции.</summary>
    internal static class Empty
    {
        /// <summary>
        /// Пустая неизменяемая коллекция.
        /// </summary>
        /// <typeparam name="T">Тип элементов.</typeparam>
        public static ReadOnlyCollection<T> ReadOnly<T>()
        {
            return new T[0].ToReadOnly();
        }
    }
}
