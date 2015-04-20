using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Преобразователь <see cref="DayOfWeek"/> для работы в SQL.
    /// </summary>
    public static class DayOfWeekConverter
    {
        /// <summary>Массив значений, в порядке 1С SQL.</summary>
        private static readonly DayOfWeek[] _values = new []
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday,
                DayOfWeek.Sunday
            };
        
        /// <summary>Преобразование в число.</summary>
        public static int ToInt32(DayOfWeek dayOfWeek)
        {
            return Array.IndexOf(_values, dayOfWeek) + 1;
        }

        /// <summary>Преобразование из числа.</summary>
        public static DayOfWeek FromInt32(int index)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index > 0 && index <= 7);

            return _values[index - 1];
        }
    }
}
