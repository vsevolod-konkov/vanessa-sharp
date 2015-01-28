using System.Collections.ObjectModel;

namespace VanessaSharp.AcceptanceTests.Utility.ExpectedData
{
    /// <summary>
    /// Ожидаемые данные для тестового иерархического справочника.
    /// </summary>
    public sealed class ExpectedHierarchicalTestDictionary
    {
        /// <summary>Ожидаемые данные в БД 1С.</summary>
        public static ReadOnlyCollection<ExpectedHierarchicalTestDictionary> ExpectedData
        {
            get { return _expectedData; }
        }
        private static readonly ReadOnlyCollection<ExpectedHierarchicalTestDictionary> _expectedData
            = new ReadOnlyCollection<ExpectedHierarchicalTestDictionary>(GetExpectedDataArray());

        private static ExpectedHierarchicalTestDictionary[] GetExpectedDataArray()
        {
            return new ExpectedHierarchicalTestDictionary[]
                {
                    
                };
        }
    }
}
