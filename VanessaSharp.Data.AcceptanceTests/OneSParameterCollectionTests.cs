using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests
{
    /// <summary>
    /// Тесты на <see cref="OneSParameterCollection"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSParameterCollectionTests
    {
        /// <summary>Тестируемый экземпляр.</summary>
        private OneSParameterCollection _testedInstance;

        /// <summary>Тестовое имя параметра.</summary>
        private const string TEST_PARAMETER_NAME = "ТестПараметр";

        /// <summary>Другое имя параметра отличное от тестового.</summary>
        private const string ANOTHER_PARAMETER_NAME = "ДругойПараметр";

        /// <summary>Служебный счетчик.</summary>
        private int _currentIndex;

        /// <summary>Генерация параметров.</summary>
        /// <param name="count">Требуемое количество.</param>
        private IEnumerable<OneSParameter> GenerateParameters(int count)
        {
            for (var counter = 0; counter < count; counter++)
            {
                yield return new OneSParameter("Параметр" + _currentIndex++);
            }
        }

        /// <summary>Генерация массива параметров.</summary>
        /// <param name="count">Требуемое количество.</param>
        private OneSParameter[] GenerateParametersArray(int count)
        {
            return GenerateParameters(count).ToArray();
        }

            /// <summary>
        /// Инициализация тестов.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _testedInstance = new OneSParameterCollection();
            _currentIndex = 0;
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.IsSynchronized"/>.
        /// </summary>
        [Test]
        public void TestIsSynchronized()
        {
            Assert.IsFalse(_testedInstance.IsSynchronized);
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.IsFixedSize"/>.
        /// </summary>
        [Test]
        public void TestIsFixedSize()
        {
            Assert.IsFalse(_testedInstance.IsFixedSize);
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.IsReadOnly"/>.
        /// </summary>
        [Test]
        public void TestIsReadOnly()
        {
            Assert.IsFalse(_testedInstance.IsReadOnly);
        }
        
        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.Count"/> после инициализации.
        /// </summary>
        [Test]
        public void TestCountAfterInit()
        {
            Assert.AreEqual(0, _testedInstance.Count);
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.Add(OneSParameter)"/>
        /// в случае хорошего сценария.
        /// </summary>
        [Test]
        public void TestAddParameter()
        {
            var parameter = new OneSParameter(TEST_PARAMETER_NAME);

            Assert.AreEqual(0, _testedInstance.Add(parameter));
            CollectionAssert.AreEqual(new[] { parameter }, _testedInstance);
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.Add(OneSParameter)"/>
        /// в случае хорошего сценария.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddParameterWhenNameIsEmpty()
        {
            _testedInstance.Add(new OneSParameter());
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.Add(OneSParameter)"/>
        /// в случае хорошего сценария.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddParameterWhenAlreadyExistsWithSameName()
        {
            _testedInstance.Add(new OneSParameter(TEST_PARAMETER_NAME));
            _testedInstance.Add(new OneSParameter(TEST_PARAMETER_NAME.ToLower()));
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.AddNew"/>
        /// в успешном случае.
        /// </summary>
        [Test]
        public void TestAddNewParameter()
        {
            var parameter = new OneSParameter(TEST_PARAMETER_NAME, 5);
            var newParameter = _testedInstance.AddNew(parameter);

            Assert.IsNotNull(newParameter);
            Assert.AreEqual(parameter.ParameterName, newParameter.ParameterName);
            Assert.AreEqual(parameter.Value, newParameter.Value);

            CollectionAssert.AreEqual(new[] { newParameter }, _testedInstance);
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.AddNew"/>.
        /// в случае когда имя не задано.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddNewParameterWhenNameIsEmpty()
        {
            _testedInstance.AddNew(new OneSParameter());
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.AddNew"/>.
        /// в случае когда уже есть параметр с таким же именем.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddNewParameterWhenAlreadyExistsWithSameName()
        {
            _testedInstance.Add(new OneSParameter(TEST_PARAMETER_NAME));

            _testedInstance.AddNew(new OneSParameter(TEST_PARAMETER_NAME.ToUpper()));
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.Add(string)"/>
        /// в случае нормального сценария.
        /// </summary>
        [Test]
        public void TestAddParameterByName()
        {
            var result = _testedInstance.Add(TEST_PARAMETER_NAME);

            Assert.IsNotNull(result);
            Assert.AreEqual(TEST_PARAMETER_NAME, result.ParameterName);
            Assert.IsNull(result.Value);

            CollectionAssert.AreEqual(new[] { result }, _testedInstance);
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.Add(string)"/>.
        /// в случае когда уже есть параметр с таким же именем.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddParameterByNameWhenAlreadyExistsWithSameName()
        {
            _testedInstance.Add(new OneSParameter(TEST_PARAMETER_NAME));

            _testedInstance.Add(TEST_PARAMETER_NAME.ToUpper());
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.Add(string, object)"/>
        /// в случае нормального сценария.
        /// </summary>
        [Test]
        public void TestAddParameterByNameAndValue()
        {
            const int PARAMETER_VALUE = 125;

            // Act
            var result = _testedInstance.Add(TEST_PARAMETER_NAME, PARAMETER_VALUE);

            // Arrange
            Assert.IsNotNull(result);
            Assert.AreEqual(TEST_PARAMETER_NAME, result.ParameterName);
            Assert.AreEqual(PARAMETER_VALUE, result.Value);

            CollectionAssert.AreEqual(new[] { result }, _testedInstance);
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.Add(string)"/>.
        /// в случае когда уже есть параметр с таким же именем.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddParameterByNameAndValueWhenAlreadyExistsWithSameName()
        {
            _testedInstance.Add(new OneSParameter(TEST_PARAMETER_NAME));

            _testedInstance.Add(TEST_PARAMETER_NAME.ToUpper(), 123);
        }

        /// <summary>
        /// Тестирование 
        /// <see cref="OneSParameterCollection.AddRange(System.Collections.Generic.IEnumerable{VanessaSharp.Data.OneSParameter})"/>
        /// в случае успешного сценария.
        /// </summary>
        [Test]
        public void TestAddRangeEnumerable()
        {
            var range = GenerateParametersArray(5);

            _testedInstance.AddRange(range.AsEnumerable());

            CollectionAssert.AreEqual(range, _testedInstance);
        }

        private void TestAddRangeEnumerableWhenIncorrectRange(
            int lenght,
            int incorrectIndex,
            Action<OneSParameter, IList<OneSParameter>> corruptAction)
        {
            var range = GenerateParametersArray(lenght);

            corruptAction(range[incorrectIndex], range);

            Assert.Throws<ArgumentException>(() =>
                _testedInstance.AddRange(range.AsEnumerable()));

            CollectionAssert.AreEqual(range.Take(incorrectIndex), _testedInstance);
        }

        /// <summary>
        /// Тестирование 
        /// <see cref="OneSParameterCollection.AddRange(System.Collections.Generic.IEnumerable{VanessaSharp.Data.OneSParameter})"/>
        /// в случае присутствия параметров с пустыми именами.
        /// </summary>
        [Test]
        public void TestAddRangeEnumerableWhenExistParametersWithEmptyName()
        {
            TestAddRangeEnumerableWhenIncorrectRange(8, 3, (p, pms) => p.ParameterName = null);
        }

        /// <summary>
        /// Тестирование 
        /// <see cref="OneSParameterCollection.AddRange(System.Collections.Generic.IEnumerable{VanessaSharp.Data.OneSParameter})"/>
        /// в случае присутствия параметров с одинаковыми именами.
        /// </summary>
        [Test]
        public void TestAddRangeEnumerableWhenExistParametersWithSameName()
        {
            const int INCORRECT_INDEX = 5;

            TestAddRangeEnumerableWhenIncorrectRange(8, INCORRECT_INDEX, (p, pms) => p.ParameterName = pms[INCORRECT_INDEX - 3].ParameterName);
        }

        /// <summary>Заполнение коллекции параметрами.</summary>
        /// <param name="count">Количество.</param>
        private void FillParameters(int count)
        {
            _testedInstance.AddRange(GenerateParameters(count));
        }

        /// <summary>
        /// Тестирование вспомогательного метода.
        /// </summary>
        [Test]
        public void TestFillParameters()
        {
            const int COUNT = 5;
            
            FillParameters(5);

            Assert.AreEqual(COUNT, _testedInstance.Count);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSParameterCollection.Count"/>. 
        /// </summary>
        [Test]
        public void TestClear()
        {
            // Arrange
            FillParameters(5);

            // Act
            _testedInstance.Clear();

            // Assert
            Assert.AreEqual(0, _testedInstance.Count);
            Assert.IsFalse(_testedInstance.GetEnumerator().MoveNext());
        }

        private void FillParametersWithParam(int offset, OneSParameter parameter, int additionalCount)
        {
            FillParameters(offset);
            _testedInstance.Add(parameter);
            FillParameters(additionalCount);
        }

        private OneSParameter FillParametersWithParam(int offset, string parameterName, int additionalCount)
        {
            var parameter = new OneSParameter(parameterName);
            FillParametersWithParam(offset, parameter, additionalCount);

            return parameter;
        }

        [Test]
        public void TestFillParametersWithParam()
        {
            // Arrange
            const int OFFSET = 3;
            const int ADDITIONAL_COUNT = 4;

            var parameter = new OneSParameter(TEST_PARAMETER_NAME);

            // Act
            FillParametersWithParam(OFFSET, parameter, ADDITIONAL_COUNT);
            
            // Assert
            Assert.AreEqual(OFFSET + 1 + ADDITIONAL_COUNT, _testedInstance.Count);
            Assert.AreSame(parameter, _testedInstance[OFFSET]);
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSParameterCollection.Contains(string)"/>.
        /// </summary>
        [Test]
        public void TestContainsByName([Values(false, true)] bool expectedResult)
        {
            // Arrange
            FillParameters(5);
            if (expectedResult)
                FillParametersWithParam(0, TEST_PARAMETER_NAME, 3);

            // Act & Assert
            Assert.AreEqual(expectedResult, _testedInstance.Contains(TEST_PARAMETER_NAME));
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSParameterCollection.Contains(object)"/>.
        /// </summary>
        [Test]
        public void TestContainsByValue([Values(false, true)] bool expectedResult)
        {
            // Arrange
            var parameter = new OneSParameter(TEST_PARAMETER_NAME);
            
            FillParameters(5);
            if (expectedResult)
                FillParametersWithParam(0, parameter, 3);

            // Act & Assert
            Assert.AreEqual(expectedResult, _testedInstance.Contains(parameter));
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSParameterCollection.CopyTo"/>.
        /// </summary>
        [Test]
        public void TestCopyTo()
        {
            // Arrange
            const int OFFSET = 2;
            const int COUNT = 4;
            const int ADDITIONAL_ELEMENTS = 3;
            const int BUFFER_LENGTH = OFFSET + COUNT + ADDITIONAL_ELEMENTS;

            FillParameters(COUNT);

            // Act
            var buffer = new object[BUFFER_LENGTH];
            _testedInstance.CopyTo(buffer, OFFSET);

            // Assert
            CollectionAssert.AreEqual(
                Enumerable.Repeat((object)null, OFFSET),
                buffer.Take(OFFSET)
                );
            CollectionAssert.AreEqual(
                _testedInstance,
                buffer.Skip(OFFSET).Take(COUNT)
                );
            CollectionAssert.AreEqual(
                Enumerable.Repeat((object)null, ADDITIONAL_ELEMENTS),
                buffer.Skip(OFFSET + COUNT)
                );
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.Item(string)"/>.
        /// </summary>
        [Test]
        public void TestGetItemByName()
        {
            // Arrange
            var expectedParameter = FillParametersWithParam(3, TEST_PARAMETER_NAME, 5);

            
            // Act & Assert
            Assert.AreSame(expectedParameter, _testedInstance[TEST_PARAMETER_NAME]);
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.Item(string)"/>
        /// в случае когда нет параметра с заданным именем.
        /// </summary>
        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void TestGetItemByNameWhenParameterIsNotExists()
        {
            // Arrange
            FillParameters(6);

            // Act & Assert
            var parameter = _testedInstance[TEST_PARAMETER_NAME];
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.Item(int)"/>.
        /// </summary>
        [Test]
        public void TestGetItemByIndex()
        {
            // Arrange
            const int INDEX = 3;
            var expectedParameter = FillParametersWithParam(INDEX, TEST_PARAMETER_NAME, 4);

            // Act & Assert
            Assert.AreSame(expectedParameter, _testedInstance[INDEX]);
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.IndexOf(string)"/>
        /// в случае наличия параметра
        /// </summary>
        [Test]
        public void TestIndexOfByNameWhenParameterExists()
        {
            // Arrange
            const int INDEX = 3;
            FillParametersWithParam(INDEX, TEST_PARAMETER_NAME, 3);

            // Act & Assert
            Assert.AreEqual(INDEX, _testedInstance.IndexOf(TEST_PARAMETER_NAME.ToUpper()));
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.IndexOf(string)"/>
        /// в случае отсутствия параметра с заданным именем.
        /// </summary>
        [Test]
        public void TestIndexOfByNameWhenParameterIsNotExist()
        {
            // Arrange
            FillParameters(6);

            // Act & Assert
            Assert.AreEqual(-1, _testedInstance.IndexOf(TEST_PARAMETER_NAME));
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.IndexOf(object)"/>
        /// в случае наличия параметра
        /// </summary>
        [Test]
        public void TestIndexOfByValueWhenParameterExists()
        {
            // Arrange
            const int INDEX = 3;
            var findingParameter = FillParametersWithParam(INDEX, TEST_PARAMETER_NAME, 3);

            // Act & Assert
            Assert.AreEqual(INDEX, _testedInstance.IndexOf(findingParameter));
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.IndexOf(string)"/>
        /// в случае отсутствия параметра с заданным именем.
        /// </summary>
        [Test]
        public void TestIndexOfByValueWhenParameterIsNotExist()
        {
            // Arrange
            FillParameters(7);

            // Act & Assert
            Assert.AreEqual(-1, _testedInstance.IndexOf(new OneSParameter(TEST_PARAMETER_NAME)));
        }

        private OneSParameter[] FillParametersArray(int count)
        {
            var parameters = GenerateParametersArray(count);

            _testedInstance.AddRange(parameters);

            return parameters;
        }

        private void TestRemove(int offset, OneSParameter removingParameter, int additionalCount,
                                Action<OneSParameterCollection, OneSParameter> removingAction)
        {
            // Arrange
            var array1 = FillParametersArray(offset);
            _testedInstance.Add(removingParameter);
            var array2 = FillParametersArray(additionalCount);

            // Act
            removingAction(_testedInstance, removingParameter);

            // Assert
            CollectionAssert.AreEqual(array1.Concat(array2), _testedInstance);
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.Remove(OneSParameter)"/>
        /// в случае если параметр есть в коллекции.
        /// </summary>
        [Test]
        public void TestRemoveWhenParameterExists()
        {
            TestRemove(3, new OneSParameter(TEST_PARAMETER_NAME), 4, (c, p) => c.Remove(p));
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.Remove(OneSParameter)"/>
        /// в случае если нет параметра в коллекции.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestRemoveWhenParameterIsNotExist()
        {
            // Arrange
            FillParameters(10);

            // Act
            _testedInstance.Remove(new OneSParameter(TEST_PARAMETER_NAME));
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.RemoveAt(string)"/>
        /// в случае когда параметр с данным именем есть в коллекции.
        /// </summary>
        [Test]
        public void TestRemoveAtByNameWhenExists()
        {
            TestRemove(3, new OneSParameter(TEST_PARAMETER_NAME), 4, (c, p) => c.RemoveAt(p.ParameterName));
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.RemoveAt(string)"/>
        /// в случае когда параметра с данным именем нет в коллекции.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestRemoveAtByNameWhenIsNotExist()
        {
            // Arrange
            FillParameters(6);

            // Act
            _testedInstance.RemoveAt(TEST_PARAMETER_NAME);
        }

        /// <summary>
        /// Тестирование <see cref="OneSParameterCollection.RemoveAt(int)"/>.
        /// </summary>
        [Test]
        public void TestRemoveAtByIndex()
        {
            // Arrange
            const int INDEX = 3;
            TestRemove(INDEX, new OneSParameter(TEST_PARAMETER_NAME), 3, (c, p) => c.RemoveAt(INDEX));
        }

        private OneSParameter[] FillParametersWithTestParam(int offset, int additionalCount)
        {
            var array1 = FillParametersArray(offset);
            var parameter = _testedInstance.Add(TEST_PARAMETER_NAME);
            var array2 = FillParametersArray(additionalCount);

            return array1.Concat(new[] {parameter}).Concat(array2).ToArray();
        }

        /// <summary>
        /// Тестирование установки <see cref="OneSParameterCollection.Item(string)"/>
        /// в случае когда в коллекции есть параметр с таким именем.
        /// </summary>
        [Test]
        public void TestSetIndexatorByNameWhenExists()
        {
            // Arrange
            const int INDEX = 3;
            var array = FillParametersWithTestParam(INDEX, 4);
            var newParameter = new OneSParameter(TEST_PARAMETER_NAME);

            // Act
            _testedInstance[TEST_PARAMETER_NAME] = newParameter;
            array[INDEX] = newParameter;

            // Assert
            CollectionAssert.AreEqual(
                array,
                _testedInstance
                );
        }

        /// <summary>
        /// Тестирование установки <see cref="OneSParameterCollection.Item(string)"/>
        /// в случае когда в коллекции есть параметр с таким именем,
        /// но новый параметр имеет отличное имя.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSetIndexatorByNameWhenExistsButNewParameterHasAnotherName()
        {
            // Arrange
            FillParametersWithParam(3, TEST_PARAMETER_NAME, 4);
            var newParameter = new OneSParameter(ANOTHER_PARAMETER_NAME);

            // Act
            _testedInstance[TEST_PARAMETER_NAME] = newParameter;
        }

        // TODO: Есть подозрения, что такое поведение неверно. Посмотреть как работают другие реализации.
        /// <summary>
        /// Тестирование установки <see cref="OneSParameterCollection.Item(string)"/>
        /// в случае когда в коллекции нет параметрa с таким именем.
        /// </summary>
        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void TestSetIndexatorByNameWhenIsNotExist()
        {
            // Arrange
            FillParameters(10);
            var newParameter = new OneSParameter(TEST_PARAMETER_NAME);

            // Act
            _testedInstance[newParameter.ParameterName] = newParameter;
        }

        /// <summary>
        /// Тестирование установки <see cref="OneSParameterCollection.Item(int)"/>.
        /// </summary>
        /// <param name="newParamName">Имя вставляемого параметра.</param>
        [Test]
        public void TestSetIndexatorByIndex([Values(TEST_PARAMETER_NAME, ANOTHER_PARAMETER_NAME)] string newParamName)
        {
            // Arrange
            const int INDEX = 3;
            var array = FillParametersWithTestParam(INDEX, 4);
            var newParameter = new OneSParameter(newParamName);

            // Act
            _testedInstance[INDEX] = newParameter;
            array[INDEX] = newParameter;

            // Assert
            CollectionAssert.AreEqual(
                array,
                _testedInstance
                );
        }

        /// <summary>
        /// Тестирование установки <see cref="OneSParameterCollection.Item(int)"/>
        /// в случае если новый параметр не имеет имени.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSetIndexatorByIndexWhenNameIsEmpty()
        {
            // Arrange
            FillParameters(10);

            // Act
            _testedInstance[5] = new OneSParameter();
        }

        /// <summary>
        /// Тестирование установки <see cref="OneSParameterCollection.Item(int)"/>
        /// в случае, если новый параметр не имеет имя, такое же как у незамещаемого параметра.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSetIndexatorByIndexWhenNameAlreadyExists()
        {
            // Arrange
            _testedInstance.Add(TEST_PARAMETER_NAME);
            FillParameters(10);

            // Act
            _testedInstance[5] = new OneSParameter(TEST_PARAMETER_NAME.ToLower());
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSParameterCollection.Insert(int, OneSParameter)"/>.
        /// </summary>
        [Test]
        public void TestInsert()
        {
            // Arrange
            const int INDEX = 3;
            var array = FillParametersArray(INDEX + 5);
            var newParameter = new OneSParameter(TEST_PARAMETER_NAME);

            // Act
            _testedInstance.Insert(INDEX, newParameter);

            // Assert
            CollectionAssert.AreEqual(
                array.Take(INDEX).Concat(new [] {newParameter}).Concat(array.Skip(INDEX)),
                _testedInstance
                );
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSParameterCollection.Insert(int, OneSParameter)"/>
        /// когда у вставляемого параметра нет имени.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInsertWhenNameIsEmpty()
        {
            FillParameters(10);

            _testedInstance.Insert(5, new OneSParameter());
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSParameterCollection.Insert(int, OneSParameter)"/>
        /// когда у вставляемого параметра имя совпадает с именем другого параметра.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInsertWhenNameIsAlreadyExist()
        {
            _testedInstance.Add(TEST_PARAMETER_NAME);
            FillParameters(10);

            _testedInstance.Insert(5, new OneSParameter(TEST_PARAMETER_NAME.ToLower()));
        }

        /// <summary>
        /// Тестирование изменения имени параметра в коллекции.
        /// </summary>
        [Test]
        public void TestChangeParameterName()
        {
            // Arrange
            var parameter = FillParametersWithParam(5, TEST_PARAMETER_NAME, 10);
            var snapshot = _testedInstance.ToArray();

            // Act
            parameter.ParameterName = ANOTHER_PARAMETER_NAME;

            // Assert
            CollectionAssert.AreEqual(snapshot, _testedInstance);
            
            Assert.IsFalse(_testedInstance.Contains(TEST_PARAMETER_NAME));
            Assert.IsTrue(_testedInstance.Contains(ANOTHER_PARAMETER_NAME));

            Assert.Throws<IndexOutOfRangeException>(() => { var p = _testedInstance[TEST_PARAMETER_NAME]; });
            Assert.AreSame(parameter, _testedInstance[ANOTHER_PARAMETER_NAME]);
        }

        /// <summary>
        /// Тестирование изменения имени параметра в коллекции.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestChangeParameterName([Values(null, ANOTHER_PARAMETER_NAME)] string newName)
        {
            // Arrange
            _testedInstance.Add(ANOTHER_PARAMETER_NAME);
            var parameter = FillParametersWithParam(5, TEST_PARAMETER_NAME, 10);

            // Act
            parameter.ParameterName = newName;
        }

        /// <summary>
        /// Тестирование изменения имени параметра, который был в коллекции,
        /// но был удален.
        /// </summary>
        [Test]
        public void TestChangeParameterNameAfterRemoved([Values(null, ANOTHER_PARAMETER_NAME)] string newName)
        {
            // Arrange
            _testedInstance.Add(ANOTHER_PARAMETER_NAME);
            var parameter = FillParametersWithParam(5, TEST_PARAMETER_NAME, 10);
            _testedInstance.Remove(parameter);

            // Act
            parameter.ParameterName = newName;

            // Assert
            Assert.AreEqual(newName, parameter.ParameterName);
        }
    }
}

