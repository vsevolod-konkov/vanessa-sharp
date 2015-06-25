using System;

namespace VanessaSharp.Data.Linq.AcceptanceTests
{
    /// <summary>Класс тестового справочника с уникальным идентификатором.</summary>
    [OneSDataSource("Справочник.СправочникUID")]
    public sealed class UidTestDictionary
    {
        [OneSDataColumn("UID")]
        public Guid UidField;
    }
}
