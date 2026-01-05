// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace XamlStudio.Services;

public class OnBackgroundEnteringEventArgs : EventArgs
{
    public SuspensionState SuspensionState { get; set; }

    public Type Target { get; private set; }

    public bool IsOutsideSuspend { get; private set; }

    public OnBackgroundEnteringEventArgs(SuspensionState suspensionState, Type target, bool outsideSuspend)
    {
        SuspensionState = suspensionState;
        Target = target;
        IsOutsideSuspend = outsideSuspend;
    }
}
