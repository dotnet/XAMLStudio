using System;

namespace XamlStudio.Services
{
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
}
