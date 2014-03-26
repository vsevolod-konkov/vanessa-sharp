using System;
using System.Collections.Generic;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Запись табличных данных 1С.
    /// </summary>
    public class OneSDataRecord
    {
        public string GetString(string columenName)
        {
            throw new System.NotImplementedException();
        }

        public string GetString(int index)
        {
            throw new System.NotImplementedException();
        }

        public int GetInt32(string columnName)
        {
            throw new System.NotImplementedException();
        }

        public int GetInt32(int index)
        {
            throw new System.NotImplementedException();
        }

        public double GetDouble(string columnName)
        {
            throw new System.NotImplementedException();
        }

        public double GetDouble(int index)
        {
            throw new System.NotImplementedException();
        }

        public bool GetBoolean(string columnName)
        {
            throw new System.NotImplementedException();
        }

        public bool GetBoolean(int index)
        {
            throw new System.NotImplementedException();
        }

        public DateTime GetDateTime(string columnName)
        {
            throw new System.NotImplementedException();
        }

        public DateTime GetDateTime(int index)
        {
            throw new System.NotImplementedException();
        }

        public char GetChar(string columnName)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int index)
        {
            throw new NotImplementedException();
        }

        public OneSValue this[string columnName]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public OneSValue this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public dynamic AsDynamic()
        {
            throw new NotImplementedException();
        }

        public IList<string> Fields
        {
            get { throw new NotImplementedException(); }
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public int GetValues(OneSValue[] values)
        {
            throw new NotImplementedException();
        }

        public OneSValue GetValue(int index)
        {
            throw new NotImplementedException();
        }

        public OneSValue GetValue(string columnName)
        {
            throw new NotImplementedException();
        }
    }
}