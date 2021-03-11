// (c) Johannes Wolfgruber, 2020

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using SystemAudioRecordingSoftware.Application.Interfaces;
using SystemAudioRecordingSoftware.Domain.Events;

namespace SystemAudioRecordingSoftware.Presentation.ViewModels
{
    public class RecordingsListViewModel : ReactiveObject
    {
        private readonly IPlaybackService _playbackService;
        private readonly IRecordsService _recordsService;

        public RecordingsListViewModel(
            IPlaybackService? playbackService = null,
            IRecordsService? recordsService = null)
        {
            _playbackService = playbackService ?? Locator.Current.GetService<IPlaybackService>();
            _recordsService = recordsService ?? Locator.Current.GetService<IRecordsService>();

            _recordsService.RecordingsCollectionChanged += OnRecordingsCollectionChanged;
            _recordsService.RecordingChanged += OnRecordingChanged;

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

        public ReactiveCommand<Unit, Unit> PlayCommand { get; }

        public ReactiveCommand<Unit, Unit> PauseCommand { get; }

        public ReactiveCommand<Unit, Unit> StopCommand { get; }

        public ReactiveCommand<Unit, Unit> ImportCommand { get; }

        public ReactiveCommand<Unit, Unit> ExportCommand { get; }

        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

        [Reactive] public RecordingViewModel? SelectedRecording { get; set; }

        [Reactive] public ObservableCollection<RecordingViewModel> Recordings { get; set; } = new();

        private void OnDelete()
        {
            if (SelectedRecording == null)
            {
                return;
            }

            if (SelectedRecording.SelectedTrack != null)
            {
                _recordsService.RemoveTrack(SelectedRecording.Id, SelectedRecording.SelectedTrack.Id);
                return;
            }

            _recordsService.RemoveRecording(SelectedRecording.Id);
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
            _playbackService.PausePlayback();
        }

        private void OnPlay()
        {
            if (SelectedRecording?.FilePath == null)
            {
                return;
            }

            _playbackService.Play(SelectedRecording.FilePath);
        }

        private void OnRecordingChanged(object? sender, RecordingChangedEventArgs e)
        {
            var changedRecording = Recordings.FirstOrDefault(r => r.Id == e.Recording.Id);
            if (changedRecording is null)
            {
                return;
            }

            var index = Recordings.IndexOf(changedRecording);
            Recordings[index] = new RecordingViewModel(e.Recording, OnSelectedTrackChanged);
        }

        private void OnRecordingsCollectionChanged(object? sender, RecordingsCollectionChangedEventArgs e)
        {
            Recordings =
                new ObservableCollection<RecordingViewModel>(e.Recordings.Select(r =>
                    new RecordingViewModel(r, OnSelectedTrackChanged)));
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
            _playbackService.StopPlayback();
        }
    }
}