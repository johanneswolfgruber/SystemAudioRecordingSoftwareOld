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
                .ObserveOnDispatcher()
                .Transform(r => new RecordingViewModel(r, OnSelectedTrackChanged))
                .Bind(out _recordings)
                .Subscribe();

            var isRecordingSelected = this.WhenAnyValue<RecordingsListViewModel, bool, RecordingViewModel?>(
                x => x.SelectedRecording, r => r != null);

            var isTrackSelected =
                this.WhenAnyValue<RecordingsListViewModel, bool, RecordingViewModel?>(x => x.SelectedRecording,
                    r => r?.SelectedTrack != null);

            var canExportOrDelete = isRecordingSelected.Concat(isTrackSelected);

            PlayCommand = ReactiveCommand.Create(OnPlay);
            PauseCommand = ReactiveCommand.Create(OnPause);
            StopCommand = ReactiveCommand.Create(OnStop);
            ImportCommand = ReactiveCommand.Create(OnImport);
            ExportCommand = ReactiveCommand.Create(OnExport, canExportOrDelete);
            DeleteCommand = ReactiveCommand.Create(OnDelete, canExportOrDelete);
        }

        public ReadOnlyObservableCollection<RecordingViewModel> Recordings => _recordings;

        public ReactiveCommand<Unit, Unit> PlayCommand { get; }

        public ReactiveCommand<Unit, Unit> PauseCommand { get; }

        public ReactiveCommand<Unit, Unit> StopCommand { get; }

        public ReactiveCommand<Unit, Unit> ImportCommand { get; }

        public ReactiveCommand<Unit, Unit> ExportCommand { get; }

        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

        [Reactive] public RecordingViewModel? SelectedRecording { get; set; }

        private void OnDelete()
        {
            if (SelectedRecording == null)
            {
                return;
            }

            if (SelectedRecording.SelectedTrack != null)
            {
                _engineService.RemoveTrack(SelectedRecording.Id, SelectedRecording.SelectedTrack.Id);
                return;
            }

            _engineService.RemoveRecording(SelectedRecording.Id);
        }

        private void OnExport()
        {
            // TODO: implement
        }

        private void OnImport()
        {
            // TODO: implement
        }

        private void OnPause()
        {
            _engineService.PausePlayback();
        }

        private void OnPlay()
        {
            _engineService.Play();
        }

        private void OnSelectedTrackChanged(RecordingViewModel vm)
        {
            if (vm.SelectedTrack == null)
            {
                return;
            }

            SelectedRecording = vm;
        }

        private void OnStop()
        {
            _engineService.StopPlayback();
        }
    }
}