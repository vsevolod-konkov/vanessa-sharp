using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Информация о колонке в читателе <see cref="OneSDataReader"/>.
    /// </summary>
    internal sealed class DataReaderFieldInfo
    {
        /// <summary>Конструктор.</summary>
        /// <param name="name">Имя поля.</param>
        /// <param name="type">Тип поля.</param>
        /// <param name="rawValueConverter">Конвертер сырого значения из 1С.</param>
        public DataReaderFieldInfo(string name, Type type, Func<object, object> rawValueConverter)
        {
            Contract.Requires<ArgumentNullException>(type != null);
            
            _type = type;
            _rawValueConverter = rawValueConverter;
            _name = name;
        }

        /// <summary>Имя поля.</summary>
        public string Name
        {
            get { return _name; }
        }
        private readonly string _name;

        /// <summary>Тип поля.</summary>
        public Type Type
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);

                return _type;
            }
        }
        private readonly Type _type;

        /// <summary>Конвертер сырого значения из 1С.</summary>
        public Func<object, object> RawValueConverter
        {
            get { return _rawValueConverter; }    
        }
        private readonly Func<object, object> _rawValueConverter;
    }
}
