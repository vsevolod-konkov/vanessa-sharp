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
        public DataReaderFieldInfo(string name, Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);
            
            _type = type;
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
    }
}
