// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace XamlStudio.Models;

public class UnhandledException
{
    public UnhandledException(string message, Exception exception)
    {
        Message = message;
        Exception = exception;
    }

    public string Message { get; set; }

    public Exception Exception { get; set; }
}
