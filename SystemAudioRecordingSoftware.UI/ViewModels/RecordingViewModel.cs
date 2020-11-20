// (c) Johannes Wolfgruber, 2020

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.UI.ViewModels
{
    public class RecordingViewModel : ReactiveObject
    {
        public RecordingViewModel(RecordingDto recordingDto)
        {
            Id = recordingDto.Id;
            Name = recordingDto.Name;
            Tracks = new ObservableCollection<TrackViewModel>(recordingDto.Tracks.Select(t => new TrackViewModel(t)));
            FilePath = recordingDto.FilePath;
            Length = recordingDto.Length;
        }
        
        public Guid Id { get; }

        [Reactive] public string Name { get; set; }
        
        [Reactive] public ObservableCollection<TrackViewModel> Tracks { get; set; }

        [Reactive] public string? FilePath { get; set; }

        [Reactive] public TimeSpan? Length { get; set; }
    }
}
