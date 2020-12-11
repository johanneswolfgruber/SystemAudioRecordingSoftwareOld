// (c) Johannes Wolfgruber, 2020

using DynamicData;
using NAudio.CoreAudioApi;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using SystemAudioRecordingSoftware.Core.AudioEngine;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.UI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly IAudioEngineService _engineService;

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

            RecordingsList = new RecordingsListViewModel(_engineService);
            // RecordingsList.WhenAnyValue(x => x.SelectedRecordings).Subscribe(OnSelectedRecordingChanged);
        }

        public ReactiveCommand<Unit, Unit> RecordCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }
        public ReactiveCommand<Unit, Unit> SnipCommand { get; }
        public ReactiveCommand<Unit, Unit> BurnCommand { get; }

        [ObservableAsProperty] public bool IsRecording { get; }
        [Reactive] public RecordingsListViewModel RecordingsList { get; set; }
        [Reactive] public string Title { get; set; } = "System Audio Recording Software";

        [Reactive] public string? FilePath { get; set; }
        [Reactive] public ObservableCollection<float> AudioData { get; set; } = new();

        [Reactive] public ObservableCollection<TimeSpan> SnipTimeStamps { get; set; } = new();
        [Reactive] public TimeSpan LengthInSeconds { get; set; } = TimeSpan.Zero;

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
            _engineService.StartRecording();
            // _waveform.Reset();
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

        private void OnStop()
        {
            _engineService.StopRecording();
        }
    }
}