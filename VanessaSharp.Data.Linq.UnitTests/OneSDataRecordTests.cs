using System.Collections.ObjectModel;
using NUnit.Framework;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>
    /// Тестирование <see cref="OneSDataRecord"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSDataRecordTests
    {
        /// <summary>
        /// Тестирование <see cref="OneSDataRecord.Fields"/>.
        /// </summary>
        [Test]
        public void TestFields()
        {
            var fields = new ReadOnlyCollection<string>(
                new[] {"Id", "Name", "Value"});

            var testedInstance = new OneSDataRecord(fields);

            CollectionAssert.AreEqual(fields, testedInstance.Fields);
        }
    }
}
