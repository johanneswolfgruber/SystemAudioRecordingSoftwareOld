// (c) Johannes Wolfgruber, 2020

using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using SystemAudioRecordingSoftware.Core.Audio;

namespace SystemAudioRecordingSoftware.UI.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly Recorder _recorder;
        private readonly PolygonWaveFormViewModel _visualization = new PolygonWaveFormViewModel();

        public MainWindowViewModel(Recorder recorder)
        {
            _recorder = recorder;

            _recorder.SampleAvailable += (s, a) =>
            {
                _visualization.OnSampleAvailable(a.MinValue, a.MaxValue);
            };

            SelectFolderCommand = new DelegateCommand(OnOpenFile);
            StartRecordingCommand = new DelegateCommand(async () => await OnStartRecording());
            StopRecordingCommand = new DelegateCommand(OnStopRecording);
        }

        public string? FileName { get; set; }
        public string? SelectedFolderPath { get; set; }
        public ICommand SelectFolderCommand { get; set; }
        public ICommand StartRecordingCommand { get; set; }
        public ICommand StopRecordingCommand { get; set; }
        public string Title { get; set; } = "System Audio Recording Software";
        public object Visualization => _visualization.Content;

        private void OnOpenFile()
        {
            using var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SystemAudioRecordingSoftware");

            if (result == DialogResult.OK)
            {
                path = dialog.SelectedPath;
            }

            SelectedFolderPath = path;
            _recorder.FilePathManager.SetRecordingFolder(path);
        }

        private async Task OnStartRecording()
        {
            var fileName = FileName ?? "recording";

            _recorder.FilePathManager.SetRecordingFile(fileName);
            await _recorder.StartRecording();
        }

        private void OnStopRecording()
        {
            _recorder.StopRecording();
        }
    }
}
