// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncRepro
{
    public class Context : IDisposable
    {
        private readonly List<IValueBufferCursor> _activeQueries
            = new List<IValueBufferCursor>();
        private bool _disposed;

        public Context(string connectionString)
        {
            Connection = new Connection(connectionString);
        }

        public Connection Connection { get; }

        public CancellationToken CancellationToken { get; set; }

        public async Task RegisterValueBufferCursorAsync(
            IValueBufferCursor valueBufferCursor,
            CancellationToken cancellationToken)
        {
            if (_activeQueries.Count > 0)
            {
                await _activeQueries.Last().BufferAllAsync(cancellationToken);
            }

            _activeQueries.Add(valueBufferCursor);
        }

        public virtual void DeregisterValueBufferCursor(IValueBufferCursor valueBufferCursor)
        {
            _activeQueries.Remove(valueBufferCursor);
        }

        public ValueBuffer CreateValueBuffer(DbDataReader dataReader)
        {
            Debug.Assert(dataReader != null); // hot path

            var fieldCount = dataReader.FieldCount;

            if (fieldCount == 0)
            {
                return ValueBuffer.Empty;
            }

            var values = new object[fieldCount];

            dataReader.GetValues(values);

            for (var i = 0; i < fieldCount; i++)
            {
                if (ReferenceEquals(values[i], DBNull.Value))
                {
                    values[i] = null;
                }
            }

            return new ValueBuffer(values);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Connection.Dispose();
                _disposed = true;
            }
        }
    }
}
