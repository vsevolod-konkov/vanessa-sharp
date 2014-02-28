using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;

namespace VanessaSharp.Data
{
    /// <summary>Коллекция параметров команды запроса к 1С.</summary>
    public sealed class OneSParameterCollection : DbParameterCollection, IEnumerable<OneSParameter>
    {
        /// <summary>
        /// Внутренний список параметров.
        /// </summary>
        private readonly List<OneSParameter> _innerList = new List<OneSParameter>();

        private static readonly StringComparer _stringComparer = StringComparer.InvariantCultureIgnoreCase;

        private static bool IsEqualParameterNames(string name1, string name2)
        {
            return _stringComparer.Compare(name1, name2) == 0;
        }

        /// <summary>Получение предиката для поиска параметра по имени.</summary>
        private static Predicate<OneSParameter> GetPredicateForFindingByName(string parameterName)
        {
            return p => IsEqualParameterNames(p.ParameterName, parameterName);
        }

        /// <summary>Приведение параметра к нужному типу.</summary>
        private static OneSParameter CastParameter(object value)
        {
            var parameter = value as OneSParameter;
            if (parameter == null)
            {
                throw new ArgumentException(string.Format(
                    "Недопустимо добавление экземпляра типа \"{0}\". Допустим добавление параметров только типа \"{1}\".",
                    value.GetType(), typeof (OneSParameter)));
            }

            return parameter;
        }

        /// <summary>Валидация параметра.</summary>
        /// <param name="parameter">Параметр.</param>
        private void ValidateParameter(OneSParameter parameter)
        {
            Contract.Requires<ArgumentNullException>(parameter != null);

            CheckValidParameterName(parameter.ParameterName);
            CheckContainsParameterName(parameter.ParameterName);
        }

        /// <summary>Проверка валидности имени.</summary>
        private static void CheckValidParameterName(string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException("Имя параметра должно быть непустым.");
            }
        }

        /// <summary>
        /// Проверка того, что параметра с данным именем нет в коллекции.
        /// </summary>
        private void CheckContainsParameterName(string parameterName)
        {
            if (ContainsParameterName(parameterName))
            {
                throw new ArgumentException(string.Format("Параметр с именем \"{0}\" уже есть в коллекции.",
                                                          parameterName));
            }
        }

        /// <summary>
        /// Получение индекса параметра по заданному имени.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        private int IndexOfByParameterName(string parameterName)
        {
            return _innerList.FindIndex(GetPredicateForFindingByName(parameterName));
        }

        /// <summary>
        /// Содержится ли в коллекции параметр с именем <paramref name="parameterName"/>.
        /// </summary>
        private bool ContainsParameterName(string parameterName)
        {
            return IndexOfByParameterName(parameterName) >= 0;
        }

        private void OnParameterNameChanging(object sender, ChangeParameterNameEventArgs args)
        {
            Contract.Requires<ArgumentNullException>(sender != null);
            Contract.Requires<ArgumentNullException>(sender is OneSParameter);
            Contract.Requires<ArgumentNullException>(args != null);

            OnParameterNameChanging((OneSParameter)sender, args.OldName, args.NewName);
        }

        private void OnParameterNameChanging(OneSParameter parameter, string oldName, string newName)
        {
            if (IsEqualParameterNames(oldName, newName))
                return;

            CheckValidParameterName(newName);
            CheckContainsParameterName(newName);
        }

        private void OnParameterAdded(OneSParameter parameter)
        {
            parameter.ParameterNameChanging += OnParameterNameChanging;
        }

        private void OnParameterRemoved(OneSParameter parameter)
        {
            parameter.ParameterNameChanging -= OnParameterNameChanging;
        }

        /// <summary>
        /// Adds the specified <see cref="T:System.Data.Common.DbParameter"/> object to the <see cref="T:System.Data.Common.DbParameterCollection"/>.
        /// </summary>
        /// <returns>
        /// The index of the <see cref="T:System.Data.Common.DbParameter"/> object in the collection.
        /// </returns>
        /// <param name="value">The <see cref="P:System.Data.Common.DbParameter.Value"/> of the <see cref="T:System.Data.Common.DbParameter"/> to add to the collection.</param><filterpriority>1</filterpriority>
        public override int Add(object value)
        {
            var parameter = CastParameter(value);

            return Add(parameter);
        }

