using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace VanessaSharp.AcceptanceTests.Utility.Mocks
{
    /// <summary>
    /// Описание поля табличной части.
    /// </summary>
    public sealed class TablePartFieldDescription : FieldDescription
    {
        /// <summary>
        /// Конструктор для поля табличной части.
        /// </summary>
        /// <param name="name">Имя поля.</param>
        /// <param name="tablePartFields">Поля табличной части.</param>
        public TablePartFieldDescription(string name, ReadOnlyCollection<FieldDescription> tablePartFields)
            : base(name)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));
            Contract.Requires<ArgumentNullException>(tablePartFields != null);
            Contract.Requires<ArgumentException>(tablePartFields.Count > 0);

            _tablePartFields = tablePartFields;
        }

        /// <summary>Вид поля.</summary>
        public override FieldKind Kind
        {
            get { return FieldKind.TablePart; }
        }

        /// <summary>
        /// Описание полей табличной части.
        /// </summary>
        public ReadOnlyCollection<FieldDescription> TablePartFields
        {
            get { return _tablePartFields; }
        }
        private readonly ReadOnlyCollection<FieldDescription> _tablePartFields;
    }
}