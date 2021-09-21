﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;
using ClrDiagnostics.Extensions;

namespace ClrDiagnostics
{
    public partial class DiagnosticAnalyzer
    {
        public Dictionary<string, int> GetDuplicateStrings(int minCount = 2)
        {
            return Objects
                .Where(o => o.Type.ElementType == ClrElementType.String)
                .Select(o => o.GetStringValue(int.MaxValue))
                .Where(s => !string.IsNullOrEmpty(s))
                .GroupBy(s => s)
                .Select(s => (str: s.Key, count: s.Count()))
                .Where(t => t.count >= minCount)
                .OrderByDescending(t => t.count)
                .ThenBy(t => t.str)
                .ToDictionary(t => t.str, t => t.count);
        }

        public IEnumerable<(ClrObject obj, string text)> GetStringsBySize(long minSize = 1024, long maxSize = long.MaxValue)
        {
            return Objects
                .Where(o => o.Type.ElementType == ClrElementType.String &&
                    o.Size >= (ulong)minSize &&
                    o.Size <= (ulong)maxSize)
                .Select(o => (@object: o, @string: o.GetStringValue(int.MaxValue)))
                .Where(t => !string.IsNullOrEmpty(t.@string))
                .OrderByDescending(t => t.@string.Length);
        }

    }
}
