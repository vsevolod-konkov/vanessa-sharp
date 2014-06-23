using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>
    /// Поставщик соответствия типам CLR
    /// данным 1С.
    /// </summary>
    internal sealed class OneSMappingProvider : IOneSMappingProvider
    {
        /// <summary>
        /// Проверка типа на корректность использования его в виде 
        /// типа записи данных из 1С.
        /// </summary>
        /// <param name="dataType">Тип данных.</param>
        public void CheckDataType(Type dataType)
        {
            if (dataType.IsAbstract)
            {
                throw new InvalidDataTypeException(
                    dataType,
                    "Тип является абстрактным");
            }
            
            if (!dataType.IsDefined(typeof(OneSDataSourceAttribute), false))
            {
                throw new InvalidDataTypeException(
                    dataType,
                    string.Format("Тип не содержит атрибут \"{0}\".", typeof(OneSDataSourceAttribute)));
            }

            if (dataType.GetConstructors().Count(c => c.GetParameters().Length == 0) == 0)
            {
                throw new InvalidDataTypeException(
                    dataType,
                    "Тип не имеет публичного конструктора без аргументов.");
            }

            var columnAttr = typeof(OneSDataColumnAttribute);

            foreach (var field in dataType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (field.IsDefined(columnAttr, true) && !field.IsPublic)
                {
                    throw new InvalidDataTypeException(
                        dataType,
                        string.Format("Тип имеет непубличное поле \"{0}\" помеченное атрибутом \"{1}\".", field.Name, columnAttr)
                        );
                }
            }

            foreach (var property in dataType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (property.IsDefined(columnAttr, true))
                {
                    var getMethod = property.GetGetMethod();
                    if (getMethod == null)
                    {
                        throw new InvalidDataTypeException(
                            dataType,
                            string.Format("Тип имеет свойство \"{0}\" помеченное атрибутом \"{1}\", которое не имеет публичного метода получения значения.", property.Name,
                                          columnAttr)
                            );
                    }

                    var setMethod = property.GetSetMethod();
                    if (setMethod == null)
                    {
                        throw new InvalidDataTypeException(
                            dataType,
                            string.Format("Тип имеет свойство \"{0}\" помеченное атрибутом \"{1}\", которое не имеет публичного метода установки значения.", property.Name,
                                          columnAttr)
                            );
                    }
                }
            }
        }

        private static void AddFieldMappingIfExists(MemberInfo member, List<OneSFieldMapping> resultList)
        {
            var columnAttrType = typeof(OneSDataColumnAttribute);

            if (member.IsDefined(columnAttrType, true))
            {
                var columnName = ((OneSDataColumnAttribute)member.GetCustomAttributes(columnAttrType, true)[0]).ColumnName;

                resultList.Add(new OneSFieldMapping(member, columnName));
            }
        }

        /// <summary>Получения соответствия для типа.</summary>
        /// <param name="dataType">Тип.</param>
        public OneSTypeMapping GetTypeMapping(Type dataType)
        {
            Contract.Assert(dataType.IsDefined(typeof(OneSDataSourceAttribute), true));
            var sourceName = ((OneSDataSourceAttribute)dataType.GetCustomAttributes(typeof (OneSDataSourceAttribute), true)[0])
                    .SourceName;
            
            var fieldMappings = new List<OneSFieldMapping>();

            foreach (var field in dataType.GetFields(BindingFlags.Instance | BindingFlags.Public))
                AddFieldMappingIfExists(field, fieldMappings);

            foreach (var property in dataType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                AddFieldMappingIfExists(property, fieldMappings);

            return new OneSTypeMapping(sourceName, new ReadOnlyCollection<OneSFieldMapping>(fieldMappings));
        }
    }
}
