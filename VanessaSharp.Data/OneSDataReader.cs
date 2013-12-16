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
        /// <summary>Состояния читателя.</summary>
        private enum States
        {
            Open,
            Close
        }

        private readonly dynamic _globalContext;
        private readonly dynamic _columns;
        private readonly dynamic _queryResultSelection;

        private States _states = States.Open;

        internal OneSDataReader(IQueryResult queryResult)
        {
            Contract.Requires<ArgumentNullException>(queryResult != null);
        }

        internal IQueryResult QueryResult
        {
            get { throw new NotImplementedException();}
        }

        [Obsolete]
        internal OneSDataReader(dynamic globalContext, dynamic columns, dynamic queryResultSelection)
        {
            _globalContext = globalContext;
            _columns = columns;
            _queryResultSelection = queryResultSelection;
        }

        /// <summary>
        /// Closes the <see cref="T:System.Data.Common.DbDataReader"/> object.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public override void Close()
        {
            FreeResource(_columns);
            FreeResource(_queryResultSelection);

            _states = States.Close;
        }

        private static void FreeResource(dynamic obj)
        {
            var disposable = obj as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        public override DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public override bool NextResult()
        {
            throw new NotImplementedException();
        }

        public override bool Read()
        {
            return _queryResultSelection.Next();
        }

        public override int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsClosed
        {
            get { return _states == States.Close; }
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

        public override int GetValues(object[] values)
        {
            var count = Math.Min(values.Length, FieldCount);

            for (var index = 0; index < count; index++)
            {
                values[index] = _queryResultSelection.Get(index);
            }

            return count;
        }

        public override bool IsDBNull(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int FieldCount
        {
            get { return _columns.Count; }
        }

        public override object this[int ordinal]
        {
            get { throw new NotImplementedException(); }
        }

        public override object this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public override bool HasRows
        {
            get { throw new NotImplementedException(); }
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

        public override string GetName(int ordinal)
        {
            return _columns.Get(ordinal).Name;
        }

        public override int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override Type GetFieldType(int ordinal)
        {
            var oneSTypes = _columns.Get(ordinal).ValueType.Types;
            var count = oneSTypes.Count;
            var oneSType = oneSTypes.Get(0);

            var oneSStringTypeName = _globalContext.String(oneSType);
            if (oneSStringTypeName == "Строка")
                return typeof(string);

            return typeof(object);
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
