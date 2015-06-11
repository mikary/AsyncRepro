// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncRepro
{
    public class QuerySourceScope<TResult> : QuerySourceScope
    {
        public readonly TResult Result;

        public QuerySourceScope(
            QuerySource querySource,
            TResult result,
            QuerySourceScope parentScope)
            : base(querySource, parentScope)
        {
            Result = result;
        }

        public override object UntypedResult => Result;
    }
}
