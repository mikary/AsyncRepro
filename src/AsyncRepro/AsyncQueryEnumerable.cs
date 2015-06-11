// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncRepro
{
    public class AsyncQueryEnumerable : IAsyncEnumerable<ValueBuffer>
    {
        private readonly Context _context;
        private readonly string _query;

        public AsyncQueryEnumerable(Context context, string query)
        {
            _context = context;
            _query = query;
        }

        public IAsyncEnumerator<ValueBuffer> GetEnumerator()
        {
            return new QueryRunnerEnumerator(_context, _query);
        }

        private class QueryRunnerEnumerator : IAsyncEnumerator<ValueBuffer>, IValueBufferCursor
        {
            private readonly Context _context;
            private readonly string _query;

            private SqlDataReader _reader;
            private Queue<ValueBuffer> _buffer;
            private bool _disposed;


            public QueryRunnerEnumerator(Context context, string query)
            {
                _context = context;
                _query = query;
            }

            public ValueBuffer Current { get; private set; }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                if (_buffer == null)
                {
                    if (_reader == null)
                    {
                        await _context.Connection.OpenAsync(cancellationToken);

                        var command = _context.Connection.CreateCommand();
                        command.CommandText = _query;

                        await _context.RegisterValueBufferCursorAsync(this, cancellationToken);

                        _reader = await command.ExecuteReaderAsync(cancellationToken);
                    }

                    var hasNext = await _reader.ReadAsync(cancellationToken);

                    Current = hasNext
                        ? _context.CreateValueBuffer(_reader)
                        : default(ValueBuffer);

                    return hasNext;
                }

                if (_buffer.Count > 0)
                {
                    Current = _buffer.Dequeue();

                    return true;
                }

                return false;

            }
            public async Task BufferAllAsync(CancellationToken cancellationToken)
            {
                if (_buffer == null)
                {
                    _buffer = new Queue<ValueBuffer>();

                    using (_reader)
                    {
                        while (await _reader.ReadAsync(cancellationToken))
                        {
                            _buffer.Enqueue(_context.CreateValueBuffer(_reader));
                        }
                    }

                    _reader = null;
                }
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _context.DeregisterValueBufferCursor(this);
                    _context.Connection.Close();
                    _reader?.Dispose();
                    _disposed = true;
                }
            }
        }
    }
}
