namespace VanessaSharp.AcceptanceTests.Utility.Mocks
{
    /// <summary>
    /// Вид поля.
    /// </summary>
    public enum FieldKind
    {
        /// <summary>
        /// Любое поле.
        /// </summary>
        Any,

        /// <summary>
        /// Поле, имеющее скалярный тип.
        /// </summary>
        Scalar,

        /// <summary>
        /// Поле, которое является табличной частью.
        /// </summary>
        TablePart
    }
}
