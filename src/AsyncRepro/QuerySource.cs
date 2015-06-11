// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace AsyncRepro
{
    public class QuerySource
    {
        public QuerySource(Type type)
        {
            ItemName = type.Name;
            ItemType = type;
        }

        string ItemName { get; }
        Type ItemType { get; }
    }
}
