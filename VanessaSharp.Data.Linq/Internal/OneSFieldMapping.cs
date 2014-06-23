using System.Reflection;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>
    /// Карта соответствия члена типа полю табличного источника данных 1С.
    /// </summary>
    internal sealed class OneSFieldMapping
    {
        /// <summary>Конструктор.</summary>
        /// <param name="memberInfo">Член типа, которому соответствует поле табличного источника данных в 1С.</param>
        /// <param name="fieldName">Имя поля табличного источника данных в 1С.</param>
        public OneSFieldMapping(MemberInfo memberInfo, string fieldName)
        {
            _memberInfo = memberInfo;
            _fieldName = fieldName;
        }

        /// <summary>
        /// Член типа, которому соответствует поле табличного источника данных в 1С.
        /// </summary>
        public MemberInfo MemberInfo
        {
            get { return _memberInfo; }
        }
        private readonly MemberInfo _memberInfo;

        /// <summary>
        /// Имя поля табличного источника данных в 1С.
        /// </summary>
        public string FieldName
        {
            get { return _fieldName; }
        }
        private readonly string _fieldName;
    }
}