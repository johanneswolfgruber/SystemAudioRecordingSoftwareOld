// (c) Johannes Wolfgruber, 2020
using NAudio.CoreAudioApi;
using System;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public class CaptureStateChangedEventArgs : EventArgs
    {
        public CaptureStateChangedEventArgs(CaptureState captureState)
        {
            CaptureState = captureState;
        }

        public CaptureState CaptureState { get; }
    }
}
