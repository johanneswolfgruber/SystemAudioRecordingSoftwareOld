// (c) Johannes Wolfgruber, 2020

using NAudio.CoreAudioApi;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive;
using SystemAudioRecordingSoftware.Core.AudioEngine;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.UI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly List<float> _audioData = new();
        private readonly IAudioEngineService _engineService;
        private int _numberOfSamples;

        public MainWindowViewModel(IAudioEngineService? engineService = null)
        {
            _engineService = engineService ?? Locator.Current.GetService<IAudioEngineService>();

            _engineService.CaptureStateChanged.Subscribe(OnCaptureStateChanged);
            _engineService.AudioDataAvailable.Subscribe(OnAudioDataAvailable);

            var canStopOrSnip = this.WhenAnyValue(x => x.IsRecording);
            var canRecordOrBurn = this.WhenAnyValue(x => x.IsRecording, isRecording => !isRecording);

            RecordCommand = ReactiveCommand.Create(OnRecord, canRecordOrBurn);
            StopCommand = ReactiveCommand.Create(OnStop, canStopOrSnip);
            SnipCommand = ReactiveCommand.Create(OnSnip, canStopOrSnip);
            BurnCommand = ReactiveCommand.Create(OnBurn, canRecordOrBurn);

            RecordingsList = new RecordingsListViewModel(_engineService);
            RecordingsList.WhenAnyValue(x => x.SelectedRecording).Subscribe(OnSelectedRecordingChanged);

            AudioData = Array.Empty<float>();
        }

        public ReactiveCommand<Unit, Unit> RecordCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }
        public ReactiveCommand<Unit, Unit> SnipCommand { get; }
        public ReactiveCommand<Unit, Unit> BurnCommand { get; }
        [Reactive] public RecordingsListViewModel RecordingsList { get; set; }
        [Reactive] public bool IsRecording { get; set; }
        [Reactive] public string Title { get; set; } = "System Audio Recording Software";
        [Reactive] public string? FilePath { get; set; }
        [Reactive] public float[] AudioData { get; set; }
        [Reactive] public TimeSpan LengthInSeconds { get; set; }

        private void OnAudioDataAvailable(AudioDataDto args)
        {
            _audioData.AddRange(args.Buffer);
            AudioData = _audioData.ToArray();
            this.RaisePropertyChanged(nameof(AudioData));
            _numberOfSamples += args.TotalNumberOfSingleChannelSamples;
            LengthInSeconds = TimeSpan.FromSeconds(((double)_numberOfSamples) / args.SampleRate);

            // _waveform.AddAudioData(args.Buffer, args.TotalNumberOfSingleChannelSamples, args.SampleRate);
        }

        private void OnBurn()
        {
            // TODO: implement
        }

        private void OnCaptureStateChanged(CaptureState _)
        {
            IsRecording = _engineService.IsRecording;
        }

        private void OnRecord()
        {
            _engineService.Record();
            // _waveform.Reset();
        }

        private void OnSelectedRecordingChanged(RecordingViewModel? vm)
        {
            FilePath = vm?.FilePath;

            if (FilePath is null)
            {
                return;
            }

            // _waveform.Reset();

            var data = _engineService.GetAudioDisplayData(FilePath);

            // _waveform.AddAudioData(data.Buffer, data.TotalNumberOfSingleChannelSamples, data.SampleRate);
        }

        private void OnSnip()
        {
            _engineService.SnipRecording();
            // _waveform.AddSnip();
        }

        private void OnStop()
        {
            _engineService.Stop();
        }
    }
}