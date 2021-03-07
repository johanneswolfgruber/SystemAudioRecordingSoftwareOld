using NAudio.CoreAudioApi;
using System;

namespace SystemAudioRecordingSoftware.Domain.Events
{
    public sealed class CaptureStateChangedEventArgs : EventArgs
    {
        public CaptureStateChangedEventArgs(CaptureState state)
        {
            State = state;
        }

        public CaptureState State { get; }
    }
}