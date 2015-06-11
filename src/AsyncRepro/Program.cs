// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncRepro
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Main");
            var reset = new ManualResetEventSlim(false);

            var instance = new Program();

            Console.WriteLine("Calling Start");
            var spinTask = Task.Factory.StartNew(async () => { await instance.Spin(reset); });

            Console.WriteLine("Waiting on task");
            spinTask.Wait();

            Console.WriteLine("Start Waiting on reset event");
            reset.Wait();

            Console.Write("Done");
            Console.ReadLine();
        }

        public async Task Spin(ManualResetEventSlim resetEvent)
        {
            Console.WriteLine("Await Done");

            try
            {
                var connectionString = new SqlConnectionStringBuilder
                {
                    DataSource = @"(localdb)\MSSQLLocalDB",
                    MultipleActiveResultSets = false,
                    InitialCatalog = "Northwind",
                    IntegratedSecurity = true,
                    ConnectTimeout = 30
                }.ConnectionString;

                for (var index = 0; index < int.MaxValue; index++)
                {
                    using (var context = new Context(connectionString))
                    {
                        Console.WriteLine("Query {0}", index);

                        var customerQuerySource = new QuerySource(typeof(Customer));
                        var orderQuerySource = new QuerySource(typeof(Order));

                        var parentScope = new QuerySourceScope<Customer>(
                            customerQuerySource,
                            new Customer(),
                            null);

                        Func<ValueBuffer, QuerySourceScope<ValueBuffer>> lambda9 = valueBuffer
                            => Adapter.CreateValueBuffer(
                                orderQuerySource,
                                context,
                                parentScope,
                                valueBuffer,
                                0);

                        Func<ValueBuffer, object> lambda8 = valueBuffer
                            => new Customer
                            {
                                CustomerId = (string)valueBuffer[0],
                                CompanyName = (string)valueBuffer[1],
                                ContactName = (string)valueBuffer[2],
                                ContactTitle = (string)valueBuffer[3],
                                Address = (string)valueBuffer[4],
                                City = (string)valueBuffer[5],
                                Region = (string)valueBuffer[6],
                                PostalCode = (string)valueBuffer[7],
                                Country = (string)valueBuffer[8],
                                Phone = (string)valueBuffer[9],
                                Fax = (string)valueBuffer[10]
                            };

                        Func<QuerySourceScope, string> lambda7 = querySourceScope
                            => (string)querySourceScope._GetResult<ValueBuffer>(orderQuerySource)[0];

                        Func<QuerySourceScope, IAsyncEnumerable<QuerySourceScope<ValueBuffer>>> lambda6 = querySourceScope
                            => Adapter._ShapedQuery(
                                context,
                                Order.Query,
                                lambda9);

                        Func<ValueBuffer, QuerySourceScope<Customer>> lambda5 = valueBuffer
                            => Adapter.CreateEntity<Customer>(
                                customerQuerySource,
                                context,
                                parentScope,
                                valueBuffer,
                                0,
                                lambda8);

                        Func<QuerySourceScope, Customer> lambda4 = querySourceScope
                            => querySourceScope._GetResult<Customer>(customerQuerySource);

                        Func<QuerySourceScope, Boolean> lambda3 = querySourceScope
                            => Adapter._Result<Boolean>(
                                Adapter._Select(
                                    Adapter._SelectMany(
                                        Adapter._ToSequence(querySourceScope),
                                        lambda6),
                                    lambda7)
                                    .Contains(
                                        querySourceScope._GetResult<Customer>(customerQuerySource).CustomerId,
                                        context.CancellationToken));

                        Func<QuerySourceScope, IAsyncEnumerable<QuerySourceScope<Customer>>> lambda2 = querySourceScope
                            => Adapter._ShapedQuery(
                                context,
                                Customer.Query,
                                lambda5);

                        var result = await 
                            Adapter._Select(
                                Adapter._Where(
                                    Adapter._SelectMany(
                                        Adapter._ToSequence(parentScope),
                                        lambda2),
                                    lambda3),
                                lambda4).ToArray();

                        Console.WriteLine("Query Done {0}", result.Count());
                    }
                }

                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Debugger.Launch();
            }

            resetEvent.Set();
        }
    }
}
