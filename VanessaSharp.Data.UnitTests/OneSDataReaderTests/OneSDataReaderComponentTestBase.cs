using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSDataReaderTests
{
    /// <summary>
    /// Базовый класс для компонентных тестов над
    /// <see cref="OneSDataReader"/>.
    /// </summary>
    public abstract class OneSDataReaderComponentTestBase
    {
        #region Вспомогательные статические методы

        /// <summary>
        /// Установка реализации <see cref="IDisposable.Dispose"/>
        /// для мока.
        /// </summary>
        protected static void SetupDispose<T>(Mock<T> mock)
            where T : class, IDisposable
        {
            mock
                .Setup(o => o.Dispose())
                .Verifiable();
        }

        /// <summary>Создание мока реализующего <see cref="IDisposable"/>.</summary>
        protected static Mock<T> CreateDisposableMock<T>()
            where T : class, IDisposable
        {
            var mock = new Mock<T>(MockBehavior.Strict);
            SetupDispose(mock);

            return mock;
        }

        /// <summary>
        /// Проверка вызова <see cref="IDisposable.Dispose"/> 
        /// у мока.
        /// </summary>
        protected static void VerifyDispose<T>(Mock<T> mock)
            where T : class, IDisposable
        {
            mock.Verify(o => o.Dispose(), Times.AtLeastOnce());
        }

        /// <summary>Менеджер строк тестового экземпляра.</summary>
        protected sealed class RowsManager
        {
            /// <summary>Индекс строки.</summary>
            private int _rowIndex;

            public RowsManager()
            {
                Reset();
                RowsCount = 1;
            }

            /// <summary>Количество строк.</summary>
            public int RowsCount { get; set; }

            /// <summary>Достигнут ли конец в коллекции строк.</summary>
            public bool IsEof
            {
                get { return !(_rowIndex < RowsCount); }
            }

            /// <summary>Переход на следующую строку.</summary>
            public void Next()
            {
                _rowIndex++;
            }

            /// <summary>Сброс. Перевод в начало.</summary>
            public void Reset()
            {
                _rowIndex = -1;
            }
        }

        /// <summary>
        /// Создание мока <see cref="IQueryResultSelection"/>
        /// для реализации <see cref="IQueryResult.Choose"/>.
        /// </summary>
        protected static Mock<IQueryResultSelection> CreateQueryResultSelectionMock(Mock<IQueryResult> queryResultMock, RowsManager rowsManager)
        {
            var queryResultSelectionMock = new Mock<IQueryResultSelection>(MockBehavior.Strict);
            queryResultSelectionMock
                .Setup(s => s.Next())
                .Returns(() =>
                {
                    rowsManager.Next();
                    return !rowsManager.IsEof;
                })
                .Verifiable();

            queryResultMock
                .Setup(r => r.IsEmpty())
                .Returns(false);
            queryResultMock
                .Setup(r => r.Choose())
                .Returns(queryResultSelectionMock.Object);

            return queryResultSelectionMock;
        }

        /// <summary>
        /// Создание мока <see cref="IQueryResultSelection"/>
        /// для реализации <see cref="IQueryResult.Choose"/>.
        /// </summary>
        protected static Mock<IQueryResultSelection> CreateQueryResultSelectionMock(Mock<IQueryResult> queryResultMock)
        {
            return CreateQueryResultSelectionMock(queryResultMock, new RowsManager());
        }

        #endregion

        /// <summary>Тестируемый экземпляр.</summary>
        protected OneSDataReader TestedInstance { get; private set; }

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            TestedInstance = new OneSDataReader(CreateQueryResult(),  CreateValueTypeConverter());

            ScenarioAfterInitTestedInstance();
        }

        /// <summary>Создание тестового экземпляра <see cref="IQueryResult"/>.</summary>
        protected abstract IQueryResult CreateQueryResult();

        /// <summary>Создание тестового экземпляра <see cref="IValueTypeConverter"/>.</summary>
        internal abstract IValueTypeConverter CreateValueTypeConverter();

        /// <summary>Сценарий для приведения тестового экземпляра в нужное состояние.</summary>
        protected virtual void ScenarioAfterInitTestedInstance() {}

        /// <summary>Тестирование свойства <see cref="OneSDataReader.Depth"/>.</summary>
        [Test]
        public void TestDepth()
        {
            Assert.AreEqual(0, TestedInstance.Depth);
        }

        /// <summary>Тестирование <see cref="OneSDataReader.RecordsAffected"/>.</summary>
        [Test]
        public void TestRecordsAffected()
        {
            Assert.AreEqual(-1, TestedInstance.RecordsAffected);
        }
    }
}
