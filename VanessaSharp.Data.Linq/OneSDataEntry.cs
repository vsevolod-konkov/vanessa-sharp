using System;

namespace VanessaSharp.Data.Linq
{
    public class OneSDataEntry
    {
        public string GetString(string columenName)
        {
            throw new System.NotImplementedException();
        }

        public int GetInt32(string columnName)
        {
            throw new System.NotImplementedException();
        }

        public double GetDouble(string columnName)
        {
            throw new System.NotImplementedException();
        }

        public bool GetBoolean(string columnName)
        {
            throw new System.NotImplementedException();
        }

        public DateTime GetDateTime(string columnName)
        {
            throw new System.NotImplementedException();
        }

        public char GetChar(string columnName)
        {
            throw new NotImplementedException();
        }

        public object this[string columnName]
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
    }
}