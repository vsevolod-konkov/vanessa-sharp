using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace VanessaSharp.AcceptanceTests.Utility.Mocks
{
    // TODO: Refactoring
    /// <summary>Описание поля табличных данных.</summary>
    public sealed class FieldDescription
    {
        /// <summary>Конструктор для скалярного поля.</summary>
        /// <param name="name">Имя поля.</param>
        /// <param name="type">Тип поля.</param>
        public FieldDescription(string name, Type type)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));
            Contract.Requires<ArgumentNullException>(type != null);

            _isTablePart = false;
            _name = name;
            _type = type;
        }

        /// <summary>
        /// Конструктор для поля табличной части.
        /// </summary>
        /// <param name="name">Имя поля.</param>
        /// <param name="tablePartFields">Поля табличной части.</param>
        public FieldDescription(string name, ReadOnlyCollection<FieldDescription> tablePartFields)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));
            Contract.Requires<ArgumentNullException>(tablePartFields != null);
            Contract.Requires<ArgumentException>(tablePartFields.Count > 0);

            _isTablePart = true;
            _name = name;
            _tablePartFields = tablePartFields;
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
                if (_isTablePart)
                {
                    throw new InvalidOperationException("Вызов допустим только для скалярных полей.");
                }
                
                return _type;
            }
        }
        private readonly Type _type;

        /// <summary>Является табличной частью.</summary>
        public bool IsTablePart
        {
            get { return _isTablePart; }
        }
        private readonly bool _isTablePart;

        /// <summary>
        /// Описание полей табилчной части.
        /// </summary>
        public ReadOnlyCollection<FieldDescription> TablePartFields
        {
            get
            {
                if (!_isTablePart)
                {
                    throw new InvalidOperationException(
                        "Вызов допустим только для полей являющиеся табличной частью.");
                }

                return _tablePartFields;
            }
        }
        private readonly ReadOnlyCollection<FieldDescription> _tablePartFields;
    }
}