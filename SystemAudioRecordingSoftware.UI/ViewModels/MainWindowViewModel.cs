// (c) Johannes Wolfgruber, 2020

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Forms;
using SystemAudioRecordingSoftware.Core.Audio;

namespace SystemAudioRecordingSoftware.UI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly IAudioEngineService _engineService;
        private readonly PolygonWaveFormViewModel _visualization = new PolygonWaveFormViewModel();

        public MainWindowViewModel(IAudioEngineService? engineService = null)
        {
            _engineService = engineService ?? Locator.Current.GetService<IAudioEngineService>();

            Observable
                .FromEventPattern(_engineService, nameof(_engineService.CaptureStateChanged))
                .Subscribe(_ => IsRecording = _engineService.IsRecording);

            Observable
                .FromEventPattern(_engineService, nameof(_engineService.PlaybackStateChanged))
                .Subscribe(_ => IsPlaying = _engineService.IsPlaying);

            _engineService.SampleAvailable += OnSampleAvailable;

            var canStop = this.WhenAnyValue(
                x => x.IsRecording, x => x.IsPlaying,
                (r, p) => r || p);

            RecordCommand = ReactiveCommand.Create(OnRecord);
            PlayCommand = ReactiveCommand.Create(OnPlay);
            StopCommand = ReactiveCommand.Create(OnStop, canStop);
            SaveCommand = ReactiveCommand.Create(OnSave);
        }

        [Reactive] public bool IsPlaying { get; set; }
        [Reactive] public bool IsRecording { get; set; }
        public ReactiveCommand<Unit, Unit> PlayCommand { get; }
        public ReactiveCommand<Unit, Unit> RecordCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }
        [Reactive] public string Title { get; set; } = "System Audio Recording Software";
        public object Visualization => _visualization.Content;

        private void OnCaptureStateChanged(object? sender, EventArgs args)
        {
            IsRecording = _engineService.IsRecording;
        }

        private void OnPlay()
        {
            _engineService.Play();
        }

        private void OnRecord()
        {
            _engineService.Record();
        }

        private void OnSampleAvailable(object? sender, MinMaxValuesEventArgs args)
        {
            _visualization.OnSampleAvailable(args.MinValue, args.MaxValue);
        }

        private void OnSave()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Wav file (*.wav)|*.wav"
            };
            var result = saveFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                _engineService.Save(saveFileDialog.FileName);
            }
        }

        private void OnStop()
        {
            _engineService.Stop();
        }
    }
}
