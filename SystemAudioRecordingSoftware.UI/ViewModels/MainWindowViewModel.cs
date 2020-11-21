// (c) Johannes Wolfgruber, 2020

using NAudio.CoreAudioApi;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using SystemAudioRecordingSoftware.Core.AudioEngine;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.UI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly IAudioEngineService _engineService;
        private readonly WaveformViewModel _waveform = new();

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
        }

        public ReactiveCommand<Unit, Unit> RecordCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }
        public ReactiveCommand<Unit, Unit> SnipCommand { get; }
        public ReactiveCommand<Unit, Unit> BurnCommand { get; }
        public object WaveformRenderer => _waveform.Content;
        [Reactive] public RecordingsListViewModel RecordingsList { get; set; }
        [Reactive] public bool IsRecording { get; set; }
        [Reactive] public string Title { get; set; } = "System Audio Recording Software";
        [Reactive] public string? FilePath { get; set; }

        private void OnAudioDataAvailable(AudioDataDto args)
        {
            _waveform.AddAudioData(args.Buffer, args.TotalNumberOfSingleChannelSamples, args.SampleRate);
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
            _waveform.Reset();
        }

        private void OnSelectedRecordingChanged(RecordingViewModel? vm)
        {
            FilePath = vm?.FilePath;

            if (FilePath is null)
            {
                return;
            }

            _waveform.Reset();

            var data = _engineService.GetAudioDisplayData(FilePath);

            _waveform.AddAudioData(data.Buffer, data.TotalNumberOfSingleChannelSamples, data.SampleRate);
        }

        private void OnSnip()
        {
            _engineService.SnipRecording();
            _waveform.AddSnip();
        }

        private void OnStop()
        {
            _engineService.Stop();
        }
    }
}