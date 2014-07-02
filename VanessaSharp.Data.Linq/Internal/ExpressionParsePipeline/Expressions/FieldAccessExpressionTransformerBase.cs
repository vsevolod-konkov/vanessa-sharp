using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Базовый класс преобразования выражения с доступом к полям источника.</summary>
    internal abstract class FieldAccessExpressionTransformerBase : ExpressionTransformerBase
    {
        private static readonly ISet<MethodInfo>
            _getValueMethods = new HashSet<MethodInfo>
            {
                OneSQueryExpressionHelper.DataRecordGetCharMethod,
                OneSQueryExpressionHelper.DataRecordGetStringMethod,
                OneSQueryExpressionHelper.DataRecordGetByteMethod,
                OneSQueryExpressionHelper.DataRecordGetInt16Method,
                OneSQueryExpressionHelper.DataRecordGetInt32Method,
                OneSQueryExpressionHelper.DataRecordGetInt64Method,
                OneSQueryExpressionHelper.DataRecordGetFloatMethod,
                OneSQueryExpressionHelper.DataRecordGetDoubleMethod,
                OneSQueryExpressionHelper.DataRecordGetDecimalMethod,
                OneSQueryExpressionHelper.DataRecordGetDateTimeMethod,
                OneSQueryExpressionHelper.DataRecordGetBooleanMethod,
            };

        /// <summary>Контекст разбора запроса.</summary>
        protected QueryParseContext Context
        {
            get { return _context; }
        }
        private readonly QueryParseContext _context;

        /// <summary>Выражение записи данных.</summary>
        protected ParameterExpression RecordExpression
        {
            get { return _recordExpression; }
        }
        private readonly ParameterExpression _recordExpression;

        /// <summary>
        /// Поставщик соответствий типам источников данных 1С.
        /// </summary>
        private readonly IOneSMappingProvider _mappingProvider;

        /// <summary>Конструктор.</summary>
        /// <param name="mappingProvider">Поставщик соответствий типам источников данных 1С.</param>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="recordExpression">Выражение записи данных.</param>
        protected FieldAccessExpressionTransformerBase(IOneSMappingProvider mappingProvider, QueryParseContext context, ParameterExpression recordExpression)
        {
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(recordExpression != null);

            _mappingProvider = mappingProvider;
            _context = context;
            _recordExpression = recordExpression;
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
            // TODO Рефакторинг
            if (RecordExpression.Type == typeof(OneSDataRecord) && node.Object == RecordExpression)
            {
                if (_getValueMethods.Contains(node.Method))
                {
                    var fieldName = GetConstant<string>(node.Arguments[0]);
                    VisitFieldAccess(new SqlFieldExpression(fieldName));

                    return node;
                }
            }

            return base.VisitMethodCall(node);
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
            // TODO Рефакторинг
            if (RecordExpression.Type != typeof(OneSDataRecord) && node.Expression == RecordExpression)
            {
                var typeMapping = _mappingProvider.GetTypeMapping(RecordExpression.Type);
                var fieldName = typeMapping.GetFieldNameByMemberInfo(node.Member);
                if (fieldName != null)
                {
                    VisitFieldAccess(new SqlFieldExpression(fieldName));
                    return node;
                }
            }
            
            return base.VisitMember(node);
        }

        /// <summary>Посещение доступа к полю.</summary>
        /// <param name="fieldExpression">Выражение доступа к полю источника.</param>
        protected abstract void VisitFieldAccess(SqlFieldExpression fieldExpression);
    }
}