        /// <summary>Добавление параметра.</summary>
        /// <param name="parameter">Добавляемый параметр.</param>
        public int Add(OneSParameter parameter)
        {
            Contract.Requires<ArgumentNullException>(parameter != null);
            Contract.Ensures(Contract.Result<int>() < Count);

            ValidateParameter(parameter);

            _innerList.Add(parameter);
            OnParameterAdded(parameter);

            return _innerList.Count - 1;
        }

        /// <summary>Добавление нового параметра.</summary>
        /// <param name="parameter">Добавляемый параметр.</param>
        public OneSParameter AddNew(OneSParameter parameter)
        {
            Contract.Requires<ArgumentNullException>(parameter != null);
            Contract.Ensures(Contract.Result<OneSParameter>() != null);
            Contract.Ensures(Contract.Result<OneSParameter>() != parameter);

            ValidateParameter(parameter);

            return Add(parameter.ParameterName, parameter.Value);
        }

        /// <summary>Добавление именованного параметра.</summary>
        /// <param name="name">Имя нового параметра.</param>
        public OneSParameter Add(string name)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
            Contract.Ensures(Contract.Result<OneSParameter>() != null);

            return Add(name, DBNull.Value);
        }

        /// <summary>Добавление именованного параметра со значением.</summary>
        /// <param name="name">Имя нового параметра.</param>
        /// <param name="value">Значение нового параметра.</param>
        public OneSParameter Add(string name, object value)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
            Contract.Ensures(Contract.Result<OneSParameter>() != null);

            var parameter = new OneSParameter(name, value);
            Add(parameter);

            return parameter;
        }

        /// <summary>
        /// Добавление списка параметров.
        /// </summary>
        /// <param name="parameters">Список добавляемых параметров.</param>
        public void AddRange(IEnumerable<OneSParameter> parameters)
        {
            Contract.Requires<ArgumentNullException>(parameters != null);

            foreach (var p in parameters)
                Add(p);
        }

        /// <summary>
        /// Adds an array of items with the specified values to the <see cref="T:System.Data.Common.DbParameterCollection"/>.
        /// </summary>
        /// <param name="values">An array of values of type <see cref="T:System.Data.Common.DbParameter"/> to add to the collection.</param><filterpriority>2</filterpriority>
        public override void AddRange(Array values)
        {
            foreach (var value in values)
                Add(value);
        }

        /// <summary>
        /// Removes all <see cref="T:System.Data.Common.DbParameter"/> values from the <see cref="T:System.Data.Common.DbParameterCollection"/>.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public override void Clear()
        {
            foreach (var p in _innerList)
                OnParameterRemoved(p);
            
            _innerList.Clear();
        }

        /// <summary>
        /// Indicates whether a <see cref="T:System.Data.Common.DbParameter"/> with the specified name exists in the collection.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Data.Common.DbParameter"/> is in the collection; otherwise false.
        /// </returns>
        /// <param name="value">The name of the <see cref="T:System.Data.Common.DbParameter"/> to look for in the collection.</param><filterpriority>1</filterpriority>
        public override bool Contains(string value)
        {
            CheckValidParameterName(value);
            
            return ContainsParameterName(value);
        }

        /// <summary>
        /// Indicates whether a <see cref="T:System.Data.Common.DbParameter"/> with the specified <see cref="P:System.Data.Common.DbParameter.Value"/> is contained in the collection.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Data.Common.DbParameter"/> is in the collection; otherwise false.
        /// </returns>
        /// <param name="value">The <see cref="P:System.Data.Common.DbParameter.Value"/> of the <see cref="T:System.Data.Common.DbParameter"/> to look for in the collection.</param><filterpriority>1</filterpriority>
        public override bool Contains(object value)
        {
            return _innerList.Any(p => p == value);
        }

        /// <summary>
        /// Copies an array of items to the collection starting at the specified index.
        /// </summary>
        /// <param name="array">The array of items to copy to the collection.</param><param name="index">The index in the collection to copy the items.</param><filterpriority>2</filterpriority>
        public override void CopyTo(Array array, int index)
        {
            ((ICollection) _innerList).CopyTo(array, index);
        }

        /// <summary>
        /// Specifies the number of items in the collection.
        /// </summary>
        /// <returns>
        /// The number of items in the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int Count
        {
            get { return _innerList.Count; }
        }

        /// <summary>
        /// Возвращает перечислитель, выполняющий итерацию в коллекции.
        /// </summary>
        /// <returns>
        /// Интерфейс <see cref="T:System.Collections.Generic.IEnumerator`1"/>, который может использоваться для перебора элементов коллекции.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        IEnumerator<OneSParameter> IEnumerable<OneSParameter>.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        /// <summary>
        /// Exposes the <see cref="M:System.Collections.IEnumerable.GetEnumerator"/> method, which supports a simple iteration over a collection by a .NET Framework data provider.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override IEnumerator GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        /// <summary>
        /// Returns <see cref="T:System.Data.Common.DbParameter"/> the object with the specified name.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Common.DbParameter"/> the object with the specified name.
        /// </returns>
        /// <param name="parameterName">The name of the <see cref="T:System.Data.Common.DbParameter"/> in the collection.</param>
        protected override DbParameter GetParameter(string parameterName)
        {
            var result = _innerList.Find(GetPredicateForFindingByName(parameterName));
            if (result == null)
            {
                throw new IndexOutOfRangeException(string.Format(
                    "Параметра с именем \"{0}\" не существует", parameterName));
            }

            return result;
        }

        /// <summary>
        /// Returns the <see cref="T:System.Data.Common.DbParameter"/> object at the specified index in the collection.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Common.DbParameter"/> object at the specified index in the collection.
        /// </returns>
        /// <param name="index">The index of the <see cref="T:System.Data.Common.DbParameter"/> in the collection.</param>
        protected override DbParameter GetParameter(int index)
        {
            return _innerList[index];
        }

        /// <summary>
        /// Returns the index of the <see cref="T:System.Data.Common.DbParameter"/> object with the specified name.
        /// </summary>
        /// <returns>
        /// The index of the <see cref="T:System.Data.Common.DbParameter"/> object with the specified name.
        /// </returns>
        /// <param name="parameterName">The name of the <see cref="T:System.Data.Common.DbParameter"/> object in the collection.</param><filterpriority>2</filterpriority>
        public override int IndexOf(string parameterName)
        {
            CheckValidParameterName(parameterName);

            return IndexOfByParameterName(parameterName);
        }

        /// <summary>
        /// Returns the index of the specified <see cref="T:System.Data.Common.DbParameter"/> object.
        /// </summary>
        /// <returns>
        /// The index of the specified <see cref="T:System.Data.Common.DbParameter"/> object.
        /// </returns>
        /// <param name="value">The <see cref="T:System.Data.Common.DbParameter"/> object in the collection.</param><filterpriority>2</filterpriority>
        public override int IndexOf(object value)
        {
            var parameter = CastParameter(value);

            return _innerList.IndexOf(parameter);
        }

        /// <summary>
        /// Inserts the specified index of the <see cref="T:System.Data.Common.DbParameter"/> object with the specified name into the collection at the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert the <see cref="T:System.Data.Common.DbParameter"/> object.</param><param name="value">The <see cref="T:System.Data.Common.DbParameter"/> object to insert into the collection.</param><filterpriority>1</filterpriority>
        public override void Insert(int index, object value)
        {
            var parameter = CastParameter(value);
            Insert(index, parameter);
        }

        /// <summary>
        /// Вставка параметра в коллекцию по данному индексу <paramref name="index"/>.
        /// </summary>
        public void Insert(int index, OneSParameter parameter)
        {
            Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Count);
            Contract.Requires<ArgumentNullException>(parameter != null);
            
            ValidateParameter(parameter);

            _innerList.Insert(index, parameter);
            OnParameterAdded(parameter);
        }

        /// <summary>
        /// Specifies whether the collection is a fixed size.
        /// </summary>
        /// <returns>
        /// true if the collection is a fixed size; otherwise false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Specifies whether the collection is read-only.
        /// </summary>
        /// <returns>
        /// true if the collection is read-only; otherwise false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Specifies whether the collection is synchronized.
        /// </summary>
        /// <returns>
        /// true if the collection is synchronized; otherwise false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the specified <see cref="T:System.Data.Common.DbParameter"/> object from the collection.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Data.Common.DbParameter"/> object to remove.</param><filterpriority>1</filterpriority>
        public override void Remove(object value)
        {
            Remove(CastParameter(value));
        }

        /// <summary>
        /// Удаление параметра из коллекции.
        /// </summary>
        /// <param name="parameter">Параметр.</param>
        public void Remove(OneSParameter parameter)
        {
            Contract.Requires<ArgumentNullException>(parameter != null);

            if (!_innerList.Remove(parameter))
            {
                throw new ArgumentException(string.Format(
                    "Параметр \"{0}\" нельзя удалить из коллекции, так как его в ней нет.",
                    parameter));
            }
            OnParameterRemoved(parameter);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Data.Common.DbParameter"/> object with the specified name from the collection.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="T:System.Data.Common.DbParameter"/> object to remove.</param><filterpriority>2</filterpriority>
        public override void RemoveAt(string parameterName)
        {
            CheckValidParameterName(parameterName);

            OneSParameter removingParameter;
            try
            {
                removingParameter = this[parameterName];
            }
            catch (IndexOutOfRangeException e)
            {
                throw new ArgumentException(string.Format(
                    "Параметра с именем \"{0}\" нельзя удалить из коллекции, так как нет параметра с данным именем в коллекции.",
                    parameterName), e);
            }

            _innerList.Remove(removingParameter);
            OnParameterRemoved(removingParameter);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Data.Common.DbParameter"/> object at the specified from the collection.
        /// </summary>
        /// <param name="index">The index where the <see cref="T:System.Data.Common.DbParameter"/> object is located.</param><filterpriority>2</filterpriority>
        public override void RemoveAt(int index)
        {
            _innerList.RemoveAt(index);
        }

        private void InnerSetParameter(int index, OneSParameter parameter)
        {
            var oldParameter = _innerList[index];
            if (oldParameter == parameter)
                return;

            OnParameterRemoved(oldParameter);
            _innerList[index] = parameter;
            OnParameterAdded(parameter);
        }


        /// <summary>
        /// Sets the <see cref="T:System.Data.Common.DbParameter"/> object with the specified name to a new value.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="T:System.Data.Common.DbParameter"/> object in the collection.</param><param name="value">The new <see cref="T:System.Data.Common.DbParameter"/> value.</param>
        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var parameter = CastParameter(value);
            
            if (!IsEqualParameterNames(parameterName, parameter.ParameterName))
            {
                throw new ArgumentException(string.Format(
                    "Имя вставляемого параметра \"{0}\" не совпадает со значением индекса \"{1}\".",
                    parameter.ParameterName, parameterName));
            }

            var index = IndexOf(parameterName);
            if (index < 0)
            {
                throw new IndexOutOfRangeException(string.Format(
                    "В коллекции нет параметров с именем \"{0}\".",
                    parameterName));
            }

            InnerSetParameter(index, parameter);
        }

        /// <summary>
        /// Sets the <see cref="T:System.Data.Common.DbParameter"/> object at the specified index to a new value. 
        /// </summary>
        /// <param name="index">The index where the <see cref="T:System.Data.Common.DbParameter"/> object is located.</param><param name="value">The new <see cref="T:System.Data.Common.DbParameter"/> value.</param>
        protected override void SetParameter(int index, DbParameter value)
        {
            var parameter = CastParameter(value);

            CheckValidParameterName(value.ParameterName);

            var sameNameIndex = IndexOfByParameterName(value.ParameterName);
            if (sameNameIndex >= 0 && sameNameIndex != index)
            {
                throw new ArgumentException(string.Format(
                    "В коллекции уже есть параметр с именем \"{0}\" совпадающим с именем вставляемого параметра.",
                    parameter.ParameterName));
            }

            InnerSetParameter(index, parameter);
        }

        /// <summary>
        /// Specifies the <see cref="T:System.Object"/> to be used to synchronize access to the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Object"/> to be used to synchronize access to the <see cref="T:System.Data.Common.DbParameterCollection"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override object SyncRoot
        {
            get { return ((ICollection)_innerList).SyncRoot; }
        }

        /// <summary>Индексатор по имени параметра.</summary>
        /// <param name="parameterName">Имя параметра.</param>
        public new OneSParameter this[string parameterName]
        {
            get
            {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(parameterName));
                Contract.Ensures(Contract.Result<OneSParameter>() != null);

                return (OneSParameter)base[parameterName];
            }

            set
            {
                Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(parameterName));
                Contract.Requires<ArgumentNullException>(value != null);

                base[parameterName] = value;
            }
        }

        /// <summary>
        /// Индексатор по индексу в коллекции.
        /// </summary>
        /// <param name="index">Индекс.</param>
        public new OneSParameter this[int index]
        {
            get
            {
                Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Count);
                Contract.Ensures(Contract.Result<OneSParameter>() != null);

                return (OneSParameter)base[index];
            }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Count);
                Contract.Requires<ArgumentNullException>(value != null);

                base[index] = value;
            }
        }
    }
}