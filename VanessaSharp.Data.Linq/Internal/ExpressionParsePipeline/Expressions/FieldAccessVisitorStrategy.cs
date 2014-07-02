using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Стратегия посщения узлов доступа к полям записи.</summary>
    internal abstract class FieldAccessVisitorStrategy
    {
        /// <summary>Посетитель узлов доступа к полям записи.</summary>
        private readonly IFieldAccessVisitor _fieldAccessVisitor;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="fieldAccessVisitor">Посетитель узлов доступа к полям записи.</param>
        /// <param name="recordExpression">Выражение записи.</param>
        protected FieldAccessVisitorStrategy(IFieldAccessVisitor fieldAccessVisitor, ParameterExpression recordExpression)
        {
            Contract.Requires<ArgumentNullException>(fieldAccessVisitor != null);
            Contract.Requires<ArgumentNullException>(recordExpression != null);

            _fieldAccessVisitor = fieldAccessVisitor;
            _recordExpression = recordExpression;
        }

        /// <summary>Посещение поля.</summary>
        /// <param name="fieldName">Имя поля.</param>
        /// <param name="node">Узел соответствующий доступу к полю записи.</param>
        protected Expression VisitFieldAccess(string fieldName, Expression node)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fieldName));
            Contract.Requires<ArgumentNullException>(node != null);

            return 
                _fieldAccessVisitor.VisitFieldAccess(new SqlFieldExpression(fieldName), node.Type) 
                ?? node;
        }

        /// <summary>Выражение записи данных.</summary>
        protected ParameterExpression RecordExpression
        {
            get { return _recordExpression; }
        }
        private readonly ParameterExpression _recordExpression;
        
        /// <summary>Посещение узла вызова метода.</summary>
        /// <param name="node">Посещаемый узел.</param>
        /// <param name="defaultAction">Действие по умолчанию.</param>
        public virtual Expression VisitMethodCall(
            MethodCallExpression node,
            Func<MethodCallExpression, Expression> defaultAction)
        {
            return defaultAction(node);
        }

        /// <summary>Посещение узла доступа к члену объекта.</summary>
        /// <param name="node">Посещаемый узел.</param>
        /// <param name="defaultAction">Действие по умолчанию.</param>
        public virtual Expression VisitMember(
            MemberExpression node,
            Func<MemberExpression, Expression> defaultAction)
        {
            return defaultAction(node);
        }

        /// <summary>Создание стратегии.</summary>
        /// <param name="fieldAccessVisitor">Посетитель узлов доступа к полям записи.</param>
        /// <param name="recordExpression">Выражение записи.</param>
        /// <param name="mappingProvider">Поставщик соответствий типам CLR источников данных 1С.</param>
        public static FieldAccessVisitorStrategy Create(
            IFieldAccessVisitor fieldAccessVisitor, ParameterExpression recordExpression, IOneSMappingProvider mappingProvider)
        {
            Contract.Requires<ArgumentNullException>(fieldAccessVisitor != null);
            Contract.Requires<ArgumentNullException>(recordExpression != null);
            Contract.Requires<ArgumentNullException>(mappingProvider != null);

            return (recordExpression.Type == typeof(OneSDataRecord))
                       ? (FieldAccessVisitorStrategy)new DataRecordStrategy(fieldAccessVisitor, recordExpression)
                       : new TypedRecordStrategy(fieldAccessVisitor, recordExpression,
                                                 mappingProvider.GetTypeMapping(recordExpression.Type));
        }
        
        /// <summary>
        /// Стратегия доступа к полям нетипизированной записи <see cref="OneSDataRecord"/>.
        /// </summary>
        private sealed class DataRecordStrategy : FieldAccessVisitorStrategy
        {
            /// <summary>Набор методов доступа к полям записи.</summary>
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
            
            public DataRecordStrategy(
                IFieldAccessVisitor fieldAccessVisitor,
                ParameterExpression recordExpression)
                : base(fieldAccessVisitor, recordExpression)
            {}

            /// <summary>Посещение узла вызова метода.</summary>
            /// <param name="node">Посещаемый узел.</param>
            /// <param name="defaultAction">Действие по умолчанию.</param>
            public override Expression VisitMethodCall(MethodCallExpression node, Func<MethodCallExpression, Expression> defaultAction)
            {
                if (node.Object == RecordExpression)
                {
                    if (_getValueMethods.Contains(node.Method))
                    {
                        var fieldName = node.Arguments[0].GetConstant<string>();
                        return VisitFieldAccess(fieldName, node);
                    }

                    throw node.CreateExpressionNotSupportedException();
                }
                
                return base.VisitMethodCall(node, defaultAction);
            }
        }

        /// <summary>Стратегия доступа к полям типизированных записей.</summary>
        private sealed class TypedRecordStrategy : FieldAccessVisitorStrategy
        {
            /// <summary>Карта соответствия типизированной записи источнику данных 1С.</summary>
            private readonly OneSTypeMapping _typeMapping;
            
            public TypedRecordStrategy(IFieldAccessVisitor fieldAccessVisitor, ParameterExpression recordExpression, OneSTypeMapping typeMapping)
                : base(fieldAccessVisitor, recordExpression)
            {
                Contract.Requires<ArgumentNullException>(typeMapping != null);
                
                _typeMapping = typeMapping;
            }

            /// <summary>Посещение узла доступа к члену объекта.</summary>
            /// <param name="node">Посещаемый узел.</param>
            /// <param name="defaultAction">Действие по умолчанию.</param>
            public override Expression VisitMember(MemberExpression node, Func<MemberExpression, Expression> defaultAction)
            {
                if (node.Expression == RecordExpression)
                {
                    var fieldName = _typeMapping.GetFieldNameByMemberInfo(node.Member);
                    if (fieldName != null)
                        return VisitFieldAccess(fieldName, node);

                    throw node.CreateExpressionNotSupportedException();
                }
                
                return base.VisitMember(node, defaultAction);
            }
        }
    }
}
