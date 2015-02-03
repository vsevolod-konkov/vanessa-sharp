using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Реализация <see cref="IDataCursor"/>.
    /// </summary>
    internal sealed class DataCursor : IDataCursor
    {
        /// <summary>
        /// Буфер значений с ленивой загрузкой.
        /// </summary>
        private readonly LazyBuffer _buffer;

        /// <summary>Конструктор.</summary>
        /// <param name="dataReaderFieldInfoCollection">Коллекция информации о полях читателя данных.</param>
        /// <param name="queryResultSelection">Выборка из результата запроса.</param>
        /// <param name="oneSObjectSpecialConverter">Специальный конвертер для объектов 1С.</param>
        public DataCursor(
            IDataReaderFieldInfoCollection dataReaderFieldInfoCollection, 
            IQueryResultSelection queryResultSelection,
            IOneSObjectSpecialConverter oneSObjectSpecialConverter)
        {
            Contract.Requires<ArgumentNullException>(dataReaderFieldInfoCollection != null);
            Contract.Requires<ArgumentNullException>(queryResultSelection != null);

            _dataReaderFieldInfoCollection = dataReaderFieldInfoCollection;
            _queryResultSelection = queryResultSelection;

            _buffer = new LazyBuffer(GetValueFunctions(oneSObjectSpecialConverter));
        }

        /// <summary>
        /// Получение массива функций получения значения полей.
        /// </summary>
        private Func<object>[] GetValueFunctions(IOneSObjectSpecialConverter oneSObjectSpecialConverter)
        {
            var fieldsCount = _dataReaderFieldInfoCollection.Count;

            var result = new Func<object>[fieldsCount];
            for (var index = 0; index < fieldsCount; index++)
            {
                var ordinal = index;

                Func<object> valueReader = () => _queryResultSelection.Get(ordinal);
                var valueConverter = GetValueConverter(oneSObjectSpecialConverter, _dataReaderFieldInfoCollection[index].Type);

                result[ordinal] = (valueConverter == null)
                                      ? valueReader
                                      : () => valueConverter(valueReader());
            }

            return result;
        }

        private static Func<object, object> GetValueConverter(IOneSObjectSpecialConverter oneSObjectSpecialConverter, Type desiredType)
        {
            if (desiredType == typeof(OneSDataReader))
                return oneSObjectSpecialConverter.ToDataReader;

            return null;
        }

        /// <summary>
        /// Коллекция информации о полях читателя данных.
        /// </summary>
        internal IDataReaderFieldInfoCollection DataReaderFieldInfoCollection
        {
            get { return _dataReaderFieldInfoCollection; }
        }
        private readonly IDataReaderFieldInfoCollection _dataReaderFieldInfoCollection;

        /// <summary>Выборка из результата запроса.</summary>
        internal IQueryResultSelection QueryResultSelection
        {
            get { return _queryResultSelection; }
        }
        private readonly IQueryResultSelection _queryResultSelection;

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _queryResultSelection.Dispose();
        }

        /// <summary>Переход на следующую запись.</summary>
        /// <returns>
        /// Возвращает <c>true</c>, если следующая запись была и переход состоялся.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        public bool Next()
        {
            _buffer.Reset();
            
            return _queryResultSelection.Next();
        }

        /// <summary>
        /// Получение значения поля записи.
        /// </summary>
        /// <param name="ordinal">
        /// Порядковый номер поля.
        /// </param>
        public object GetValue(int ordinal)
        {
            if (ordinal < 0 || ordinal >= _dataReaderFieldInfoCollection.Count)
                throw new ArgumentOutOfRangeException("ordinal");

            return _buffer[ordinal];
        }

        /// <summary>
        /// Получение значения поля записи.
        /// </summary>
        /// <param name="name">
        /// Имя поля.
        /// </param>
        public object GetValue(string name)
        {
            var ordinal = _dataReaderFieldInfoCollection.IndexOf(name);
            if (ordinal == -1)
                throw new ArgumentOutOfRangeException("name");
            
            return _buffer[ordinal];
        }

        /// <summary>
        /// Уровень текущей записи.
        /// </summary>
        public int Level 
        { 
            get
            {
                // TODO: Буферизация
                return _queryResultSelection.Level;
            } 
        }

        /// <summary>
        /// Имя группировки текущей записи.
        /// </summary>
        public string GroupName
        {
            get
            {
                // TODO: Буферизация
                return _queryResultSelection.Group;
            }
        }

        /// <summary>
        /// Тип текущей записи.
        /// </summary>
        public SelectRecordType RecordType
        {
            get
            {
                // TODO: Буферизация
                return _queryResultSelection.RecordType;
            }
        }

        #region Вспомогательные типы

        /// <summary>
        /// Ленивый бефер значений.
        /// </summary>
        private sealed class LazyBuffer
        {
            /// <summary>
            /// Массив функций получения значения полей.
            /// </summary>
            private readonly Func<object>[] _valueFunctions; 
            
            /// <summary>
            /// Буфер ленивых значений.
            /// </summary>
            private readonly Lazy<object>[] _buffer;

            /// <summary>Конструктор.</summary>
            /// <param name="valueFunctions">
            /// Массив функций получения значения полей.
            /// </param>
            public LazyBuffer(Func<object>[] valueFunctions)
            {
                Contract.Requires<ArgumentNullException>(valueFunctions != null);

                _valueFunctions = valueFunctions;
                _buffer = new Lazy<object>[valueFunctions.Length];

                InitBuffer();
            }

            /// <summary>Индексатор.</summary>
            public object this[int ordinal]
            {
                get { return _buffer[ordinal].Value; }
            }

            /// <summary>
            /// Сброс значений в буфере.
            /// </summary>
            public void Reset()
            {
                // TODO: Подумать
                //foreach (var lazyValue in _buffer)
                //{
                //    if (lazyValue.IsValueCreated)
                //    {
                //        var disposableValue = lazyValue.Value as IDisposable;
                //        if (disposableValue != null)
                //            disposableValue.Dispose();
                //    }
                //}
                
                InitBuffer();
            }

            private void InitBuffer()
            {
                for (var index = 0; index < _valueFunctions.Length; index++)
                {
                    _buffer[index] = new Lazy<object>(_valueFunctions[index]);
                }
            }
        }

        #endregion
    }
}
