using System;

namespace VanessaSharp.Data.Linq.Internal
{
    // TODO Прототип
    internal sealed class ExpressionParseProduct<T> : ExpressionParseProduct
    {
        public ExpressionParseProduct(SqlCommand command) : base(command)
        {
        }

        public Func<ISqlResultReader, T> ItemReader { get { throw new NotImplementedException(); } }
        
        public override object Execute(ISqlCommandExecuter executer)
        {
            throw new NotImplementedException();
        }
    }
}
