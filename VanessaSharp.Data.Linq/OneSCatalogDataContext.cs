using System;
using System.Linq;

namespace VanessaSharp.Data.Linq
{
    // TODO Прототип
    public class OneSCatalogDataContext
    {
        public IQueryable<OneSDataRecord> GetRecords(string tableName)
        {
            throw new NotImplementedException();
        }

        public IQueryable<dynamic> GetDynamicRecords(string tableName)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Get<T>()
        {
            throw new NotImplementedException();
        }
    }
}