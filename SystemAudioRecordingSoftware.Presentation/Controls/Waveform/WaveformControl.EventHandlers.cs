using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    public partial class WaveformControl
    {
        private void OnMainElementMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (_audioWaveform?.MainElement == null)
            {
                return;
            }

            AddMarker(mouseButtonEventArgs.GetPosition(_audioWaveform?.MainElement).X);
        }

        private void OnMainElementMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (_audioWaveform?.MainElement == null || Mouse.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            MoveLine(mouseEventArgs.GetPosition(_audioWaveform?.MainElement).X);
        }

        private void OnOverviewElementMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (_audioWaveform?.MainElement is null)
            {
                return;
            }
            
            SetShouldFollowWaveform(false);
            if (_waveformSlider is not null)
            {
                _waveformSlider.ShouldFollowWaveform = _shouldFollowWaveform;
            }

            SetOverviewWaveformClickPosition(mouseButtonEventArgs.GetPosition(_audioWaveform?.OverviewElement));
            _waveformSlider?.OnRectangleCanvasMouseDown(sender, mouseButtonEventArgs);
        }
        
        private void OnOverviewElementMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            _waveformSlider?.OnRectangleCanvasMouseUp(sender, mouseButtonEventArgs);
        }

        private void OnOverviewElementMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (_audioWaveform?.MainElement is null || Mouse.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            SetOverviewWaveformClickPosition(mouseEventArgs.GetPosition(_audioWaveform?.OverviewElement));
            _waveformSlider?.OnRectangleCanvasMouseMove(sender, mouseEventArgs);
        }

        private void OnFollowPlayHeadClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            _shouldFollowWaveform = _followPlayHeadButton?.IsChecked ?? true;
            if (_waveformSlider is not null)
            {
                _waveformSlider.ShouldFollowWaveform = _shouldFollowWaveform;
            }
        }

        private void OnRemoveSnipClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_snipLines.Count <= 0 || _selectedLines == null || _mainLineCanvas == null ||
                _overviewLineCanvas == null)
            {
                return;
            }

            _snipLines.Remove(_selectedLines);

            if (SnipTimeStamps is IList<TimeSpan> snipTimeStamps)
            {
                snipTimeStamps.Remove(_selectedLines.Timestamp);
            }

            _mainLineCanvas.Children.Remove(_selectedLines.MainWaveformLine);
            _overviewLineCanvas.Children.Remove(_selectedLines.OverviewWaveformLine);

            SnipRemoved.Execute(_selectedLines.Timestamp);

            if (_snipLines.Count > 0)
            {
                SetSelectedLines(_snipLines.Last());
            }
            else
            {
                _selectedLines = null;
            }
        }

        private void OnAddSnipClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_markerLines is null)
            {
                return;
            }

            var lineContainer = AddSnipToCanvas(_markerLines.Timestamp);
            if (lineContainer is not null)
            {
                _snipLines.Add(lineContainer);
                SetSelectedLines(_snipLines.Last());
                if (SnipTimeStamps is IList<TimeSpan> snipTimeStamps)
                {
                    snipTimeStamps.Add(lineContainer.Timestamp);
                }

                SnipAdded.Execute(lineContainer.Timestamp);
            }
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

                if (newTimeStamps == null || _mainLineCanvas == null || _overviewLineCanvas == null)
                {
                    return;
                }

                foreach (var timeStamp in newTimeStamps)
                {
                    var lineContainer = AddSnipToCanvas(timeStamp);
                    if (lineContainer != null)
                    {
                        _snipLines.Add(lineContainer);
                        SetSelectedLines(_snipLines.Last());
                    }
                }

                if (oldTimeStamps == null || _snipLines.Count <= 0)
                {
                    return;
                }

                foreach (var timeStamp in oldTimeStamps)
                {
                    var affectedLines = _snipLines.Where(x => x.Timestamp == timeStamp).ToList();

                    foreach (var line in affectedLines)
                    {
                        _snipLines.Remove(line);

                        _mainLineCanvas.Children.Remove(line.MainWaveformLine);
                        _overviewLineCanvas.Children.Remove(line.OverviewWaveformLine);
                    }
                }

                if (_snipLines.Count > 0)
                {
                    SetSelectedLines(_snipLines.Last());
                }
                else
                {
                    _selectedLines = null;
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