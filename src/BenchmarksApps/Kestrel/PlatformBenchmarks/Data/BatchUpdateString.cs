// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System.Linq;

namespace PlatformBenchmarks
{
    internal class BatchUpdateString
    {
        private const int MaxBatch = 500;

        public static DatabaseServer DatabaseServer;

        internal static readonly string[] Ids = Enumerable.Range(0, MaxBatch).Select(i => $"@I{i}").ToArray();
        internal static readonly string[] Randoms = Enumerable.Range(0, MaxBatch).Select(i => $"@R{i}").ToArray();

        private static string[] _queries = new string[MaxBatch + 1];

        public static string Query(int batchSize)
        {
            if (_queries[batchSize] != null)
            {
                return _queries[batchSize];
            }

            return CreateBatch(batchSize);
        }

        private static string CreateBatch(int batchSize)
        {
            var lastIndex = batchSize - 1;

            var sb = StringBuilderCache.Acquire();

            sb.Append("UPDATE world SET randomnumber = CASE id");
#if NET6_0_OR_GREATER
            Enumerable.Range(0, batchSize).Select(i => i * 2 + 1).ToList().ForEach(i => sb.Append($" WHEN ${i} THEN ${i + 1}"));
#else
            Enumerable.Range(0, batchSize).ToList().ForEach(i => sb.Append($" WHEN @I{i} THEN @R{i}"));
#endif
            sb.Append(" ELSE randomnumber END WHERE id IN (");
#if NET6_0_OR_GREATER
            Enumerable.Range(0, batchSize).Select(i => i * 2 + 1).ToList().ForEach(i => sb.Append($"${i}{(lastIndex * 2 + 1 == i ? "" : ",")}"));
#else
            Enumerable.Range(0, batchSize).ToList().ForEach(i => sb.Append($"@I{i}{(lastIndex == i ? "" : ",")}"));
#endif
            sb.Append(")");

            return _queries[batchSize] = StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}
