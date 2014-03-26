using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Продукт парсинга LINQ-выражения запроса.</summary>
    /// <remarks>Немутабельная структура.</remarks>
    [ContractClass(typeof(ExpressionParseProductContract))]
    internal abstract class ExpressionParseProduct
    {
        /// <summary>Конструктор.</summary>
        /// <param name="command">Команда SQL-запроса.</param>
        protected ExpressionParseProduct(SqlCommand command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            _command = command;
        }

        /// <summary>Команда SQL-запроса.</summary>
        public SqlCommand Command
        {
            get { return _command; }
        }
        private readonly SqlCommand _command;

        /// <summary>Выполнение запроса.</summary>
        /// <param name="executer">Выполнитель запроса.</param>
        public abstract object Execute(ISqlCommandExecuter executer);
    }

    [ContractClassFor(typeof(ExpressionParseProduct))]
    internal abstract class ExpressionParseProductContract : ExpressionParseProduct
    {
        protected ExpressionParseProductContract(SqlCommand command) : base(command)
        {}

        public override object Execute(ISqlCommandExecuter executer)
        {
            Contract.Requires<ArgumentNullException>(executer != null);

            return null;
        }
    }
}
