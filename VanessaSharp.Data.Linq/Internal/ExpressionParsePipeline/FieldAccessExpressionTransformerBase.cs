﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Базовый класс преобразования выражения с доступом к полям источника.</summary>
    internal abstract class FieldAccessExpressionTransformerBase : ExpressionTransformerBase
    {
        private static readonly ISet<MethodInfo>
            _getValueMethods = new HashSet<MethodInfo>
            {
                OneSQueryExpressionHelper.DataRecordGetStringMethod,
                OneSQueryExpressionHelper.DataRecordGetInt32Method,
                OneSQueryExpressionHelper.DataRecordGetDoubleMethod,
                OneSQueryExpressionHelper.DataRecordGetDateTimeMethod,
                OneSQueryExpressionHelper.DataRecordGetBooleanMethod,
                OneSQueryExpressionHelper.DataRecordGetCharMethod,
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

        /// <summary>Конструктор.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="recordExpression">Выражение записи данных.</param>
        protected FieldAccessExpressionTransformerBase(QueryParseContext context, ParameterExpression recordExpression)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(recordExpression != null);
            Contract.Requires<ArgumentException>(recordExpression.Type == typeof(OneSDataRecord));

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
            if (node.Object == RecordExpression)
            {
                if (_getValueMethods.Contains(node.Method))
                {
                    var fieldName = GetConstant<string>(node.Arguments[0]);
                    VisitFieldAccess(new SqlFieldExpression(fieldName));

                    return node;
                }

                throw CreateExpressionNotSupportedException(node);
            }

            return base.VisitMethodCall(node);
        }

        /// <summary>Посещение доступа к полю.</summary>
        /// <param name="fieldExpression">Выражение доступа к полю источника.</param>
        protected abstract void VisitFieldAccess(SqlFieldExpression fieldExpression);
    }
}