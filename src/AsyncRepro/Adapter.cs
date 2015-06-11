// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncRepro
{
    public static class Adapter
    {
        public static IAsyncEnumerable<T> _ShapedQuery<T>(
            Context context,
            string query,
            Func<ValueBuffer, T> shaper)
            => new AsyncQueryEnumerable(context, query).Select(shaper);
        public static IAsyncEnumerable<T> _ToSequence<T>(T element)
            => new AsyncEnumerableAdapter<T>(new[] { element });

        public static IAsyncEnumerable<TResult> _Select<TSource, TResult>(
            IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
            => source.Select(selector);

        public static IAsyncEnumerable<TResult> _SelectMany<TSource, TResult>(
            IAsyncEnumerable<TSource> source, Func<TSource, IAsyncEnumerable<TResult>> selector)
            => source.SelectMany(selector);

        public static IAsyncEnumerable<TSource> _Where<TSource>(
            IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
            => source.Where(predicate);

        public static QuerySourceScope<ValueBuffer> CreateValueBuffer(
            QuerySource querySource,
            Context context,
            QuerySourceScope parentQuerySourceScope,
            ValueBuffer valueBuffer,
            int valueBufferOffset)
        {
            return new QuerySourceScope<ValueBuffer>(
                querySource,
                valueBuffer.UpdateOffset(valueBufferOffset),
                parentQuerySourceScope);
        }

        public static QuerySourceScope<T> CreateEntity<T>(
            QuerySource querySource,
            Context context,
            QuerySourceScope parentQuerySourceScope,
            ValueBuffer valueBuffer,
            int valueBufferOffset,
            Func<ValueBuffer, object> materializer)
            where T : class
        {
            valueBuffer = valueBuffer.UpdateOffset(valueBufferOffset);

            return new QuerySourceScope<T>(
                querySource,
                (T)materializer(valueBuffer),
                parentQuerySourceScope);
        }

        public static T _Result<T>(Task<T> task) => task.Result;




    }
}
