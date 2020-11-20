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
            Name = trackDto.Name;
            FilePath = trackDto.FilePath;
            Length = trackDto.Length;
        }

        [Reactive] public string Name { get; set; }
        [Reactive] public string? FilePath { get; set; }
        [Reactive] public TimeSpan? Length { get; set; }
    }
}
