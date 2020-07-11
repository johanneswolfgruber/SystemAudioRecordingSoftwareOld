// (c) Johannes Wolfgruber, 2020

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using SystemAudioRecordingSoftware.Core.Audio;
using SystemAudioRecordingSoftware.Core.AudioEngine;

namespace SystemAudioRecordingSoftware.UI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly IAudioEngineService _engineService;
        private readonly PolygonWaveFormViewModel _visualization = new PolygonWaveFormViewModel();

        public MainWindowViewModel(IAudioEngineService? engineService = null)
        {
            _engineService = engineService ?? Locator.Current.GetService<IAudioEngineService>();

            _engineService
                .CaptureStateChanged
                .Subscribe(_ => IsRecording = _engineService.IsRecording);

            _engineService
                .SampleAvailable
                .Subscribe(x => OnSampleAvailable(x));

            _engineService
                .RecordingsChanged
                .Subscribe(x => Recordings =
                    new ObservableCollection<RecordingViewModel>(x.Select(r => new RecordingViewModel(r))));

            var canStop = this.WhenAnyValue(
                x => x.IsRecording);

            RecordCommand = ReactiveCommand.Create(OnRecord);
            StopCommand = ReactiveCommand.Create(OnStop, canStop);
            Recordings = new ObservableCollection<RecordingViewModel>();
        }

        [Reactive] public bool IsRecording { get; set; }
        public ReactiveCommand<Unit, Unit> RecordCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }
        [Reactive] public string Title { get; set; } = "System Audio Recording Software";
        [Reactive] public ObservableCollection<RecordingViewModel> Recordings { get; set; }
        public object Visualization => _visualization.Content;

        private void OnRecord()
        {
            _engineService.Record();
        }

        private void OnSampleAvailable(MinMaxValuesEventArgs args)
        {
            _visualization.OnSampleAvailable(args.MinValue, args.MaxValue);
        }

        private void OnStop()
        {
            _engineService.Stop();
        }
    }
}
