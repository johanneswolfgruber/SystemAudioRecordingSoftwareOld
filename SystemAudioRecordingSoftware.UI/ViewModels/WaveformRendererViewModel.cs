// (c) Johannes Wolfgruber, 2020

using System.Collections.Generic;
using System.Windows;
using SystemAudioRecordingSoftware.UI.Views;

namespace SystemAudioRecordingSoftware.UI.ViewModels
{
    public class WaveformRendererViewModel
    {
        private readonly WaveformRenderer _waveformRenderer = new();

        public object Content => _waveformRenderer;

        public void AddAudioData(IEnumerable<float> buffer, int totalNumberOfSamples, int sampleRate)
        {
            Application.Current.Dispatcher.Invoke(() => _waveformRenderer.AddAudioData(buffer, totalNumberOfSamples, sampleRate));
        }

        public void Reset()
        {
            Application.Current.Dispatcher.Invoke(() => _waveformRenderer.Reset());
        }

        public void AddSnip()
        {
            Application.Current.Dispatcher.Invoke(() => _waveformRenderer.AddLiveSnip());
        }
    }
}