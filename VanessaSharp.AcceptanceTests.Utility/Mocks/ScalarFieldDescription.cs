using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.AcceptanceTests.Utility.Mocks
{
    /// <summary>
    /// Описание поля, принимающего скалярные значения.
    /// </summary>
    public sealed class ScalarFieldDescription : FieldDescription
    {
        /// <summary>Конструктор для скалярного поля.</summary>
        /// <param name="name">Имя поля.</param>
        /// <param name="type">Тип поля.</param>
        public ScalarFieldDescription(string name, Type type)
            : base(name)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));
            Contract.Requires<ArgumentNullException>(type != null);

            _type = type;
        }

        /// <summary>Вид поля.</summary>
        public override FieldKind Kind
        {
            get { return FieldKind.Scalar; }
        }

        /// <summary>Тип поля.</summary>
        public Type Type
        {
            get { return _type; }
        }
        private readonly Type _type;
    }
}