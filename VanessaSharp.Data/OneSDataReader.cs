using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using VanessaSharp.Data.DataReading;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    /// <summary>Читатель данных, являющихся результатом запроса к 1С.</summary>
    public sealed class OneSDataReader : DbDataReader
    {
        /// <summary>Состояния.</summary>
        private enum States
        {
            BofOpen,
            RecordOpen,
            EofOpen,
            Closed
        }

        /// <summary>
        /// Поставщик данных записей результата запроса.
        /// </summary>
        private readonly IDataRecordsProvider _dataRecordsProvider;

        /// <summary>Сервис перевода значений.</summary>
        private readonly IValueConverter _valueConverter;

        /// <summary>Текущее состояние.</summary>
        private States _currentState = States.BofOpen;

        /// <summary>Курсор.</summary>
        private IDataCursor _dataCursor;

        /// <summary>Инварианты класса.</summary>
        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_dataRecordsProvider != null);
            Contract.Invariant(_valueConverter != null);
        }

        /// <summary>
        /// Конструктор для модульного тестирования.
        /// </summary>
        /// <param name="dataRecordsProvider">Поставщик данных записей результата запроса.</param>
        /// <param name="valueConverter">Сервис перевода значений.</param>
        /// <param name="isTablePart">Является ли читатель - читателем табличной части</param>
        internal OneSDataReader(
            IDataRecordsProvider dataRecordsProvider,
            IValueConverter valueConverter,
            bool isTablePart)
        {
            Contract.Requires<ArgumentNullException>(dataRecordsProvider != null);
            Contract.Requires<ArgumentNullException>(valueConverter != null);

            _dataRecordsProvider = dataRecordsProvider;
            _valueConverter = valueConverter;
            _isTablePart = isTablePart;
        }

        /// <summary>Конструктор принимающий провайдер записей.</summary>
        /// <param name="dataRecordsProvider">Поставщик данных записей результата запроса.</param>
        /// <param name="isTablePart">Является ли читатель - читателем табличной части</param>
        private OneSDataReader(IDataRecordsProvider dataRecordsProvider, bool isTablePart)
            : this(
                    dataRecordsProvider,
                    Data.ValueConverter.Default,
                    isTablePart)
        {
            Contract.Requires<ArgumentNullException>(dataRecordsProvider != null);
        }

        /// <summary>Конструктор принимающий результат запроса.</summary>
        /// <param name="queryResult">Результат запроса.</param>
        /// <param name="queryResultIteration">Стратегия обхода записей.</param>
        /// <param name="isTablePart">Является ли читатель - читателем табличной части.</param>
        private OneSDataReader(IQueryResult queryResult, QueryResultIteration queryResultIteration, bool isTablePart)
            : this(
                    new QueryResultDataRecordsProvider(queryResult, queryResultIteration), 
                    isTablePart)
        {
            Contract.Requires<ArgumentNullException>(queryResult != null);
        }

        /// <summary>Создание читателя верхнего уровня, по результату запроса.</summary>
        /// <param name="queryResult">Результат запроса данных у 1С.</param>
        /// <param name="queryResultIteration">Стратегия перебора записей.</param>
        internal static OneSDataReader CreateRootDataReader(
            IQueryResult queryResult, QueryResultIteration queryResultIteration)
        {
            Contract.Requires<ArgumentNullException>(queryResult != null);
            Contract.Ensures(Contract.Result<OneSDataReader>() != null);

            return new OneSDataReader(
                queryResult, queryResultIteration, false);
        }

        /// <summary>
        /// Создание читателя записей-потомков.
        /// </summary>
        /// <param name="dataRecordsProvider">
        /// Поставщик записей-потомков.
        /// </param>
        private static OneSDataReader CreateDescendantsDataReader(
            IDataRecordsProvider dataRecordsProvider)
        {
            Contract.Requires<ArgumentNullException>(dataRecordsProvider != null);
            Contract.Ensures(Contract.Result<OneSDataReader>() != null);
            
            return new OneSDataReader(dataRecordsProvider, false);
        }

        /// <summary>Создание читателя табличной части, по результату запроса.</summary>
        /// <param name="queryResult">Результат запроса данных у 1С.</param>
        internal static OneSDataReader CreateTablePartDataReader(IQueryResult queryResult)
        {
            Contract.Requires<ArgumentNullException>(queryResult != null);
            Contract.Ensures(Contract.Result<OneSDataReader>() != null);
            
            return new OneSDataReader(queryResult, QueryResultIteration.Default, true);
        }

        /// <summary>Поставщик данных записей результата запроса.</summary>
        internal IDataRecordsProvider DataRecordsProvider 
        {
            get { return _dataRecordsProvider;}
        }

        /// <summary>Конвертер значений.</summary>
        public IValueConverter ValueConverter
        {
            get { return _valueConverter; }
        }

        /// <summary>
        /// Closes the <see cref="T:System.Data.Common.DbDataReader"/> object.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public override void Close()
        {
            if (_currentState != States.Closed)
            {
                if (_dataCursor != null)
                    _dataCursor.Dispose();

                _dataRecordsProvider.Dispose();
                _currentState = States.Closed;
            }
        }

        /// <summary>
        /// Возвращает объект <see cref="T:System.Data.DataTable"/>, описывающий метаданные столбца объекта <see cref="T:System.Data.Common.DbDataReader"/>.
        /// </summary>
        /// <remarks>В текущей версии не реализовано.</remarks>
        /// <returns>
        /// Объект <see cref="T:System.Data.DataTable"/>, описывающий метаданные столбца.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">Объект <see cref="T:System.Data.SqlClient.SqlDataReader"/> закрыт. </exception>
        /// <filterpriority>1</filterpriority>
        /// <exception cref="NotImplementedException"/>
        [CurrentVersionNotImplemented]
        public override DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Advances the reader to the next result when reading the results of a batch of statements.
        /// </summary>
        /// <returns>
        /// true if there are more result sets; otherwise false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool NextResult()
        {
            // TODO: В 8.3 появились пакетные запросы, надо реализовать.

            if (_currentState == States.Closed)
            {
                throw new InvalidOperationException(
                    "Недопустимо вызывать NextResult в закрытом состоянии.");
            }

            return false;
        }

        /// <summary>
        /// Advances the reader to the next record in a result set.
        /// </summary>
        /// <returns>
        /// true if there are more rows; otherwise false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool Read()
        {
            switch (_currentState)
            {
                case States.BofOpen:
                    if (!_dataRecordsProvider.TryCreateCursor(out _dataCursor))
                    {
                        _currentState = States.EofOpen;
                        return false;
                    }
                    
                    _currentState = States.RecordOpen;
                    break;

                case States.EofOpen:
                    return false;

                case States.Closed:
                    throw new InvalidOperationException(
                        "Недопустимо вызывать метод GetFieldType в закрытом состоянии.");
            }

            var result = _dataCursor.Next();
            if (!result)
                _currentState = States.EofOpen;

            return result;
        }

        /// <summary>
        /// Получение читателя
        /// записей-потомков для текущей записи.
        /// </summary>
        /// <param name="queryResultIteration">
        /// Стратегия обхода записей.
        /// </param>
        public OneSDataReader GetDescendantsReader(
            QueryResultIteration queryResultIteration = QueryResultIteration.Default)
        {
            Contract.Ensures(Contract.Result<OneSDataReader>() != null);

            return InternalGetDescendantsReader(queryResultIteration, null, null);
        }

        /// <summary>
        /// Получение читателя
        /// записей-потомков для текущей записи.
        /// </summary>
        /// <param name="queryResultIteration">
        /// Стратегия обхода записей.
        /// </param>
        /// <param name="groupNames">
        /// Имена группировок по которым будет производиться обход.
        /// </param>
        public OneSDataReader GetDescendantsReader(
            QueryResultIteration queryResultIteration, params string[] groupNames)
        {
            Contract.Ensures(Contract.Result<OneSDataReader>() != null);

            return InternalGetDescendantsReader(queryResultIteration, groupNames, null);
        }

        /// <summary>
        /// Получение читателя
        /// записей-потомков для текущей записи.
        /// </summary>
        /// <param name="queryResultIteration">
        /// Стратегия обхода записей.
        /// </param>
        /// <param name="groupNamesAndValues">
        /// Имена и значения группировок по которым будет производиться обход.
        /// </param>
        public OneSDataReader GetDescendantsReader(
            QueryResultIteration queryResultIteration, params Tuple<string, string>[] groupNamesAndValues)
        {
            Contract.Ensures(Contract.Result<OneSDataReader>() != null);

            return InternalGetDescendantsReader(
                queryResultIteration,
                groupNamesAndValues.Select(p => p.Item1),
                groupNamesAndValues.Select(p => p.Item2)
                );
        }

        /// <summary>
        /// Внутренняя реализация
        /// получения читателя
        /// записей-потомков для текущей записи.
        /// </summary>
        /// <param name="queryResultIteration">
        /// Стратегия обхода записей.
        /// </param>
        /// <param name="groupNames">
        /// Имена группировок.
        /// </param>
        /// <param name="groupValues">
        /// Имена значений.
        /// </param>
        /// <returns></returns>
        private OneSDataReader InternalGetDescendantsReader(
            QueryResultIteration queryResultIteration,
            IEnumerable<string> groupNames,
            IEnumerable<string> groupValues)
        {
            if (_currentState == States.RecordOpen)
            {
                return CreateDescendantsDataReader(
                    _dataCursor.GetDescendantRecordsProvider(queryResultIteration, groupNames, groupValues)
                    );
            }

            throw new InvalidOperationException(
                string.Format("Недопустим вызов метода \"{1}\" в состоянии \"{0}\".",
                _currentState, MethodBase.GetCurrentMethod().Name));
        }

        /// <summary>
        /// Получение значения свойства открытого курсора.
        /// </summary>
        /// <typeparam name="T">Тип свойства.</typeparam>
        /// <param name="propertyAccessor">Получатель значения.</param>
        /// <param name="propertyName">Имя свойства.</param>
        private T GetCursorProperty<T>(Func<IDataCursor, T> propertyAccessor, string propertyName)
        {
            Contract.Assert(propertyAccessor != null);

            if (_currentState == States.RecordOpen)
                return propertyAccessor(_dataCursor);

            throw new InvalidOperationException(
                string.Format("Недопустимо получение свойства \"{1}\" в состоянии \"{0}\".",
                _currentState, propertyName));
        }
        
        /// <summary>
        /// Уровень записи.
        /// </summary>
        public int Level
        {
            get
            {
                return GetCursorProperty(c => c.Level, "Level");
            }
        }

        /// <summary>
        /// Имя группы текущей записи.
        /// </summary>
        public string GroupName
        {
            get
            {
                return GetCursorProperty(c => c.GroupName, "GroupName");
            }
        }

        /// <summary>
        /// Тип текущей записи.
        /// </summary>
        public SelectRecordType RecordType
        {
            get
            {
                return GetCursorProperty(c => c.RecordType, "RecordType");
            }
        }

        /// <summary>
        /// Является ли читателем табличной части.
        /// </summary>
        public bool IsTablePart
        {
            get { return _isTablePart; }
        }
        private readonly bool _isTablePart;

        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row.
        /// </summary>
        /// <returns>
        /// The depth of nesting for the current row.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int Depth
        {
            get
            {
                return IsTablePart
                    ? Level + 1 : Level;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Data.Common.DbDataReader"/> is closed.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Data.Common.DbDataReader"/> is closed; otherwise false.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.SqlClient.SqlDataReader"/> is closed. </exception><filterpriority>1</filterpriority>
        public override bool IsClosed
        {
            get { return _currentState == States.Closed; }
        }

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement. 
        /// </summary>
        /// <returns>
        /// The number of rows changed, inserted, or deleted. -1 for SELECT statements; 0 if no rows were affected or the statement failed.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int RecordsAffected
        {
            get { return -1; }
        }

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
        /// <filterpriority>1</filterpriority>
        public override bool GetBoolean(int ordinal)
        {
            return _valueConverter.ToBoolean(
                GetValue(ordinal));
        }

        /// <summary>
        /// Gets the value of the specified column as a byte.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
        /// <filterpriority>1</filterpriority>
        public override byte GetByte(int ordinal)
        {
            return _valueConverter.ToByte(
                GetValue(ordinal));
        }

        /// <summary>
        /// Считывает поток байтов из указанного столбца, начиная с местоположения, указанного параметром <paramref name="dataOffset"/>, в буфер, начиная с местоположения, указанного параметром <paramref name="bufferOffset"/>.
        /// </summary>
        /// <remarks>В текущей версии не реализовано.</remarks>
        /// <returns>
        /// Фактическое количество считанных байтов.
        /// </returns>
        /// <param name="ordinal">Порядковый номер (с нуля) столбца.</param>
        /// <param name="dataOffset">Индекс в строке, с которого начинается операция считывания.</param>
        /// <param name="buffer">Буфер, в который копируются данные.</param><param name="bufferOffset">Индекс для буфера, в который будут копироваться данные.</param>
        /// <param name="length">Наибольшее число символов для чтения.</param><exception cref="T:System.InvalidCastException">Указанное приведение недопустимо. </exception>
        /// <filterpriority>1</filterpriority>
        /// <exception cref="NotImplementedException"/>
        [CurrentVersionNotImplemented]
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            // TODO: Копипаст
            if (_currentState != States.RecordOpen)
            {
                throw new InvalidOperationException(
                    "Невозможно получить значение поля так как экземпляр не находится на позиции строки данных.");
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Получает значение указанного столбца в виде одного символа.
        /// </summary>
        /// <returns>
        /// Значение указанного столбца.
        /// </returns>
        /// <param name="ordinal">Порядковый номер столбца (начиная с нуля).</param>
        /// <exception cref="T:System.InvalidCastException">Указанное приведение недопустимо. </exception>
        /// <filterpriority>1</filterpriority>
        public override char GetChar(int ordinal)
        {
            return _valueConverter.ToChar(
                GetValue(ordinal));
        }

        /// <summary>
        /// Считывает поток символов из указанного столбца, начиная с местоположения,
        /// указанного параметром <paramref name="dataOffset"/>, в буфер, 
        /// начиная с местоположения, указанного параметром <paramref name="bufferOffset"/>.
        /// </summary>
        /// <remarks>В текущей версии не реализовано.</remarks>
        /// <returns>
        /// Фактическое количество считанных символов.
        /// </returns>
        /// <param name="ordinal">Порядковый номер (с нуля) столбца.</param>
        /// <param name="dataOffset">Индекс в строке, с которого начинается операция считывания.</param>
        /// <param name="buffer">Буфер, в который копируются данные.</param>
        /// <param name="bufferOffset">Индекс для буфера, в который будут копироваться данные.</param>
        /// <param name="length">Наибольшее число символов для чтения.</param>
        /// <filterpriority>1</filterpriority>
        /// <exception cref="NotImplementedException"/>
        [CurrentVersionNotImplemented]
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            // TODO: Копипаст
            if (_currentState != States.RecordOpen)
            {
                throw new InvalidOperationException(
                    "Невозможно получить значение поля так как экземпляр не находится на позиции строки данных.");
            }
            
            throw new NotImplementedException();
        }

        /// <summary>
        /// Возвращает значение заданного столбца в виде глобального уникального идентификатора (GUID).
        /// </summary>
        /// <remarks>В текущей версии не реализовано.</remarks>
        /// <returns>
        /// Значение указанного столбца.
        /// </returns>
        /// <param name="ordinal">Порядковый номер столбца, начиная с нуля.</param>
        /// <exception cref="T:System.InvalidCastException">Указанное приведение недопустимо. </exception>
        /// <filterpriority>1</filterpriority>
        /// <exception cref="NotImplementedException"/>
        [CurrentVersionNotImplemented]
        public override Guid GetGuid(int ordinal)
        {
            // TODO: Копипаст
            if (_currentState != States.RecordOpen)
            {
                throw new InvalidOperationException(
                    "Невозможно получить значение поля так как экземпляр не находится на позиции строки данных.");
            }
            
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of the specified column as a 16-bit signed integer.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
        /// <filterpriority>2</filterpriority>
        public override short GetInt16(int ordinal)
        {
            return _valueConverter.ToInt16(
                GetValue(ordinal));
        }

        /// <summary>
        /// Gets the value of the specified column as a 32-bit signed integer.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
        /// <filterpriority>1</filterpriority>
        public override int GetInt32(int ordinal)
        {
            return _valueConverter.ToInt32(
                GetValue(ordinal));
        }

        /// <summary>
        /// Gets the value of the specified column as a 64-bit signed integer.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
        /// <filterpriority>2</filterpriority>
        public override long GetInt64(int ordinal)
        {
            return _valueConverter.ToInt64(
                GetValue(ordinal));
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="T:System.DateTime"/> object.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
        /// <filterpriority>1</filterpriority>
        public override DateTime GetDateTime(int ordinal)
        {
            return _valueConverter.ToDateTime(
                GetValue(ordinal));
        }

        /// <summary>
        /// Gets the value of the specified column as an instance of <see cref="T:System.String"/>.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
        /// <filterpriority>1</filterpriority>
        public override string GetString(int ordinal)
        {
            return _valueConverter.ToString(
                GetValue(ordinal));
        }

        /// <summary>
        /// Gets the value of the specified column as an instance of <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><filterpriority>1</filterpriority>
        public override object GetValue(int ordinal)
        {
            // TODO: Копипаст с индексатором и методом GetValues. Требуется рефакторинг
            
            if (_currentState != States.RecordOpen)
            {
                throw new InvalidOperationException(
                    "Невозможно получить значение поля так как экземпляр не находится на позиции строки данных.");
            }

            return _dataCursor.GetValue(ordinal);
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current row.
        /// </summary>
        /// <returns>
        /// The number of instances of <see cref="T:System.Object"/> in the array.
        /// </returns>
        /// <param name="values">
        /// An array of <see cref="T:System.Object"/> into which to copy the attribute columns.
        /// </param>
        /// <filterpriority>1</filterpriority>
        public override int GetValues(object[] values)
        {
            if (_currentState != States.RecordOpen)
            {
                throw new InvalidOperationException(
                    "Вызов метода GetValues недопустимо в данном состоянии.");
            }
            
            var count = Math.Min(values.Length, FieldCount);

            for (var index = 0; index < count; index++)
                values[index] = _dataCursor.GetValue(index);

            return count;
        }

        /// <summary>
        /// Gets a value that indicates whether the column contains nonexistent or missing values.
        /// </summary>
        /// <returns>
        /// true if the specified column is equivalent to <see cref="T:System.DBNull"/>; otherwise false.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><filterpriority>1</filterpriority>
        public override bool IsDBNull(int ordinal)
        {
            return GetValue(ordinal) is DBNull;
        }

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        /// <returns>
        /// The number of columns in the current row.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">There is no current connection to an instance of SQL Server. </exception><filterpriority>1</filterpriority>
        public override int FieldCount
        {
            get
            {
                if (_currentState == States.Closed)
                {
                    throw new InvalidOperationException(
                        "Недопустимо получить значение свойства FieldCount в закрытом состоянии.");
                }

                return _dataRecordsProvider.Fields.Count;
            }
        }

        /// <summary>
        /// Gets the value of the specified column as an instance of <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <exception cref="T:System.IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>.
        /// </exception>
        /// <filterpriority>1</filterpriority>
        public override object this[int ordinal]
        {
            get
            {
                if (_currentState != States.RecordOpen)
                {
                    throw new InvalidOperationException(
                        "Невозможно получить значение свойства Item так как экземпляр не находится на позиции строки данных.");
                }

                return _dataCursor.GetValue(ordinal);
            }
        }

        /// <summary>
        /// Gets the value of the specified column as an instance of <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="name">
        /// The name of the column.
        /// </param>
        /// <exception cref="T:System.IndexOutOfRangeException">
        /// No column with the specified name was found.
        /// </exception>
        /// <filterpriority>1</filterpriority>
        public override object this[string name]
        {
            get
            {
                if (_currentState != States.RecordOpen)
                {
                    throw new InvalidOperationException(
                        "Невозможно получить значение свойства Item так как экземпляр не находится на позиции строки данных.");
                }

                return _dataCursor.GetValue(name);
            }
        }

        /// <summary>
        /// Gets a value that indicates whether this <see cref="T:System.Data.Common.DbDataReader"/> contains one or more rows.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Data.Common.DbDataReader"/> contains one or more rows; otherwise false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool HasRows
        {
            get
            {
                if (_currentState == States.Closed)
                {
                    throw new InvalidOperationException(
                        "Невозможно получить значение свойства HasRows поскольку экземпляр находится в закрытом состоянии.");
                }
                
                return _dataRecordsProvider.HasRecords;
            }
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="T:System.Decimal"/> object.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
        /// <filterpriority>1</filterpriority>
        public override decimal GetDecimal(int ordinal)
        {
            return _valueConverter.ToDecimal(
                GetValue(ordinal));
        }

        /// <summary>
        /// Gets the value of the specified column as a double-precision floating point number.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
        /// <filterpriority>1</filterpriority>
        public override double GetDouble(int ordinal)
        {
            return _valueConverter.ToDouble(
                GetValue(ordinal));
        }

        /// <summary>
        /// Gets the value of the specified column as a single-precision floating point number.
        /// </summary>
        /// <returns>
        /// The value of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception>
        /// <filterpriority>2</filterpriority>
        public override float GetFloat(int ordinal)
        {
            return _valueConverter.ToFloat(
                GetValue(ordinal));
        }

        /// <summary>
        /// Получение читателя табличной части, которое находится в поле
        /// <paramref name="ordinal"/>.
        /// </summary>
        /// <param name="ordinal">Порядковый номер поля с табличной частью.</param>
        /// <exception cref="T:System.InvalidCastException">Если поле не является табличной частью.</exception>
        public OneSDataReader GetDataReader(int ordinal)
        {
            Contract.Ensures(Contract.Result<OneSDataReader>() != null);

            if (GetFieldType(ordinal) != typeof(OneSDataReader))
            {
                throw new InvalidOperationException(
                    string.Format(
                    "Поле с индексом \"{0}\" не является табличной частью.",
                    ordinal));
            }

            return (OneSDataReader)GetValue(ordinal);
        }

        /// <summary>
        /// Возвращает объект <see cref="T:System.Data.Common.DbDataReader"/> для запрошенного порядкового номера столбца, который может быть переопределен с помощью зависящей от поставщика реализации.
        /// </summary>
        /// <returns>
        /// Объект <see cref="T:System.Data.Common.DbDataReader"/>.
        /// </returns>
        /// <param name="ordinal">Порядковый номер (с нуля) столбца.</param>
        protected override DbDataReader GetDbDataReader(int ordinal)
        {
            return GetDataReader(ordinal);
        }

        /// <summary>
        /// Gets the name of the column, given the zero-based column ordinal.
        /// </summary>
        /// <returns>
        /// The name of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><filterpriority>1</filterpriority>
        public override string GetName(int ordinal)
        {
            if (_currentState == States.Closed)
            {
                throw new InvalidOperationException(
                    "Недопустимо вызывать метод GetName в закрытом состоянии.");
            }

            return _dataRecordsProvider.Fields[ordinal].Name;
        }

        /// <summary>
        /// Gets the column ordinal given the name of the column.
        /// </summary>
        /// <returns>
        /// The zero-based column ordinal.
        /// </returns>
        /// <param name="name">The name of the column.</param><exception cref="T:System.IndexOutOfRangeException">The name specified is not a valid column name.</exception><filterpriority>1</filterpriority>
        public override int GetOrdinal(string name)
        {
            if (_currentState == States.Closed)
            {
                throw new InvalidOperationException(
                    "Недопустимо вызывать метод GetOrdinal в закрытом состоянии.");
            }

            var result = _dataRecordsProvider.Fields.IndexOf(name);
            if (result == -1)
            {
                throw new IndexOutOfRangeException(string.Format(
                        "Колонки с именем \"{0}\" не существует.", name));
            }

            return result;
        }

        /// <summary>
        /// Получает имя типа данных указанного столбца.
        /// </summary>
        /// <remarks>В текущей версии не реализовано.</remarks>
        /// <returns>
        /// Строка, представляющая имя типа данных.
        /// </returns>
        /// <param name="ordinal">Порядковый номер столбца, начиная с нуля.</param>
        /// <exception cref="T:System.InvalidCastException">Указанное приведение недопустимо. </exception>
        /// <filterpriority>1</filterpriority>
        /// <exception cref="NotImplementedException"/>
        [CurrentVersionNotImplemented]
        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the data type of the specified column.
        /// </summary>
        /// <returns>
        /// The data type of the specified column.
        /// </returns>
        /// <param name="ordinal">The zero-based column ordinal.</param><exception cref="T:System.InvalidCastException">The specified cast is not valid. </exception><filterpriority>1</filterpriority>
        public override Type GetFieldType(int ordinal)
        {
            if (_currentState == States.Closed)
            {
                throw new InvalidOperationException(
                    "Недопустимо вызывать метод GetFieldType в закрытом состоянии.");
            }

            return _dataRecordsProvider.Fields[ordinal].Type;
        }

        /// <summary>
        /// Возвращает значение <see cref="T:System.Collections.IEnumerator"/>, которое можно использовать для итерации элементов строк в модуле чтения данных.
        /// </summary>
        /// <remarks>В текущей версии не реализовано.</remarks>
        /// <returns>
        /// Значение <see cref="T:System.Collections.IEnumerator"/>, которое может использоваться для итерации элементов строк в модуле чтения данных.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        /// <exception cref="NotImplementedException"/>
        [CurrentVersionNotImplemented]
        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
