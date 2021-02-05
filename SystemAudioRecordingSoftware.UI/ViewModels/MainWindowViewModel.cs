// (c) Johannes Wolfgruber, 2020

using DynamicData;
using NAudio.CoreAudioApi;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using SystemAudioRecordingSoftware.Core.AudioEngine;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.UI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly IAudioEngineService _engineService;
        private readonly Subject<Unit> _resetRequest = new();
        private Guid? _currentRecordingId;

        public MainWindowViewModel(IAudioEngineService? engineService = null)
        {
            _engineService = engineService ?? Locator.Current.GetService<IAudioEngineService>();

            _engineService.CaptureStateChanged
                .Select(x => x != CaptureState.Stopped)
                .ToPropertyEx(this, x => x.IsRecording);
            _engineService.AudioDataAvailable.Subscribe(OnAudioDataAvailable);

            var canStopOrSnip = this.WhenAnyValue(x => x.IsRecording);
            var canRecordOrBurn = this.WhenAnyValue(x => x.IsRecording, isRecording => !isRecording);

            RecordCommand = ReactiveCommand.Create(OnRecord, canRecordOrBurn);
            StopCommand = ReactiveCommand.Create(OnStop, canStopOrSnip);
            SnipCommand = ReactiveCommand.Create(OnSnip, canStopOrSnip);
            BurnCommand = ReactiveCommand.Create(OnBurn, canRecordOrBurn);
            SnipAddedCommand = ReactiveCommand.Create<TimeSpan, Unit>(OnSnipAdded);
            SnipRemovedCommand = ReactiveCommand.Create<TimeSpan, Unit>(OnSnipRemoved);

            RecordingsList = new RecordingsListViewModel(_engineService);
            SnipTimeStamps.CollectionChanged += SnipTimeStampsOnCollectionChanged;
            // RecordingsList.WhenAnyValue(x => x.SelectedRecordings).Subscribe(OnSelectedRecordingChanged);
        }

        public ReactiveCommand<Unit, Unit> RecordCommand { get; }

        public ReactiveCommand<Unit, Unit> StopCommand { get; }

        public ReactiveCommand<Unit, Unit> SnipCommand { get; }

        public ReactiveCommand<Unit, Unit> BurnCommand { get; }

        public ReactiveCommand<TimeSpan, Unit> SnipAddedCommand { get; }

        public ReactiveCommand<TimeSpan, Unit> SnipRemovedCommand { get; }

        [Reactive] public string Title { get; set; } = "System Audio Recording Software";

        public ObservableCollection<float> AudioData { get; } = new();

        public ObservableCollection<TimeSpan> SnipTimeStamps { get; } = new();

        public RecordingsListViewModel RecordingsList { get; }

        [ObservableAsProperty] public bool IsRecording { get; }

        [Reactive] public string? FilePath { get; set; }

        [Reactive] public TimeSpan LengthInSeconds { get; set; } = TimeSpan.Zero;

        public IObservable<Unit> ResetWaveform => _resetRequest;

        private void OnAudioDataAvailable(AudioDataDto data)
        {
            AudioData.AddRange(data.Buffer);
            LengthInSeconds += data.TotalTime;
        }

        private void OnBurn()
        {
            // TODO: implement
        }

        private void OnRecord()
        {
            _currentRecordingId = _engineService.StartRecording();
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
            var time = _engineService.SnipRecording();
            SnipTimeStamps.Add(time);
        }

        private Unit OnSnipAdded(TimeSpan timeStamp)
        {
            _engineService.SnipRecording(_currentRecordingId,
                timeStamp); // TODO: _currentRecordingId needs to change with selection
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

            _engineService.RemoveTrack(recording.Id, track.Id);
            SnipTimeStamps.Remove(timeStamp);

            return Unit.Default;
        }

        private void OnStop()
        {
            _engineService.StopRecording();
        }

        private void ResetWaveformView()
        {
            AudioData.Clear();
            LengthInSeconds = TimeSpan.Zero;
            SnipTimeStamps.Clear();
            _resetRequest.OnNext(Unit.Default);
        }

        private void SnipTimeStampsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Title = e.NewItems?.OfType<TimeSpan>().FirstOrDefault().ToString() ?? string.Empty;
        }
    }
}