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
        /// <param name="level">Уровень данных.</param>
        /// <param name="dataType">Тип данных.</param>
        public void CheckDataType(OneSDataLevel level, Type dataType)
        {
            if (dataType.IsAbstract)
            {
                throw new InvalidDataTypeException(
                    dataType,
                    "Тип является абстрактным");
            }
            
            if (level == OneSDataLevel.Root && !dataType.IsDefined(typeof(OneSDataSourceAttribute), false))
            {
                throw new InvalidDataTypeException(
                    dataType,
                    string.Format("Тип не содержит атрибут \"{0}\".", typeof(OneSDataSourceAttribute)));
            }

            if (!dataType.IsValueType && dataType.GetConstructors().All(c => c.GetParameters().Length > 0))
            {
                throw new InvalidDataTypeException(
                    dataType,
                    "Тип не имеет публичного конструктора без аргументов.");
            }

            var columnAttr = typeof(OneSDataColumnAttribute);

            foreach (var field in dataType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (field.IsDefined(columnAttr, true))
                {
                    if (!field.IsPublic)
                    {
                        throw new InvalidDataTypeException(
                            dataType,
                            string.Format("Тип имеет непубличное поле \"{0}\" помеченное атрибутом \"{1}\".", field.Name,
                                          columnAttr)
                            );
                    }

                    if ((field.Attributes & FieldAttributes.InitOnly) == FieldAttributes.InitOnly)
                    {
                        throw new InvalidDataTypeException(
                            dataType,
                            string.Format("Тип имеет поле только для чтения \"{0}\" помеченное атрибутом \"{1}\".", field.Name,
                                          columnAttr)
                            );
                    }
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

            if (level == OneSDataLevel.TablePart)
            {
                foreach (var memberInfo in dataType.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                    .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property))
                {
                    CheckTablePartMember(memberInfo);
                }
            }
        }

        private static void CheckTablePartMember(MemberInfo memberInfo)
        {
            var columnAttr = typeof(OneSDataColumnAttribute);
            if (!memberInfo.IsDefined(columnAttr, true))
                return;

            var columnAttrInstance = (OneSDataColumnAttribute)memberInfo.GetCustomAttributes(columnAttr, true)[0];
            if (columnAttrInstance.Kind == OneSDataColumnKind.TablePart)
            {
                throw new InvalidDataTypeException(
                    memberInfo.DeclaringType,
                    string.Format("Тип уровня табличной части имеет {0} \"{1}\" помеченное атрибутом \"{2}\", которое также является табличной частью.", 
                              memberInfo.MemberType == MemberTypes.Property ? "свойство" : "поле",
                              memberInfo.Name,
                              columnAttr)
                );
            }
        }

        /// <summary>
        /// Является ли тип, типом данных который имеет соответствие объекту 1С.
        /// </summary>
        /// <param name="level">Уровень данных.</param>
        /// <param name="type">Тип данных.</param>
        /// <returns>Возвращает <c>true</c> если тип соответствует типу данных заданного уровня.</returns>
        public bool IsDataType(OneSDataLevel level, Type type)
        {
            try
            {
                CheckDataType(level, type);
            }
            catch (InvalidDataTypeException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Добавление соответствия поля в результирующий список <paramref name="resultList"/>
        /// в случае если член типа маркирован атрибутом <see cref="OneSDataColumnAttribute"/>.
        /// </summary>
        /// <param name="member">Член типа.</param>
        /// <param name="resultList">Результирующий список соответствий.</param>
        private static void AddFieldMappingIfExists(MemberInfo member, List<OneSFieldMapping> resultList)
        {
            var columnAttrType = typeof(OneSDataColumnAttribute);

            if (member.IsDefined(columnAttrType, true))
            {
                var attr = (OneSDataColumnAttribute)member.GetCustomAttributes(columnAttrType, true)[0];

                resultList.Add(new OneSFieldMapping(member, attr.ColumnName, attr.Kind));
            }
        }

        private ReadOnlyCollection<OneSFieldMapping> GetTypeMappings(Type dataType)
        {
            var fieldMappings = new List<OneSFieldMapping>();

            foreach (var field in dataType.GetFields(BindingFlags.Instance | BindingFlags.Public))
                AddFieldMappingIfExists(field, fieldMappings);

            foreach (var property in dataType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                AddFieldMappingIfExists(property, fieldMappings);

            return new ReadOnlyCollection<OneSFieldMapping>(
                fieldMappings.ToArray());
        }

        /// <summary>Получение соответствия для типа верхнего уровня.</summary>
        /// <param name="dataType">Тип.</param>
        public OneSTypeMapping GetRootTypeMapping(Type dataType)
        {
            Contract.Assert(dataType.IsDefined(typeof(OneSDataSourceAttribute), true));
            
            var sourceName = ((OneSDataSourceAttribute)dataType.GetCustomAttributes(typeof(OneSDataSourceAttribute), true)[0])
                    .SourceName;
            
            return new OneSTypeMapping(sourceName, GetTypeMappings(dataType));
        }

        /// <summary>Получение соответствия для типа уровня табличной части.</summary>
        /// <param name="dataType">Тип.</param>
        public ReadOnlyCollection<OneSFieldMapping> GetTablePartTypeMappings(Type dataType)
        {
            return GetTypeMappings(dataType);
        }
    }
}
