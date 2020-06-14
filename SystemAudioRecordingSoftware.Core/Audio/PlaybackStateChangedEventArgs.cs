// (c) Johannes Wolfgruber, 2020
using NAudio.Wave;
using System;

namespace SystemAudioRecordingSoftware.Core.Audio
{
    public class PlaybackStateChangedEventArgs : EventArgs
    {
        public PlaybackStateChangedEventArgs(PlaybackState playbackState)
        {
            PlaybackState = playbackState;
        }

        public PlaybackState PlaybackState { get; }
    }
}
