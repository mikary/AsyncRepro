// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncRepro
{
    public class AsyncEnumerableAdapter<T> : IAsyncEnumerable<T>
    {
        private readonly IEnumerable<T> _source;

        public AsyncEnumerableAdapter(IEnumerable<T> source)
        {
            _source = source;
        }

        public IAsyncEnumerator<T> GetEnumerator()
        {
            return new AsyncEnumeratorAdapter(_source.GetEnumerator());
        }

        private class AsyncEnumeratorAdapter : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _enumerator;

            public AsyncEnumeratorAdapter(IEnumerator<T> enumerator)
            {
                _enumerator = enumerator;
            }

            public Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                return Task.FromResult(_enumerator.MoveNext());
            }

            public T Current => _enumerator.Current;

            public void Dispose()
            {
                _enumerator.Dispose();
            }
        }
    }
}
