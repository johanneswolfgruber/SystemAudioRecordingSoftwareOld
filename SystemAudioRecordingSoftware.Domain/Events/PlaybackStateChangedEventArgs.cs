using NAudio.Wave;
using System;

namespace SystemAudioRecordingSoftware.Domain.Events
{
    public class PlaybackStateChangedEventArgs : EventArgs
    {
        public PlaybackStateChangedEventArgs(PlaybackState state)
        {
            State = state;
        }

        public PlaybackState State { get; }
    }
}