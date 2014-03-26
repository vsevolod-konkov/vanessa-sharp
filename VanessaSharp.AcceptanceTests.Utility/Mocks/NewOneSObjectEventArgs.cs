using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.AcceptanceTests.Utility.Mocks
{
    /// <summary>Аргументы запроса на создание экземпляра объекта 1С.</summary>
    public sealed class NewOneSObjectEventArgs : EventArgs
    {
        /// <summary>Конструктор.</summary>
        /// <param name="requiredType">Тип экземпляра, чье создание было запрошено.</param>
        public NewOneSObjectEventArgs(Type requiredType)
        {
            Contract.Requires<ArgumentNullException>(requiredType != null);

            _requiredType = requiredType;
        }

        /// <summary>
        /// Тип экземпляра, чье создание было запрошено.
        /// </summary>
        public Type RequiredType
        {
            get { return _requiredType; }
        }
        private readonly Type _requiredType;
        
        /// <summary>Созданный экземпяр требуемого типа.</summary>
        public object CreatedInstance
        {
            get { return _createdInstance; }
            set
            {
                if (value != null)
                {
                    if (!_requiredType.IsInstanceOfType(value))
                    {
                        throw new ArgumentException(
                            string.Format("Создаваемый экземпляр должен поддерживать тип \"{0}\".", _requiredType));
                    }
                }

                _createdInstance = value;
            }
        }
        private object _createdInstance;
    }
}
