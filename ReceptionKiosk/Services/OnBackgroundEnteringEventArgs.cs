using System;

namespace ReceptionKiosk.Services
{
    public class OnBackgroundEnteringEventArgs : EventArgs
    {
        public SuspensionState SuspensionState { get; set; }
        public Type Target { get; private set; }

        public OnBackgroundEnteringEventArgs(SuspensionState suspensionState, Type target) : base()
        {
            SuspensionState = suspensionState;
            Target = target;
        }
    }
}
