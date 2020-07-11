// (c) Johannes Wolfgruber, 2020
using System;

namespace SystemAudioRecordingSoftware.Core.Model
{
    public sealed class Recording
    {
        public Recording(string name, string filePath, TimeSpan length)
        {
            Name = name;
            FilePath = filePath;
            Length = length;
        }

        public string Name { get; }

        public string FilePath { get; }

        public TimeSpan Length { get; }
    }
}
