using System;
using System.Data.Common;
using System.Collections;

namespace VsevolodKonkov.OneSSharp.Data
{
    /// <summary>Коллекция парамтеров команды запроса к 1С.</summary>
    public sealed class OneSParameterCollection : DbParameterCollection
    {
        public override int Add(object value)
        {
            throw new NotImplementedException();
        }

        public override void AddRange(Array values)
        {
            throw new NotImplementedException();
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public override bool Contains(string value)
        {
            throw new NotImplementedException();
        }

        public override bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public override void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public override int Count
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            throw new NotImplementedException();
        }

        protected override DbParameter GetParameter(int index)
        {
            throw new NotImplementedException();
        }

        public override int IndexOf(string parameterName)
        {
            throw new NotImplementedException();
        }

        public override int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public override void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public override bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public override void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public override void RemoveAt(string parameterName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            throw new NotImplementedException();
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            throw new NotImplementedException();
        }

        public override object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }
    }
}