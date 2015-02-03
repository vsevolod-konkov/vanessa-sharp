using System;
using System.Linq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common.EnumMapping;

namespace VanessaSharp.Proxy.Common.Tests
{
    /// <summary>
    /// Тесты на <see cref="OneSEnumMapInfoProvider"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSEnumMapInfoProviderTests
    {
        /// <summary>
        /// Тестируемый экземпляр.
        /// </summary>
        private static readonly OneSEnumMapInfoProvider _testedInstance = OneSEnumMapInfoProvider.Default;

        /// <summary>
        /// Тестирование <see cref="OneSEnumMapInfoProvider.GetEnumMapInfo"/>.
        /// </summary>
        [Test]
        public void TestGetEnumMapInfo()
        {
            // Act
            var result = _testedInstance.GetEnumMapInfo(typeof(TestEnum));

            // Assert
            Assert.IsInstanceOf<OneSEnumMapInfo>(result);
            var enumMapInfo = (OneSEnumMapInfo)result;

            Assert.AreEqual("TestEnum", enumMapInfo.OneSEnumName);

            CollectionAssert.AllItemsAreInstancesOfType(enumMapInfo.ValueMaps, typeof(OneSEnumValueMapInfo));

            var enumValueMapInfos = enumMapInfo
                .ValueMaps.Cast<OneSEnumValueMapInfo>().ToArray();

            Assert.AreEqual(2, enumValueMapInfos.Length);

            var first = enumValueMapInfos
                .SingleOrDefault(v => v.OneSPropertyName == "First");
            Assert.IsNotNull(first);
            Assert.AreEqual(TestEnum.First, first.EnumValue);

            var second = enumValueMapInfos
                .SingleOrDefault(v => v.OneSPropertyName == "Second");
            Assert.IsNotNull(second);
            Assert.AreEqual(TestEnum.Second, second.EnumValue);
        }

        /// <summary>
        /// Тестирование <see cref="OneSEnumMapInfoProvider.GetEnumMapInfo"/>
        /// в случае если, перечисление не промаркировано атрибутом
        /// <see cref="OneSEnumAttribute"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInvalidGetEnumMapInfoWhenNotSupportEnum()
        {
            var result = _testedInstance.GetEnumMapInfo(typeof(NotSupportTestEnum));
        }

        /// <summary>
        /// Тестирование <see cref="OneSEnumMapInfoProvider.GetEnumMapInfo"/>
        /// в случае, если нет ни одного поля промаркированного
        /// атрибутом <see cref="OneSEnumValueAttribute"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInvalidGetEnumMapInfoWhenNothingAnyOneSValue()
        {
            var result = _testedInstance.GetEnumMapInfo(typeof(NothingValueTestEnum));
        }

        /// <summary>
        /// Тестовое перечисление.
        /// </summary>
        [OneSEnum]
        public enum TestEnum
        {
            [OneSEnumValue]
            First,
       
            [OneSEnumValue]
            Second,

            Third
        }

        public enum NotSupportTestEnum
        {
            [OneSEnumValue]
            First,

            Second,

            [OneSEnumValue]
            Third
        }

        [OneSEnum]
        public enum NothingValueTestEnum
        {
            First,
            Second,
            Third
        }
    }
}
