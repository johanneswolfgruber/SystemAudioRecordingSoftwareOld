using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    public partial class WaveformControl
    {
        private void OnFollowPlayHeadClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            _audioWaveform!.ShouldFollowWaveform = _followPlayHeadButton?.IsChecked ?? true;;
        }

        private void OnRemoveSnipClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            var lineToRemove = _audioWaveform!.MainLineDisplay.SnipLines.FirstOrDefault(x => x.IsSelected);
            _audioWaveform.MainLineDisplay.RemoveSnipLine();
            SnipRemoved.Execute(lineToRemove?.TimeStamp);
        }

        private void OnAddSnipClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            _audioWaveform!.MainLineDisplay.AddSnipLine();
            SnipAdded.Execute(_audioWaveform.MainLineDisplay.SnipLines[^1].TimeStamp);
        }

        private void OnZoomOutClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            throw new NotImplementedException();
        }

        private void OnZoomInClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            throw new NotImplementedException();
        }

        private void OnDisplayAudioDataChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems is null)
            {
                return;
            }

            if (System.Windows.Application.Current is null)
            {
                return;
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() => _audioArray = DisplayAudioData.ToArray());
        }

        private static void OnDisplayAudioDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not WaveformControl control)
            {
                return;
            }

            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= control.OnDisplayAudioDataChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += control.OnDisplayAudioDataChanged;
            }
        }

        private void OnSnipTimeStampsChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (System.Windows.Application.Current == null)
            {
                return;
            }
            
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var newTimeStamps = args.NewItems?.OfType<TimeSpan>().ToList();
                var oldTimeStamps = args.OldItems?.OfType<TimeSpan>().ToList();
            
                if (newTimeStamps is null || _audioWaveform is null)
                {
                    return;
                }
            
                foreach (var timeStamp in newTimeStamps)
                {
                    _audioWaveform.MainLineDisplay.AddSnipLine(timeStamp);
                }
            
                if (oldTimeStamps is null || _audioWaveform.MainLineDisplay.SnipLines.Count == 0)
                {
                    return;
                }
            
                foreach (var timeStamp in oldTimeStamps)
                {
                    _audioWaveform.MainLineDisplay.RemoveSnipLine(timeStamp);
                }
            });
        }

        private static void OnSnipTimeStampsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not WaveformControl control)
            {
                return;
            }

            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= control.OnSnipTimeStampsChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += control.OnSnipTimeStampsChanged;
            }
        }

        private static void OnLengthInSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not WaveformControl control)
            {
                return;
            }

            control.UpdateLength((TimeSpan)e.NewValue);
        }

        private static void OnResetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not WaveformControl control)
            {
                return;
            }

            control.ResubscribeToResetObservable();
        }
    }
}