using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace VanessaSharp.Proxy.Common
{
    // TODO: В зачаточном состоянии - должен стать публичным и конфигурироваться через конфиг файл.
    /// <summary>Поставщик соответствий типов.</summary>
    internal static class OneSObjectMappingProvider
    {
        /// <summary>Получение всех карт соответствий.</summary>
        public static IEnumerable<OneSObjectMapping> GetObjectMappings()
        {
            Contract.Ensures(Contract.Result<IEnumerable<OneSObjectMapping>>() != null);

            return GetObjectMappings(typeof(IQuery).Assembly);
        }

        /// <summary>Получение карт соответствий для указанной сборки.</summary>
        /// <param name="assembly">Сборка, для которой надо получить карты соответствий.</param>
        public static IEnumerable<OneSObjectMapping> GetObjectMappings(Assembly assembly)
        {
            Contract.Requires<ArgumentNullException>(assembly != null);
            Contract.Ensures(Contract.Result<IEnumerable<OneSObjectMapping>>() != null);

            return InternalGetObjectMappings(assembly);
        }

        /// <summary>Получение карт соответствий для указанной сборки.</summary>
        /// <param name="assembly">Сборка, для которой надо получить карты соответствий.</param>
        private static IEnumerable<OneSObjectMapping> InternalGetObjectMappings(Assembly assembly)
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                OneSObjectMapping mapping = null;
                try
                {
                    mapping = GetObjectMapping(type);
                }
                catch (Exception e)
                {
                    // TODO: Перейти на log4net
                    Trace.Write(e);
                }

                if (mapping != null)
                    yield return mapping;
            }
        }

        /// <summary>Получение карты соответствия для указанного типа.</summary>
        public static OneSObjectMapping GetObjectMapping(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            Type attrType = typeof(OneSObjectMappingAttribute);

            if (!type.IsDefined(attrType, false))
                return null;

            var attribute = (OneSObjectMappingAttribute)type.GetCustomAttributes(attrType, false)[0];
            if (attribute.WrapType == null)
            {
                throw new ArgumentException(
                    string.Format(
                    "Неверно задан атрибут \"{0}\" у типа \"{1}\". Свойство WrapType не может быть равным null.",
                    attrType, type));
            }

            return new OneSObjectMapping(type, attribute.WrapType, attribute.OneSTypeName);
        }
    }
}
