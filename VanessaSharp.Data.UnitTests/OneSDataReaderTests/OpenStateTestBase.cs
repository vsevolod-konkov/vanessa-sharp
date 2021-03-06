﻿using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;

namespace VanessaSharp.Data.UnitTests.OneSDataReaderTests
{
    /// <summary>
    /// Базовый класс для тестов экземпляра
    /// <see cref="OneSDataReader"/>
    /// находящегося в открытом состоянии. 
    /// </summary>
    public abstract class OpenStateTestBase : OneSDataReaderComponentTestBase
    {
        /// <summary>
        /// Мок для <see cref="IDataReaderFieldInfoCollection"/>.
        /// </summary>
        private Mock<IDataReaderFieldInfoCollection> _dataReaderFieldInfoCollectionMock;

        /// <summary>
        /// Создание мока тестового экземпляра <see cref="IDataReaderFieldInfoCollection"/>.
        /// </summary>
        internal sealed override Mock<IDataReaderFieldInfoCollection> CreateDataReaderFieldInfoCollectionMock()
        {
            _dataReaderFieldInfoCollectionMock = base.CreateDataReaderFieldInfoCollectionMock();

            return _dataReaderFieldInfoCollectionMock;
        }

        /// <summary>Мок для <see cref="IValueConverter"/>.</summary>
        internal Mock<IValueConverter> ValueConverterMock { get; private set; }

        /// <summary>Создание тестового экземпляра <see cref="IValueConverter"/>.</summary>
        internal override IValueConverter CreateValueConverter()
        {
            ValueConverterMock = new Mock<IValueConverter>(MockBehavior.Strict);

            return ValueConverterMock.Object;
        }

