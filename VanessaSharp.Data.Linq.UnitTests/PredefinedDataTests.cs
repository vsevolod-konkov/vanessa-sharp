using NUnit.Framework;
using VanessaSharp.Data.Linq.PredefinedData;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>
    /// Тестирование описаня предопределенных данных.
    /// </summary>
    [TestFixture]
    public sealed class PredefinedDataTests
    {
        /// <summary>
        /// Тестирование <see cref="PredefinedData.Fields.GetLocalizedName"/>.
        /// </summary>
        [Test]
        public void TestGetLocalizedName()
        {
            Assert.AreEqual(
                "Ссылка",
                Fields.Catalog.Ref.GetLocalizedName()
                );
        }
    }
}
