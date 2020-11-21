// (c) Johannes Wolfgruber, 2020

using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
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

        public RecordingsListViewModel(IAudioEngineService? engineService = null)
        {
            _engineService = engineService ?? Locator.Current.GetService<IAudioEngineService>();

            _engineService.RecordingsChanged()
                .Transform(r => new RecordingViewModel(r))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _recordings)
                .Subscribe();

            var canExportOrDelete = this.WhenAnyValue(
                x => x.SelectedRecording,
                x => x.SelectedTrack,
                (recording, track) => recording != null || track != null);

            ImportCommand = ReactiveCommand.Create(OnImport);
            ExportCommand = ReactiveCommand.Create(OnExport, canExportOrDelete);
            DeleteCommand = ReactiveCommand.Create(OnDelete, canExportOrDelete);
        }

        public ReadOnlyObservableCollection<RecordingViewModel> Recordings => _recordings;

        public ReactiveCommand<Unit, Unit> ImportCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
        [Reactive] public RecordingViewModel? SelectedRecording { get; set; }
        [Reactive] public TrackViewModel? SelectedTrack { get; set; }

        public void OnSelectedItemChanged(object selectedItem)
        {
            SelectedRecording = selectedItem as RecordingViewModel;
            SelectedTrack = selectedItem as TrackViewModel;
        }

        private void OnDelete()
        {
            if (SelectedRecording != null)
            {
                _engineService.RemoveRecording(SelectedRecording.Id);
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