        /// <summary>
        /// Установка рeализации <see cref="IDataReaderFieldInfoCollection.Count"/>.
        /// </summary>
        /// <param name="columnsCount">Количество колонок.</param>
        protected void SetupColumnsGetCount(int columnsCount)
        {
            _dataReaderFieldInfoCollectionMock
                .SetupGet(c => c.Count)
                .Returns(columnsCount);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.IsClosed"/>.
        /// </summary>
        [Test]
        public void TestIsClosed()
        {
            Assert.IsFalse(TestedInstance.IsClosed);
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.Close"/>.</summary>
        [Test]
        public virtual void TestClose()
        {
            // Act
            TestedInstance.Close();

            // Assert
            Assert.IsTrue(TestedInstance.IsClosed);
            DataRecordsProviderMock.VerifyDispose();
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.FieldCount"/>.</summary>
        [Test]
        public void TestFieldCount()
        {
            const int TEST_FIELD_COUNT = 8;

            // Arrange
            SetupColumnsGetCount(TEST_FIELD_COUNT);

            // Act & Assert
            Assert.AreEqual(TEST_FIELD_COUNT, TestedInstance.FieldCount);

            _dataReaderFieldInfoCollectionMock.VerifyGet(cs => cs.Count);
        }

        /// <summary>Установка получения колонки.</summary>
        /// <param name="index">Индекс колонки.</param>
        /// <param name="fieldInfo">Поле читателя.</param>
        private void SetupGetColumn(int index, DataReaderFieldInfo fieldInfo)
        {
            SetupColumnsGetCount(index + 1);

            _dataReaderFieldInfoCollectionMock
                .Setup(c => c[index])
                .Returns(fieldInfo);
        }

        /// <summary>Установка получения колонки.</summary>
        /// <param name="index">Индекс колонки.</param>
        /// <param name="name">Имя колонки.</param>
        /// <param name="type">Тип колонки.</param>
        protected void SetupGetColumn(int index, string name, Type type)
        {
            SetupGetColumn(index, new DataReaderFieldInfo(name, type, null, null));
        }

        /// <summary>Проверка получения полонки.</summary>
        /// <param name="index">Индекс колонки.</param>
        private void VerifyGetColumn(int index)
        {
            _dataReaderFieldInfoCollectionMock.Verify(c => c[index]);
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.GetDataTypeName"/>.</summary>
        [Test]
        public void TestGetDataTypeName()
        {
            const int TEST_FIELD_INDEX = 4;

            // Arrange
            const string EXPECTED_DATA_TYPE_NAME = "Тестовый тип";
            var field = new DataReaderFieldInfo("TestName", typeof(object), EXPECTED_DATA_TYPE_NAME, null);
            SetupGetColumn(TEST_FIELD_INDEX, field);

            // Act
            var result = TestedInstance.GetDataTypeName(TEST_FIELD_INDEX);

            // Assert
            Assert.AreEqual(EXPECTED_DATA_TYPE_NAME, result);
        }

        /// <summary>Тестирование <see cref="OneSDataReader.GetName"/>.</summary>
        [Test]
        public void TestGetName()
        {
            const string TEST_FIELD_NAME = "Тестовая колонка";
            const int TEST_FIELD_ORDINAL = 3;

            // Arrange
            SetupGetColumn(TEST_FIELD_ORDINAL, TEST_FIELD_NAME, typeof(object));

            // Act
            var actualName = TestedInstance.GetName(TEST_FIELD_ORDINAL);

            // Assert
            Assert.AreEqual(TEST_FIELD_NAME, actualName);

            VerifyGetColumn(TEST_FIELD_ORDINAL);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetFieldType"/>.
        /// </summary>
        [Test]
        public void TestGetFieldType()
        {
            const int TEST_FIELD_ORDINAL = 3;

            // Arrange
            var expectedType = typeof(decimal);
            SetupGetColumn(TEST_FIELD_ORDINAL, "[SomeName]", expectedType);

            // Act
            var actualType = TestedInstance.GetFieldType(TEST_FIELD_ORDINAL);

            // Assert
            Assert.AreEqual(expectedType, actualType);
            
            VerifyGetColumn(TEST_FIELD_ORDINAL);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.GetOrdinal"/>
        /// в случае если колонка с заданным именем существует.
        /// </summary>
        [Test]
        public void TestGetOrdinalWhenColumnExist()
        {
            // Arrange
            const string TEST_COLUMN_NAME = "Test";
            const int TEST_EXPECTED_ORDINAL = 2;

            SetupColumnsGetCount(TEST_EXPECTED_ORDINAL + 1);

            _dataReaderFieldInfoCollectionMock
                .Setup(c => c.IndexOf(TEST_COLUMN_NAME))
                .Returns(TEST_EXPECTED_ORDINAL)
                .Verifiable();
               
            // Act & Assert
            Assert.AreEqual(TEST_EXPECTED_ORDINAL, TestedInstance.GetOrdinal(TEST_COLUMN_NAME));

            _dataReaderFieldInfoCollectionMock
                .Verify(c => c.IndexOf(TEST_COLUMN_NAME));
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.GetOrdinal"/>
        /// в случае если колонка с заданным именем не существует.
        /// </summary>
        [Test]
        public void TestGetOrdinalWhenColumnNotExist()
        {
            // Arrange
            const string TEST_COLUMN_NAME = "Test";

            _dataReaderFieldInfoCollectionMock
                .Setup(c => c.IndexOf(TEST_COLUMN_NAME))
                .Returns(-1);

            // Act & Assert
            Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    var ordinal = TestedInstance.GetOrdinal(TEST_COLUMN_NAME);
                });

            _dataReaderFieldInfoCollectionMock
                .Verify(c => c.IndexOf(TEST_COLUMN_NAME));
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.NextResult"/>.
        /// </summary>
        [Test]
        public void TestNextResult()
        {
            // TODO: Пока не реализована возможность работы с пакетными запросами.
            Assert.IsFalse(TestedInstance.NextResult());
        }

        /// <summary>Настройка для получения значения.</summary>
        protected override void ArrangeGetValue(int ordinal, object returnValue)
        {
            SetupColumnsGetCount(ordinal + 1);
        }

        protected override void ArrangeGetDataReader(int ordinal, object returnValue, OneSDataReader expectedResult)
        {
            SetupGetColumn(ordinal, "TablePartField", typeof(OneSDataReader));
        }

        protected override void AssertGetDataReader(int ordinal, object returnValue)
        {
            VerifyGetColumn(ordinal);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetSchemaTable"/>.
        /// </summary>
        [Test]
        public void TestGetSchemaTable()
        {
            // Arrange
            var fields = new[]
                {
                    new DataReaderFieldInfo("Test1", typeof (string), "Null,Строка", null),
                    new DataReaderFieldInfo("Test2", typeof (int), "Число", null)
                };

            for (var fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
                SetupGetColumn(fieldIndex, fields[fieldIndex]);    

            // Act
            var result = TestedInstance.GetSchemaTable();

            // Assert
            Assert.AreEqual(4, result.Columns.Count);

            Assert.AreEqual("ColumnOrdinal", result.Columns[0].ColumnName);
            Assert.AreEqual("ColumnName", result.Columns[1].ColumnName);
            Assert.AreEqual("DataTypeName", result.Columns[2].ColumnName);
            Assert.AreEqual("AllowDBNull", result.Columns[3].ColumnName);

            Assert.AreEqual(typeof(int), result.Columns[0].DataType);
            Assert.AreEqual(typeof(string), result.Columns[1].DataType);
            Assert.AreEqual(typeof(string), result.Columns[2].DataType);
            Assert.AreEqual(typeof(bool), result.Columns[3].DataType);

            Assert.AreEqual(fields.Length, result.Rows.Count);

            for (var fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
            {
                var field = fields[fieldIndex];
                var row = result.Rows[fieldIndex];

                Assert.AreEqual(fieldIndex, row["ColumnOrdinal"]);
                Assert.AreEqual(field.Name, row["ColumnName"]);
                Assert.AreEqual(field.DataTypeName, row["DataTypeName"]);
                Assert.AreEqual(field.DataTypeName.Contains("Null"), row["AllowDBNull"]);
            }
        }
    }
}
