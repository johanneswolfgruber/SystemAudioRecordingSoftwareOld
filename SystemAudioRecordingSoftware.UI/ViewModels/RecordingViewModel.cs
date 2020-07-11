// (c) Johannes Wolfgruber, 2020
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Windows.Forms;
using SystemAudioRecordingSoftware.Core.AudioEngine;
using SystemAudioRecordingSoftware.Core.Model;

namespace SystemAudioRecordingSoftware.UI.ViewModels
{
    public class RecordingViewModel
    {
        private readonly IAudioEngineService _engineService;

        public RecordingViewModel(Recording recording, IAudioEngineService? engineService = null)
        {
            _engineService = engineService ?? Locator.Current.GetService<IAudioEngineService>();

            _engineService
                .CaptureStateChanged
                .Subscribe(_ => IsRecording = _engineService.IsRecording);

            _engineService
                .PlaybackStateChanged
                .Subscribe(_ => IsPlaying = _engineService.IsPlaying);

            Name = recording.Name;
            FilePath = recording.FilePath;
            Length = recording.Length;

            var canStop = this.WhenAnyValue(
                x => x.IsRecording, x => x.IsPlaying,
                (r, p) => r || p);

            PlayCommand = ReactiveCommand.Create(OnPlay);
            PauseCommand = ReactiveCommand.Create(OnPause);
            StopCommand = ReactiveCommand.Create(OnStop, canStop);
            SaveCommand = ReactiveCommand.Create(OnSave);
        }

        [Reactive] public bool IsPlaying { get; set; }

        [Reactive] public bool IsRecording { get; set; }

        [Reactive] public string Name { get; set; } = string.Empty;

        [Reactive] public string FilePath { get; set; } = string.Empty;

        [Reactive] public TimeSpan Length { get; set; }

        public ReactiveCommand<Unit, Unit> PlayCommand { get; }

        public ReactiveCommand<Unit, Unit> PauseCommand { get; }

        public ReactiveCommand<Unit, Unit> StopCommand { get; }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

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

        private void OnPause()
        {
            _engineService.Pause();
        }

        private void OnPlay()
        {
            _engineService.Play();
        }
    }
}
