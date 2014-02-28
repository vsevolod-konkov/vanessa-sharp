using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace VanessaSharp.Data.Linq
{
    public class OneSDataContext : IDisposable
    {
        public OneSDataContext(OneSConnection connection)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
        }

        public OneSCatalogDataContext Catalogs
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            
        }

        public IQueryable<OneSDataEntry> GetEntries(string tableName)
        {
            throw new NotImplementedException();
        }

        public IQueryable<dynamic> GetDynamicEntries(string tableName)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Get<T>()
        {
            throw new NotImplementedException();
        }
    }

    public class OneSCatalogDataContext
    {
        public IQueryable<OneSDataEntry> GetEntries(string tableName)
        {
            throw new NotImplementedException();
        }

        public IQueryable<dynamic> GetDynamicEntries(string tableName)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Get<T>()
        {
            throw new NotImplementedException();
        }
    }
}