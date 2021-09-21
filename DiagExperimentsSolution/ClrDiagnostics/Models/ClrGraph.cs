﻿using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace ClrDiagnostics.Models
{
    public class ClrGraph
    {
        internal HashSet<UInt64> Visited = new HashSet<ulong>();

        public ClrGraphNode StartObject { get; private set; }

        public static ClrGraphNode CreateForSelf(ClrObject startObject)
        {
            var instance = new ClrGraph();
            var firstNode = new ClrGraphNode(instance, startObject);
            instance.StartObject = firstNode;
            return firstNode;
        }

        public static IEnumerable<ClrGraphNode> CreateForChildren(ClrObject startObject)
        {
            var instance = new ClrGraph();
            var firstNode = new ClrGraphNode(instance, startObject);
            instance.StartObject = firstNode;
            return firstNode.Children;
        }

    }
}
