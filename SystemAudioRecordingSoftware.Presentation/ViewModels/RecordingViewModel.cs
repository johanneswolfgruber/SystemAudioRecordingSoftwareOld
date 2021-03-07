// (c) Johannes Wolfgruber, 2020

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using SystemAudioRecordingSoftware.Domain.Model;

namespace SystemAudioRecordingSoftware.Presentation.ViewModels
{
    public class RecordingViewModel : ReactiveObject
    {
        private readonly Action<RecordingViewModel> _selectedTrackChangedCallback;
        private TrackViewModel? _selectedTrack;

        public RecordingViewModel(RecordingDto recordingDto, Action<RecordingViewModel> selectedTrackChangedCallback)
        {
            _selectedTrackChangedCallback = selectedTrackChangedCallback;
            Id = recordingDto.Id;
            Name = recordingDto.Name;
            Tracks = new ObservableCollection<TrackViewModel>(recordingDto.Tracks.Select(t => new TrackViewModel(t)));
            FilePath = recordingDto.FilePath;
            Length = recordingDto.Length;
        }

        public Guid Id { get; }

        [Reactive] public string Name { get; set; }

        [Reactive] public ObservableCollection<TrackViewModel> Tracks { get; set; }

        public TrackViewModel? SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedTrack, value);
                _selectedTrackChangedCallback(this);
            }
        }

        [Reactive] public string? FilePath { get; set; }

        [Reactive] public TimeSpan? Length { get; set; }
    }
}