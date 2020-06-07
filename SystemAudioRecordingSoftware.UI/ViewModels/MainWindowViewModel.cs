// (c) Johannes Wolfgruber, 2020

using Prism.Commands;
using Prism.Mvvm;
using PropertyChanged;
using System;
using System.ComponentModel;
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
        private readonly IAudioEngineService _engineService;
        private readonly PolygonWaveFormViewModel _visualization = new PolygonWaveFormViewModel();

        public MainWindowViewModel(IAudioEngineService engineService)
        {
            _engineService = engineService;

            _engineService.SampleAvailable += OnSampleAvailable;
            _engineService.CaptureStateChanged += OnCaptureStateChanged;

            RecordCommand = new DelegateCommand(OnRecord);
            PlayCommand = new DelegateCommand(OnPlay);
            StopCommand = new DelegateCommand(OnStop);
            SaveCommand = new DelegateCommand(OnSave);
        }

        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand RecordCommand { get; set; }

        public DelegateCommand SaveCommand { get; set; }

        public DelegateCommand StopCommand { get; set; }

        public string Title { get; set; } = "System Audio Recording Software";

        public object Visualization => _visualization.Content;

        [SuppressPropertyChangedWarnings]
        private void OnCaptureStateChanged(object? sender, EventArgs args)
        {
            StopCommand.RaiseCanExecuteChanged();
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
