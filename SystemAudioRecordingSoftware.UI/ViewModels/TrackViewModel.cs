// (c) Johannes Wolfgruber, 2020

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.UI.ViewModels
{
    public class TrackViewModel : ReactiveObject
    {
        public TrackViewModel(TrackDto trackDto)
        {
            Id = trackDto.Id;
            Name = trackDto.Name;
            FilePath = trackDto.FilePath;
            Start = trackDto.Start;
            Length = trackDto.Length;
        }

        public Guid Id { get; }


        [Reactive] public string Name { get; set; }

        [Reactive] public string? FilePath { get; set; }

        [Reactive] public TimeSpan? Start { get; set; }

        [Reactive] public TimeSpan? Length { get; set; }
    }
}