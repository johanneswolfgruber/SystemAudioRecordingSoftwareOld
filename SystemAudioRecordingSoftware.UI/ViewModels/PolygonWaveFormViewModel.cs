// (c) Johannes Wolfgruber, 2020

using System.Windows;
using SystemAudioRecordingSoftware.UI.Views;

namespace SystemAudioRecordingSoftware.UI.ViewModels
{
    public class PolygonWaveFormViewModel
    {
        private readonly PolygonWaveFormControl _polygonWaveFormControl = new PolygonWaveFormControl();

        public object Content => _polygonWaveFormControl;

        public void OnSampleAvailable(float min, float max)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _polygonWaveFormControl.AddValue(max, min);
            });
        }
    }
}
