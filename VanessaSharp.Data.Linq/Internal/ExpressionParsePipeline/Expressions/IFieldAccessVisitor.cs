using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Интерфейс посетителя доступа к полю записи.
    /// </summary>
    [ContractClass(typeof(FieldAccessVisitorContract))]
    internal interface IFieldAccessVisitor
    {
        /// <summary>Посещение узла доступа к полю записи.</summary>
        /// <param name="fieldExpression">SQL-Выражение поля.</param>
        /// <param name="fieldType">Тип поля.</param>
        Expression VisitFieldAccess(SqlFieldExpression fieldExpression, Type fieldType);
    }

    [ContractClassFor(typeof(IFieldAccessVisitor))]
    internal abstract class FieldAccessVisitorContract : IFieldAccessVisitor
    {
        /// <summary>Посещение узла доступа к полю записи.</summary>
        /// <param name="fieldExpression">SQL-Выражение поля.</param>
        /// <param name="fieldType">Тип поля.</param>
        Expression IFieldAccessVisitor.VisitFieldAccess(SqlFieldExpression fieldExpression, Type fieldType)
        {
            Contract.Requires<ArgumentNullException>(fieldExpression != null);
            Contract.Requires<ArgumentNullException>(fieldType != null);

            return null;
        }
    }
}