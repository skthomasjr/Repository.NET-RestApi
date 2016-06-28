﻿using System;
using System.Linq.Expressions;

namespace NetClient.Rest
{
    /// <summary>
    ///     The query translator.
    /// </summary>
    internal class RestQueryTranslator : ExpressionVisitor
    {
        private readonly RestQueryValues queryValues = new RestQueryValues();

        /// <summary>
        ///     Translates binary nodes.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>Expression.</returns>
        /// <exception cref="InvalidOperationException">
        ///     A duplicate resource key was used in the query expression.
        ///     or
        ///     An invalid expression type was used in the query expression.
        /// </exception>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    var name = (node.Left as MemberExpression)?.Member.Name;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        if (queryValues.ResourceValues.ContainsKey(name))
                        {
                            throw new InvalidOperationException("A duplicate resource key was used in the query expression.");
                        }

                        object value = null;
                        switch (node.Right.NodeType)
                        {
                            case ExpressionType.Constant:
                                value = (node.Right as ConstantExpression)?.Value;
                                break;
                            case ExpressionType.MemberAccess:
                                var equalExpression = Expression.Convert(node.Right, typeof(object));
                                value = Expression.Lambda<Func<object>>(equalExpression).Compile()();
                                break;
                            default:
                                throw new InvalidOperationException("The expression type used is not supported.");
                        }
                        queryValues.ResourceValues.Add(name, value);
                    }
                    break;
                case ExpressionType.AndAlso:
                    break;
                case ExpressionType.NotEqual:
                    if ((node.Right as ConstantExpression)?.Value == null)
                    {
                        var notEqualExpression = Expression.Convert(node.Left, typeof(object));
                        queryValues.Criteria.Add(Expression.Lambda<Func<object>>(notEqualExpression).Compile()());
                    }
                    break;
                default:
                    throw new InvalidOperationException("An invalid expression type was used in the query expression.");
            }

            return base.VisitBinary(node);
        }

        /// <summary>
        ///     Gets the query values.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>IDictionary&lt;System.String, System.Object&gt;.</returns>
        internal RestQueryValues GetQueryValues(Expression expression)
        {
            Visit(expression);
            return queryValues;
        }
    }
}