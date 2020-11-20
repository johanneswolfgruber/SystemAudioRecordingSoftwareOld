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
        private readonly ReadOnlyObservableCollection<RecordingViewModel> _recordings;
        private readonly WaveformRendererViewModel _waveformRenderer = new WaveformRendererViewModel();

        public MainWindowViewModel(IAudioEngineService? engineService = null)
        {
            _engineService = engineService ?? Locator.Current.GetService<IAudioEngineService>();

            _engineService.CaptureStateChanged.Subscribe(OnCaptureStateChanged);
            _engineService.AudioDataAvailable.Subscribe(OnAudioDataAvailable);
            _engineService.RecordingsChanged()
                .Transform(r => new RecordingViewModel(r))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _recordings)
                .Subscribe();

            var canStopOrSnip = this.WhenAnyValue(x => x.IsRecording);
            var canRecordOrBurn = this.WhenAnyValue(x => x.IsRecording, isRecording => !isRecording);
            var canDelete = this.WhenAnyValue(
                x => x.SelectedRecording, 
                x => x.SelectedTrack, 
                (recording, track) => recording != null || track != null);

            RecordCommand = ReactiveCommand.Create(OnRecord, canRecordOrBurn);
            StopCommand = ReactiveCommand.Create(OnStop, canStopOrSnip);
            SnipCommand = ReactiveCommand.Create(OnSnip, canStopOrSnip);
            BurnCommand = ReactiveCommand.Create(OnBurn, canRecordOrBurn);
            DeleteCommand = ReactiveCommand.Create(OnDelete, canDelete);

            this.WhenAnyValue(x => x.SelectedRecording)
                .Subscribe(OnSelectedRecordingChanged);
        }

        public ReadOnlyObservableCollection<RecordingViewModel> Recordings => _recordings;

        [Reactive] public bool IsRecording { get; set; }

        public ReactiveCommand<Unit, Unit> RecordCommand { get; }

        public ReactiveCommand<Unit, Unit> StopCommand { get; }

        public ReactiveCommand<Unit, Unit> SnipCommand { get; }

        public ReactiveCommand<Unit, Unit> BurnCommand { get; }

        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

        [Reactive] public string Title { get; set; } = "System Audio Recording Software";

        [Reactive] public RecordingViewModel? SelectedRecording { get; set; }

        [Reactive] public TrackViewModel? SelectedTrack { get; set; }

        public object WaveformRenderer => _waveformRenderer.Content;

        [Reactive] public string? FilePath { get; set; }

        public void OnSelectedItemChanged(object selectedItem)
        {
            SelectedRecording = selectedItem as RecordingViewModel;
            SelectedTrack = selectedItem as TrackViewModel;
        }

        private void OnAudioDataAvailable(AudioDataDto args)
        {
            _waveformRenderer.AddAudioData(args.Buffer, args.TotalNumberOfSingleChannelSamples, args.SampleRate);
        }

        private void OnBurn()
        {
            throw new NotImplementedException();
        }

        private void OnCaptureStateChanged(CaptureState _)
        {
            IsRecording = _engineService.IsRecording;
        }

        private void OnDelete()
        {
            if (SelectedRecording != null)
            {
                _engineService.RemoveRecording(SelectedRecording.Id);
            }
        }

        private void OnRecord()
        {
            _engineService.Record();
            _waveformRenderer.Reset();
        }

        private void OnSelectedRecordingChanged(RecordingViewModel? vm)
        {
            FilePath = vm?.FilePath;
            
            if (FilePath is null)
            {
                return;
            }
            
            _waveformRenderer.Reset();

            var data = _engineService.GetAudioDisplayData(FilePath);

            _waveformRenderer.AddAudioData(data.Buffer, data.TotalNumberOfSingleChannelSamples, data.SampleRate);
        }

        private void OnSnip()
        {
            _engineService.SnipRecording();
            _waveformRenderer.AddSnip();
        }

        private void OnStop()
        {
            _engineService.Stop();
        }
    }
}