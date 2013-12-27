using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
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
        
        /// <summary>Результат запроса.</summary>
        private readonly IQueryResult _queryResult;

        /// <summary>Сервис перевода типов.</summary>
        private readonly IValueTypeConverter _valueTypeConverter;

        /// <summary>Текущее состояние.</summary>
        private States _currentState = States.BofOpen;

        /// <summary>Выборка из результата запроса.</summary>
        private IQueryResultSelection _queryResultSelection;

        /// <summary>Инварианты класса.</summary>
        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_queryResult != null);
            Contract.Invariant(_valueTypeConverter != null);
        }

        /// <summary>Конструктор принимающий результат запроса и сервис перевода типов.</summary>
        /// <param name="queryResult">Результат запроса данных у 1С.</param>
        /// <param name="valueTypeConverter">Сервис перевода типов.</param>
        internal OneSDataReader(IQueryResult queryResult, IValueTypeConverter valueTypeConverter)
        {
            Contract.Requires<ArgumentNullException>(queryResult != null);
            Contract.Requires<ArgumentNullException>(valueTypeConverter != null);

            _queryResult = queryResult;
            _valueTypeConverter = valueTypeConverter;
        }
        
        /// <summary>Конструктор принимающий результат запроса.</summary>
        /// <param name="queryResult">Результат запроса данных у 1С.</param>
        internal OneSDataReader(IQueryResult queryResult)
            : this(queryResult, ValueTypeConverter.Default)
        {
            Contract.Requires<ArgumentNullException>(queryResult != null);
        }

        /// <summary>Результат запроса данных у 1С.</summary>
        internal IQueryResult QueryResult
        {
            get { return _queryResult;}
        }

        /// <summary>
        /// Closes the <see cref="T:System.Data.Common.DbDataReader"/> object.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public override void Close()
        {
            if (_currentState != States.Closed)
            {
                if (_queryResultSelection != null)
                    _queryResultSelection.Dispose();

                _queryResult.Dispose();
                _currentState = States.Closed;
            }
        }

        public override DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public override bool NextResult()
        {
            throw new NotImplementedException();
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
                    if (_queryResult.IsEmpty())
                    {
                        _currentState = States.EofOpen;
                        return false;
                    }
                    _queryResultSelection = _queryResult.Choose();
                    _currentState = States.RecordOpen;
                    break;

                case States.EofOpen:
                    return false;
            }

            var result = _queryResultSelection.Next();
            if (!result)
                _currentState = States.EofOpen;

            return result;
        }

        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row.
        /// </summary>
        /// <returns>
        /// The depth of nesting for the current row.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int Depth
        {
            get { return 0; }
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

        public override int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        public override bool GetBoolean(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override byte GetByte(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override Guid GetGuid(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override short GetInt16(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetInt32(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetInt64(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override string GetString(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(int ordinal)
        {
            throw new NotImplementedException();
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
                values[index] = _queryResultSelection.Get(index);

            return count;
        }

        public override bool IsDBNull(int ordinal)
        {
            throw new NotImplementedException();
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
                using (var columns = _queryResult.Columns)
                    return columns.Count;
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

                return _queryResultSelection.Get(ordinal);
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

                return _queryResultSelection.GetByName(name);
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
                
                return !_queryResult.IsEmpty();
            }
        }

        public override decimal GetDecimal(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override double GetDouble(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override float GetFloat(int ordinal)
        {
            throw new NotImplementedException();
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
            using (var columns = _queryResult.Columns)
            using (var column = columns.Get(ordinal))
                return column.Name;
        }

        public override int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

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
            using (var columns = _queryResult.Columns)
            using (var column = columns.Get(ordinal))
            using (var valueType = column.ValueType)
            {
                return _valueTypeConverter.ConvertFrom(valueType);
            }
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
