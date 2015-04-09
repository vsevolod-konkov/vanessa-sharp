using System;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Функции 1С SQL.
    /// </summary>
    /// <remarks>
    /// Можно использовать только в LINQ-запросах.
    /// </remarks>
    public static class OneSSqlFunctions
    {
        /// <summary>
        /// Условие IN со списком значений.
        /// </summary>
        /// <param name="operand">Операнд, проверяемый на соответствие.</param>
        /// <param name="values">Значения.</param>
        /// <typeparam name="T">Тип значений.</typeparam>
        /// <returns>
        /// Возвращает <c>true</c> если операнд соответствует какому-либо значению из списка.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// В случае если вызывался не в LINQ-запросе.
        /// </exception>
        public static bool In<T>(T operand, params T[] values)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Условие IN со списком значений с проверкой в иерархии элементов.
        /// </summary>
        /// <param name="operand">Операнд, проверяемый на соответствие.</param>
        /// <param name="values">Значения.</param>
        /// <typeparam name="T">Тип значений.</typeparam>
        /// <returns>
        /// Возвращает <c>true</c> если операнд соответствует какому-либо значению из списка.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// В случае если вызывался не в LINQ-запросе.
        /// </exception>
        public static bool InHierarchy<T>(T operand, params T[] values)
        {
            throw new InvalidOperationException();
        }
    }
}
