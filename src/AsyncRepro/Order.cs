// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace AsyncRepro
{public class Order
{
    public const string Query = @"
SELECT
    CustomerID
FROM
    Orders";

        public string CustomerId { get; set; }
    }
}
