using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    /// <summary>
    /// Стандартная реализация сервиса <see cref="ITypeDescriptionConverter"/>.
    /// </summary>
    internal sealed class TypeDescriptionConverter : ITypeDescriptionConverter
    {
        private static readonly Type DB_NULL_TYPE = typeof(DBNull);

        private static readonly Type OBJECT_TYPE = typeof(object);
        
        /// <summary>Реализация по умолчанию.</summary>
        public static TypeDescriptionConverter Default
        {
            get
            {
                Contract.Ensures(Contract.Result<TypeDescriptionConverter>() != null);

                return _default;
            }
        }
        private static readonly TypeDescriptionConverter _default 
            = new TypeDescriptionConverter(OneSTypeConverter.Default);

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_oneSTypeConverter != null);
        }

        /// <summary>Конструктор, принимающий конвертор типов 1С.</summary>
        public TypeDescriptionConverter(IOneSTypeConverter oneSTypeConverter)
        {
            Contract.Requires<ArgumentNullException>(oneSTypeConverter != null);

            _oneSTypeConverter = oneSTypeConverter;
        }

        private readonly IOneSTypeConverter _oneSTypeConverter;

        /// <summary>
        /// Конвертация типа 1С в тип CLR.
        /// </summary>
        /// <param name="typeDescription">Описание типа 1С.</param>
        public Type ConvertFrom(ITypeDescription typeDescription)
        {
            var clrTypes = GetClrTypes(typeDescription)
                .Where(t => t != DB_NULL_TYPE)
                .Distinct()
                .ToArray();

            return (clrTypes.Length == 1)
                       ? (clrTypes.Single() ?? OBJECT_TYPE)
                       : OBJECT_TYPE;
        }

        /// <summary>
        /// Получение списка CLR-типов соответствующих описанию типа 1С.
        /// </summary>
        /// <param name="typeDescription">
        /// Описание типа.
        /// </param>
        private IEnumerable<Type> GetClrTypes(ITypeDescription typeDescription)
        {
            using (var types = typeDescription.Types)
            {
                var typesCount = types.Count();

                for (var index = 0; index < typesCount; index++)
                {
                    using (var oneSType = types.Get(index))
                    {
                        yield return _oneSTypeConverter.TryConvertFrom(oneSType);
                    }
                }
            }
        }
    }
}