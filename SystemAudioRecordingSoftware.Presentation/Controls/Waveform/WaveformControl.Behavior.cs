using System;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    public partial class WaveformControl
    {
        private void ResubscribeToResetObservable()
        {
            _resetSubscription?.Dispose();
            _resetSubscription = Reset.Subscribe(_ => ResetWaveform());
        }

        private void ResetWaveform()
        {
            // TODO: Reset AudioWaveform, WaveformSlider, LineDisplay etc.
        }

        private void RenderAudioWaveform()
        {
            if (_audioWaveform is null || _audioArray.Length == 0)
            {
                return;
            }
            
            SetShouldFollowWaveform(_audioWaveform.ShouldFollowWaveform);

            _audioWaveform.RenderWaveform(_audioArray, _length);
        }

        private void UpdateLength(TimeSpan newLength)
        {
            _length = newLength;
            if (_timeDisplayTextBlock != null)
            {
                _timeDisplayTextBlock.Text = $"TimeStamp: {_audioWaveform?.SelectedTimeStamp:mm\\:ss\\.f}s, " +
                                             $"Length: {_length:mm\\:ss\\.f}s";
            }
        }

        private void SetShouldFollowWaveform(bool shouldFollowWaveform)
        {
            if (_followPlayHeadButton is not null)
            {
                _followPlayHeadButton.IsChecked = shouldFollowWaveform;
            }
        }
    }
}