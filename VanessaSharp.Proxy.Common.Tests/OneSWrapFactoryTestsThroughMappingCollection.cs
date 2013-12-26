using NUnit.Framework;

namespace VanessaSharp.Proxy.Common.Tests
{
    /// <summary>
    /// Тестирование <see cref="OneSWrapFactory"/>
    /// через подачу в конструктор коллекции <see cref="OneSObjectMapping"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSWrapFactoryTestsThroughMappingCollection : OneSWrapFactoryTestsBase
    {
        /// <summary>Инициализация тестируемого экземпляра.</summary>
        protected override OneSWrapFactory InitTestedInstance()
        {
            var mapping = new OneSObjectMapping(typeof(ITestObject), typeof(TestObject), TEST_ONES_TYPE_NAME);

            return new OneSWrapFactory(new[] { mapping });
        }
    }
}
