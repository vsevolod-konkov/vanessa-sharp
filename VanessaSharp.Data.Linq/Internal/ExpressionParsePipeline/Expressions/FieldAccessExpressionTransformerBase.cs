using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Базовый класс преобразования выражения с доступом к полям источника.</summary>
    internal abstract class FieldAccessExpressionTransformerBase : ExpressionTransformerBase
    {
        /// <summary>Контекст разбора запроса.</summary>
        protected QueryParseContext Context
        {
            get { return _context; }
        }
        private readonly QueryParseContext _context;

        /// <summary>Стратегия посещения доступа к полю.</summary>
        private readonly FieldAccessVisitorStrategy _fieldAccessVisitorStrategy;

        /// <summary>Конструктор.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="fieldAccessVisitorStrategy">Стратегия посещения доступа к полю.</param>
        protected FieldAccessExpressionTransformerBase(QueryParseContext context, FieldAccessVisitorStrategy fieldAccessVisitorStrategy)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(fieldAccessVisitorStrategy != null);

            _context = context;
            _fieldAccessVisitorStrategy = fieldAccessVisitorStrategy;
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MethodCallExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected sealed override Expression VisitMethodCall(MethodCallExpression node)
        {
            return _fieldAccessVisitorStrategy
                .VisitMethodCall(node, n => base.VisitMethodCall(n));
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MemberExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitMember(MemberExpression node)
        {
            return _fieldAccessVisitorStrategy
                .VisitMember(node, n => base.VisitMember(n));
        }

        /// <summary>
        /// Создание стратегии посещения доступа к полю.
        /// </summary>
        /// <param name="fieldAccessVisitor">Стратегия посещения доступа к полям.</param>
        /// <param name="recordExpression">Выражение записи.</param>
        /// <param name="mappingProvider">Поставщик соответствий типам CLR источников данных 1С.</param>
        protected static FieldAccessVisitorStrategy CreateFieldAccessVisitorStrategy(
            IFieldAccessVisitorForOnlySql fieldAccessVisitor, ParameterExpression recordExpression, IOneSMappingProvider mappingProvider)
        {
            Contract.Requires<ArgumentNullException>(fieldAccessVisitor != null);
            Contract.Requires<ArgumentNullException>(recordExpression != null);
            Contract.Requires<ArgumentNullException>(mappingProvider != null);

            return FieldAccessVisitorStrategy.Create(
                new FieldAccessVisitorAdapter(fieldAccessVisitor),
                recordExpression,
                mappingProvider);
        }

        /// <summary>Интерфейс посещения узлов доступа к полям только для построения SQL.</summary>
        protected interface IFieldAccessVisitorForOnlySql
        {
            /// <summary>Посещение узла доступа к полю.</summary>
            /// <param name="fieldExpression">SQL-Выражение поля.</param>
            void VisitFieldAccess(SqlFieldExpression fieldExpression);
        }

        /// <summary>
        /// Адаптер интерфейсов.
        /// </summary>
        private sealed class FieldAccessVisitorAdapter : IFieldAccessVisitor
        {
            /// <summary>
            /// Объект визитора с адаптируемым интерфейсом.
            /// </summary>
            private readonly IFieldAccessVisitorForOnlySql _adapteeVisitor;

            public FieldAccessVisitorAdapter(IFieldAccessVisitorForOnlySql adapteeVisitor)
            {
                Contract.Requires<ArgumentNullException>(adapteeVisitor != null);
                
                _adapteeVisitor = adapteeVisitor;
            }

            /// <summary>Посещение узла доступа к полю записи.</summary>
            /// <param name="fieldExpression">SQL-Выражение поля.</param>
            /// <param name="fieldType">Тип поля.</param>
            public Expression VisitFieldAccess(SqlFieldExpression fieldExpression, Type fieldType)
            {
                _adapteeVisitor.VisitFieldAccess(fieldExpression);

                return null;
            }
        }
    }
}
