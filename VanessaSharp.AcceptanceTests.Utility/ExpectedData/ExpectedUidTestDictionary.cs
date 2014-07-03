using System;
using System.Collections.ObjectModel;

namespace VanessaSharp.AcceptanceTests.Utility.ExpectedData
{
    /// <summary>
    /// Ожидаемые данные в справочнике 1С : СправочникUID
    /// </summary>
    public sealed class ExpectedUidTestDictionary
    {
        [Field("UID")]
        public Guid GuidField;

        /// <summary>
        /// Ожидаемые данные в справочнике 1С : СправочникUID
        /// </summary>
        public static ReadOnlyCollection<ExpectedUidTestDictionary> ExpectedData
        {
            get
            {
                return new ReadOnlyCollection<ExpectedUidTestDictionary>(
                    new[] { new ExpectedUidTestDictionary { GuidField = Guid.Parse("8e12149f-5b71-4218-a1cd-429d3d1cfe68") } }
                    );
            }
        }
    }
}