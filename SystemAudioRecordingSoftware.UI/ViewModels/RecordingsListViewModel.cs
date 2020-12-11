// (c) Johannes Wolfgruber, 2020

using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using SystemAudioRecordingSoftware.Core.AudioEngine;

namespace SystemAudioRecordingSoftware.UI.ViewModels
{
    public class RecordingsListViewModel : ReactiveObject
    {
        private readonly IAudioEngineService _engineService;
        private readonly ReadOnlyObservableCollection<RecordingViewModel> _recordings;
        private readonly ObservableCollection<RecordingViewModel> _selectedRecordings = new();
        private readonly ObservableCollection<TrackViewModel> _selectedTracks = new();

        public RecordingsListViewModel(IAudioEngineService? engineService = null)
        {
            _engineService = engineService ?? Locator.Current.GetService<IAudioEngineService>();

            _engineService.RecordingsChanged()
                .Transform(r => new RecordingViewModel(r))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _recordings)
                .Subscribe();

            var isRecordingSelected = _selectedRecordings
                .ToObservableChangeSet(x => x)
                .ToCollection()
                .Select(x => x.Count > 0);

            var isTrackSelected = _selectedTracks
                .ToObservableChangeSet(x => x)
                .ToCollection()
                .Select(x => x.Count > 0);

            var canExportOrDelete = isRecordingSelected.Concat(isTrackSelected);

            ImportCommand = ReactiveCommand.Create(OnImport);
            ExportCommand = ReactiveCommand.Create(OnExport, canExportOrDelete);
            DeleteCommand = ReactiveCommand.Create(OnDelete, canExportOrDelete);
        }

        public ReadOnlyObservableCollection<RecordingViewModel> Recordings => _recordings;

        public ReactiveCommand<Unit, Unit> ImportCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

        public void OnSelectedRecordingsChanged(IEnumerable<RecordingViewModel> selectedRecordings)
        {
            _selectedRecordings.Clear();
            _selectedRecordings.AddRange(selectedRecordings);
        }

        public void OnSelectedTracksChanged(IEnumerable<TrackViewModel> selectedTracks)
        {
            _selectedTracks.Clear();
            _selectedTracks.AddRange(selectedTracks);
        }

        private void OnDelete()
        {
            if (_selectedRecordings.Count <= 0)
            {
                return;
            }

            foreach (var recording in _selectedRecordings)
            {
                _engineService.RemoveRecording(recording.Id);
            }
        }

        private void OnExport()
        {
            // TODO: implement
        }

        private void OnImport()
        {
            // TODO: implement
        }
    }
}