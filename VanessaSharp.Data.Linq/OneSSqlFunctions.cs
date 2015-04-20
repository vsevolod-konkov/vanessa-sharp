using System;
using System.Reflection;

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
        private static Exception CreateInvalidCallException(MethodBase method)
        {
            throw new InvalidOperationException(
                string.Format("Ошибка вызова метода \"{0}\". Данный метод можно использовать только в linq-запросе.",
                method));
        }

        #region Синтаксические конструкции

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
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
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
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        #endregion

        #region Приведения

        /// <summary>
        /// Преобразование к булевому типу.
        /// </summary>
        /// <param name="operand">Преобразуемый операнд.</param>
        public static bool ToBoolean<T>(T operand)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Преобразование к <see cref="short"/>.
        /// </summary>
        /// <param name="operand">Преобразуемый операнд.</param>
        /// <param name="length">Длина числа.</param>
        public static short ToInt16<T>(T operand, int? length = null)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Преобразование к <see cref="int"/>.
        /// </summary>
        /// <param name="operand">Преобразуемый операнд.</param>
        /// <param name="length">Длина числа.</param>
        public static int ToInt32<T>(T operand, int? length = null)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Преобразование к <see cref="long"/>.
        /// </summary>
        /// <param name="operand">Преобразуемый операнд.</param>
        /// <param name="length">Длина числа.</param>
        public static long ToInt64<T>(T operand, int? length = null)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Преобразование к <see cref="float"/>.
        /// </summary>
        /// <param name="operand">Преобразуемый операнд.</param>
        /// <param name="length">Длина числа.</param>
        /// <param name="precision">Точность числа.</param>
        public static float ToSingle<T>(T operand, int? length = null, int? precision = null)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Преобразование к <see cref="double"/>.
        /// </summary>
        /// <param name="operand">Преобразуемый операнд.</param>
        /// <param name="length">Длина числа.</param>
        /// <param name="precision">Точность числа.</param>
        public static double ToDouble<T>(T operand, int? length = null, int? precision = null)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Преобразование к <see cref="decimal"/>.
        /// </summary>
        /// <param name="operand">Преобразуемый операнд.</param>
        /// <param name="length">Длина числа.</param>
        /// <param name="precision">Точность числа.</param>
        public static decimal ToDecimal<T>(T operand, int? length = null, int? precision = null)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Преобразование к строковому типу.
        /// </summary>
        /// <param name="operand">Преобразуемый операнд.</param>
        /// <param name="length">Длина строки.</param>
        public static string ToString<T>(T operand, int? length = null)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Преобразование к <see cref="DateTime"/>.
        /// </summary>
        /// <param name="operand">Преобразуемый операнд.</param>
        public static DateTime ToDateTime<T>(T operand)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Преобразование к <see cref="OneSDataRecord"/>.
        /// </summary>
        /// <param name="operand">Преобразуемый операнд.</param>
        /// <param name="dataSourceName">Имя источника данных записи.</param>
        public static OneSDataRecord ToDataRecord<T>(T operand, string dataSourceName)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        #endregion

        #region Встроенные функции

        /// <summary>
        /// Получение квартала даты.
        /// </summary>
        public static int GetQuarter(DateTime date)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Получение недели года даты.
        /// </summary>
        public static int GetWeek(DateTime date)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Получение номера дня недели даты.
        /// </summary>
        public static int GetDayWeek(DateTime date)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        /// <summary>Получение начальной даты периода, в который входит заданная дата.</summary>
        /// <param name="date">Заданная дата.</param>
        /// <param name="kind">Вид периода.</param>
        public static DateTime BeginOfPeriod(DateTime date, OneSTimePeriodKind kind)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        /// <summary>Получение конечной даты периода, в который входит заданная дата.</summary>
        /// <param name="date">Заданная дата.</param>
        /// <param name="kind">Вид периода.</param>
        public static DateTime EndOfPeriod(DateTime date, OneSTimePeriodKind kind)
        {
            throw CreateInvalidCallException(MethodBase.GetCurrentMethod());
        }

        #endregion
    }
}
