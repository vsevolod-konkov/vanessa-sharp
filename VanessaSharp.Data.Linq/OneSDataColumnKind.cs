namespace VanessaSharp.Data.Linq
{
    /// <summary>Вид данных поля 1С-объекта.</summary>
    public enum OneSDataColumnKind
    {
        /// <summary>Вид по-умолчанию.</summary>
        Default,

        /// <summary>Реквизит.</summary>
        Property = Default,

        /// <summary>Табличная часть.</summary>
        TablePart
    }
}
