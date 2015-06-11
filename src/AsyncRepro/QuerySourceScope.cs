// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AsyncRepro
{
    public abstract class QuerySourceScope
    {
        private static readonly MethodInfo _createMethodInfo
            = typeof(QuerySourceScope).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Create));

        private static readonly MethodInfo _getResultMethodInfo
            = typeof(QuerySourceScope).GetTypeInfo()
                .GetDeclaredMethod(nameof(_GetResult));

        public static Expression Create(
            QuerySource querySource,
            Expression result,
            Expression parentScope)
            => Expression.Call(
                _createMethodInfo.MakeGenericMethod(result.Type),
                Expression.Constant(querySource),
                result,
                parentScope);

        public static Expression GetResult(
            Expression querySourceScope,
            QuerySource querySource,
            Type resultType)
            => Expression.Call(
                querySourceScope,
                _getResultMethodInfo.MakeGenericMethod(resultType),
                Expression.Constant(querySource));

        private readonly QuerySourceScope _parentScope;
        private readonly QuerySource _querySource;

        protected QuerySourceScope(QuerySource querySource, QuerySourceScope parentScope)
        {
            _querySource = querySource;
            _parentScope = parentScope;
        }

        private static QuerySourceScope<TResult> _Create<TResult>(
            QuerySource querySource, TResult result, QuerySourceScope parentScope)
            => new QuerySourceScope<TResult>(querySource, result, parentScope);


        public TResult _GetResult<TResult>(QuerySource querySource)
            => _querySource == querySource
                ? ((QuerySourceScope<TResult>)this).Result
                : _parentScope._GetResult<TResult>(querySource);

        public virtual object GetResult(QuerySource querySource)
            => _querySource == querySource
                ? UntypedResult
                : _parentScope.GetResult(querySource);

        public abstract object UntypedResult { get; }
    }
}
