// (c) Johannes Wolfgruber, 2020

using ReactiveUI;
using System.Collections.Generic;
using System.Windows;
using SystemAudioRecordingSoftware.UI.Views;

namespace SystemAudioRecordingSoftware.UI.ViewModels
{
    public class WaveformViewModel : ReactiveObject
    {
        private readonly WaveformView _waveformView = new();

        public object Content => _waveformView;

        public void AddAudioData(IEnumerable<float> buffer, int totalNumberOfSamples, int sampleRate)
        {
            Application.Current.Dispatcher.Invoke(() =>
                _waveformView.AddAudioData(buffer, totalNumberOfSamples, sampleRate));
        }

        public void AddSnip()
        {
            Application.Current.Dispatcher.Invoke(() => _waveformView.AddLiveSnip());
        }

        public void Reset()
        {
            Application.Current.Dispatcher.Invoke(() => _waveformView.Reset());
        }
    }
}