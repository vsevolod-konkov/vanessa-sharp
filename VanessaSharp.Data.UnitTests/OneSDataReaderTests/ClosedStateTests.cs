using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSDataReaderTests
{
    /// <summary>
    /// Тестирование <see cref="OneSDataReader"/>
    /// в случае когда экземпляр находится в закрытом состоянии.
    /// </summary>
    [TestFixture(Case.AfterBof)]
    [TestFixture(Case.AfterRow)]
    [TestFixture(Case.AfterEof)]
    public sealed class ClosedStateTests : OneSDataReaderComponentTestBase
    {
        public enum Case
        {
            AfterBof,
            AfterRow,
            AfterEof
        }

        /// <summary>Сценарий закрытия.</summary>
        private readonly Case _case;

        public ClosedStateTests(Case @case)
        {
            _case = @case;
        }

        /// <summary>Создание тестового экземпляра <see cref="ITypeDescriptionConverter"/>.</summary>
        internal override ITypeDescriptionConverter CreateValueTypeConverter()
        {
            return new Mock<ITypeDescriptionConverter>(MockBehavior.Strict).Object;
        }

        /// <summary>Создание тестового экземпляра <see cref="IValueConverter"/>.</summary>
        internal override IValueConverter CreateValueConverter()
        {
            return new Mock<IValueConverter>(MockBehavior.Strict).Object;
        }

        /// <summary>Создание тестового экземпляра <see cref="IQueryResult"/>.</summary>
        protected override IQueryResult CreateQueryResult()
        {
            var queryResultMock = new Mock<IQueryResult>(MockBehavior.Strict);
            SetupDispose(queryResultMock);

            if (_case != Case.AfterBof)
            {
                var queryResultSelectionMock = CreateQueryResultSelectionMock(queryResultMock);
                SetupDispose(queryResultSelectionMock);
            }

            return queryResultMock.Object;
        }

        /// <summary>Сценарий для приведения тестового экземпляра в нужное состояние.</summary>
        protected override void ScenarioAfterInitTestedInstance()
        {
            if (_case != Case.AfterBof)
            {
                Assert.IsTrue(TestedInstance.Read());
            }

            if (_case == Case.AfterEof)
            {
                Assert.IsFalse(TestedInstance.Read());
            }

            TestedInstance.Close();
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.IsClosed"/>.
        /// </summary>
        [Test]
        public void TestIsClosed()
        {
            Assert.IsTrue(TestedInstance.IsClosed);
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.FieldCount"/>.</summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestFieldCount()
        {
            var result = TestedInstance.FieldCount;
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.GetDataTypeName"/>.</summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetDataTypeName()
        {
            var result = TestedInstance.GetDataTypeName(4);
        }

        /// <summary>Тестирование <see cref="OneSDataReader.GetName"/>.</summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetName()
        {
            var result = TestedInstance.GetName(3);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetFieldType"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetFieldType()
        {
            var actualType = TestedInstance.GetFieldType(3);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.Read"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestRead()
        {
            var actualType = TestedInstance.Read();
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetValues"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetValues()
        {
            var actualResult = TestedInstance.GetValues(new object[10]);
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.HasRows"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestHasRows()
        {
            var result = TestedInstance.HasRows;
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.GetOrdinal"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetOrdinal()
        {
            var result = TestedInstance.GetOrdinal("TEST_FIELD");
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.NextResult"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNextResult()
        {
            TestedInstance.NextResult();
        }
    }
}
