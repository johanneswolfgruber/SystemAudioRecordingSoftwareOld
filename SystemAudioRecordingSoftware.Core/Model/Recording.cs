// (c) Johannes Wolfgruber, 2020
using System;

namespace SystemAudioRecordingSoftware.Core.Model
{
    public sealed class Recording
    {
        public Recording(Guid id, string name, string filePath, TimeSpan length)
        {
            Id = id;
            Name = name;
            FilePath = filePath;
            Length = length;
        }

        public Guid Id { get; }
        public string Name { get; }

        public string FilePath { get; }

        public TimeSpan Length { get; }
    }
}
