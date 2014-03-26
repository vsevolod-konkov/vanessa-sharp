namespace VanessaSharp.AcceptanceTests.Utility
{
    /// <summary>Режим выполнения тестов.</summary>
    public enum TestMode
    {
        /// <summary>Реальный режим.</summary>
        /// <remarks>
        /// Режим тестирования при котором, происходит взаимодействие с
        /// реальной БД 1С.
        /// </remarks>
        Real,

        /// <summary>Изоляционный режим.</summary>
        /// <remarks>
        /// Изоляционный режим теста это режим при котором подключение
        /// к реальной БД 1С не делается, а ведется работа с моковой реализацией контрактов с 1С
        /// в памяти.
        /// </remarks>
        Isolated
    }
}
