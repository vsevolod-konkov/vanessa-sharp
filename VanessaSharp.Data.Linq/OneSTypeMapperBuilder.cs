using System;
using System.Linq.Expressions;
using System.Reflection;

namespace VanessaSharp.Data.Linq
{
#if PROTOTYPE

    public sealed class OneSTypeMapperBuilder
    {
        public OneSDataSourceMapperBuilder<T> AddSource<T>(string sourceName)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class OneSDataSourceMapperBuilder<T>
    {
        public OneSDataSourceMapperBuilder<T> AddColumn<TProperty>(
            Expression<Func<T, TProperty>> propertyExpression, string column)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class OneSDataSourceMapperBuilder
    {
        public OneSDataSourceMapperBuilder AddColumn(PropertyInfo property, string column)
        {
            throw new NotImplementedException();
        }
    }

#endif
}
