﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.NETCore.Client.DiagnosticsIpc;
using System;
using System.Diagnostics;

namespace Microsoft.Diagnostics.NETCore.Client.ReversedServer
{
    /// <summary>
    /// Represents a runtine instance connection to a reversed diagnostics server.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct IpcEndpointInfo
    {
        public IpcEndpointInfo(IpcEndpoint endpoint, int processId, Guid runtimeInstanceCookie)
        {
            Endpoint = endpoint;
            ProcessId = processId;
            RuntimeInstanceCookie = runtimeInstanceCookie;
        }

        /// <summary>
        /// An endpoint used to retrieve diagnostic information from the associated runtime instance.
        /// </summary>
        public IpcEndpoint Endpoint { get; }

        /// <summary>
        /// The identifier of the process that is unique within its process namespace.
        /// </summary>
        public int ProcessId { get; }

        /// <summary>
        /// The unique identifier of the runtime instance.
        /// </summary>
        public Guid RuntimeInstanceCookie { get; }

        internal string DebuggerDisplay => FormattableString.Invariant($"PID={ProcessId}, Cookie={RuntimeInstanceCookie}");
    }
}
