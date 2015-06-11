// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncRepro
{
    public class Connection : IDisposable
    {
        private readonly SqlConnection _connection;
        private int _openedCount;
        private bool _disposed;

        public Connection(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            _openedCount = 0;
        }

        public async Task OpenAsync(CancellationToken cancellationToken)
        {
            if (_openedCount == 0)
            {
                await _connection.OpenAsync(cancellationToken);
            }
            _openedCount++;
        }

        public void Close()
        {
            if (_openedCount > 0
                && --_openedCount == 0)
            {
                _connection.Close();
            }
        }

        public SqlCommand CreateCommand()
        {
            return _connection.CreateCommand();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _connection.Dispose();
                _disposed = true;
            }
        }
    }
}
