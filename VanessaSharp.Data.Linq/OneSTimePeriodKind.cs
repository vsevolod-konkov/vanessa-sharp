namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Вид временного периода во встроенных функциях 1С.
    /// </summary>
    public enum OneSTimePeriodKind
    {
        /// <summary>Минута.</summary>
        Minute,

        /// <summary>Час.</summary>
        Hour,

        /// <summary>День.</summary>
        Day,

        /// <summary>Неделя.</summary>
        Week,

        /// <summary>Месяц.</summary>
        Month,

        /// <summary>Квартал.</summary>
        Quarter,

        /// <summary>Год.</summary>
        Year,

        /// <summary>Декада.</summary>
        TenDays,

        /// <summary>Полугодие.</summary>
        HalfYear
    }
}
