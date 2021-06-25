// (c) Johannes Wolfgruber, 2020

using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using SystemAudioRecordingSoftware.Application.Interfaces;
using SystemAudioRecordingSoftware.Domain.Events;
using SystemAudioRecordingSoftware.Domain.Model;

namespace SystemAudioRecordingSoftware.Presentation.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly IRecordingService _recordingService;
        private readonly IRecordsService _recordsService;
        private readonly Subject<Unit> _resetRequest = new();
        private readonly ISnippingService _snippingService;
        private Guid? _currentRecordingId;

        public MainWindowViewModel(
            IRecordingService? recordingService = null,
            IPlaybackService? playbackService = null,
            IRecordsService? recordsService = null,
            ISnippingService? snippingService = null)
        {
            _recordingService = recordingService ?? Locator.Current.GetService<IRecordingService>();
            _recordsService = recordsService ?? Locator.Current.GetService<IRecordsService>();
            _snippingService = snippingService ?? Locator.Current.GetService<ISnippingService>();

            _recordingService.CaptureStateChanged += OnCaptureStateChanged;
            _recordingService.DisplayDataProvider.DataAvailable += OnAudioDataAvailable;

            var canStopOrSnip = this.WhenAnyValue(x => x.IsRecording);
            var canRecordOrBurn = this.WhenAnyValue(x => x.IsRecording, isRecording => !isRecording);

            RecordCommand = ReactiveCommand.Create(OnRecord, canRecordOrBurn);
            StopCommand = ReactiveCommand.Create(OnStop, canStopOrSnip);
            SnipCommand = ReactiveCommand.Create(OnSnip, canStopOrSnip);
            BurnCommand = ReactiveCommand.Create(OnBurn, canRecordOrBurn);
            SnipsChangedCommand = ReactiveCommand.Create<IReadOnlyList<TimeSpan>, Unit>(OnSnipsChanged);
            SnipAddedCommand = ReactiveCommand.Create<TimeSpan, Unit>(OnSnipAdded);
            SnipRemovedCommand = ReactiveCommand.Create<TimeSpan, Unit>(OnSnipRemoved);

            RecordingsList = new RecordingsListViewModel(playbackService, recordsService);
            SnipTimeStamps.CollectionChanged += SnipTimeStampsOnCollectionChanged;
            // RecordingsList.WhenAnyValue(x => x.SelectedRecordings).Subscribe(OnSelectedRecordingChanged);
        }

        public ReactiveCommand<Unit, Unit> RecordCommand { get; }

        public ReactiveCommand<Unit, Unit> StopCommand { get; }

        public ReactiveCommand<Unit, Unit> SnipCommand { get; }

        public ReactiveCommand<Unit, Unit> BurnCommand { get; }

        public ReactiveCommand<IReadOnlyList<TimeSpan>, Unit> SnipsChangedCommand { get; }

        public ReactiveCommand<TimeSpan, Unit> SnipAddedCommand { get; }

        public ReactiveCommand<TimeSpan, Unit> SnipRemovedCommand { get; }

        [Reactive] public string Title { get; set; } = "System Audio Recording Software";

        public ObservableCollection<AudioDataPoint> AudioData { get; } = new();

        public ObservableCollection<TimeSpan> SnipTimeStamps { get; } = new();

        public RecordingsListViewModel RecordingsList { get; }

        [Reactive] public bool IsRecording { get; set; }

        [Reactive] public string? FilePath { get; set; }

        [Reactive] public TimeSpan TotalTime { get; set; } = TimeSpan.Zero;

        public IObservable<Unit> ResetWaveform => _resetRequest;

        private void OnAudioDataAvailable(object? sender, AudioDataAvailableEventArgs e)
        {
            AudioData.AddRange(e.AudioData.Buffer);
            TotalTime = _recordingService.DisplayDataProvider.TotalTime;
        }

        private void OnBurn()
        {
            // TODO: implement
        }

        private void OnCaptureStateChanged(object? sender, CaptureStateChangedEventArgs e)
        {
            IsRecording = _recordingService.IsRecording;
        }

        private void OnRecord()
        {
            var result = _recordingService.StartRecording();
            if (result.Succeeded)
            {
                _currentRecordingId = result.Value;
            }

            ResetWaveformView();
        }


        // private void OnSelectedRecordingChanged(RecordingViewModel? vm)
        // {
        //     FilePath = vm?.FilePath;
        //
        //     if (FilePath is null)
        //     {
        //         return;
        //     }
        //
        //     // _waveform.Reset();
        //
        //     var data = _engineService.GetAudioDisplayData(FilePath);
        //
        //     // _waveform.AddAudioData(data.Buffer, data.TotalNumberOfSingleChannelSamples, data.SampleRate);
        // }

        private void OnSnip()
        {
            var snippingResult = _snippingService.SnipCurrentRecording();
            if (snippingResult.Failed)
            {
                return;
            }
            SnipTimeStamps.Add(snippingResult.Value);
        }

        private Unit OnSnipsChanged(IReadOnlyList<TimeSpan> arg)
        {
            if (_currentRecordingId is null)
            {
                return Unit.Default;
            }
            
            _snippingService.UpdateSnipsForRecording(_currentRecordingId.Value, arg);
            return Unit.Default;
        }

        private Unit OnSnipAdded(TimeSpan timeStamp)
        {
            if (_currentRecordingId is null)
            {
                return Unit.Default;
            }

            _snippingService!.SnipRecording(_currentRecordingId.Value, timeStamp); // TODO: _currentRecordingId needs to change with selection
            SnipTimeStamps.Add(timeStamp);

            return Unit.Default;
        }

        private Unit OnSnipRemoved(TimeSpan timeStamp)
        {
            var recording = RecordingsList.Recordings.FirstOrDefault(r => r.Id == _currentRecordingId);
            var track = recording?.Tracks.FirstOrDefault(t => t.Start == timeStamp);

            if (recording == null || track == null)
            {
                return Unit.Default;
            }

            _recordsService.RemoveTrack(recording.Id, track.Id);
            SnipTimeStamps.Remove(timeStamp);

            return Unit.Default;
        }

        private void OnStop()
        {
            _recordingService.StopRecording();
        }

        private void ResetWaveformView()
        {
            AudioData.Clear();
            TotalTime = TimeSpan.Zero;
            SnipTimeStamps.Clear();
            _resetRequest.OnNext(Unit.Default);
        }

        private void SnipTimeStampsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // if (_currentRecordingId is null)
            // {
            //     return;
            // }
            //
            // _snippingService.UpdateSnipsForRecording(_currentRecordingId.Value, SnipTimeStamps.ToList());
        }
    }
}