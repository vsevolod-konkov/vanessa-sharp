using System;
using System.Linq;

namespace VanessaSharp.Data.Linq
{
#if PROTOTYPE
    
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

#endif
}