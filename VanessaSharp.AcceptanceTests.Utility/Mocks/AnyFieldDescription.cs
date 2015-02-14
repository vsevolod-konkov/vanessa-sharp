using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.AcceptanceTests.Utility.Mocks
{
    /// <summary>
    /// Описание поля, которое не тестируется.
    /// </summary>
    public sealed class AnyFieldDescription : FieldDescription
    {
        /// <summary>Конструктор.</summary>
        /// <param name="name">Имя поля.</param>
        public AnyFieldDescription(string name)
            : base(name)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));
        }

        /// <summary>Вид поля.</summary>
        public override FieldKind Kind
        {
            get { return FieldKind.Any; }
        }

        /// <summary>Тип поля.</summary>
        public override Type Type
        {
            get { return typeof(AnyType); }
        }

        /// <summary>Имя типа данных.</summary>
        public override string DataTypeName
        {
            get { return null; }
        }
    }
}