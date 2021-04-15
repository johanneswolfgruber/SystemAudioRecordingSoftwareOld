using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    public partial class WaveformControl
    {
        private void OnMainLineCanvasMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (_audioWaveform?.MainElement == null)
            {
                return;
            }

            AddMarker(mouseButtonEventArgs.GetPosition(_audioWaveform?.MainElement).X);
        }

        private void OnMainLineCanvasMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (_audioWaveform?.MainElement == null || Mouse.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            MoveLine(mouseEventArgs.GetPosition(_audioWaveform?.MainElement).X);
        }

        private void OnOverviewLineCanvasMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (_audioWaveform?.OverviewElement == null)
            {
                return;
            }

            SetOverviewWaveformClickPosition(mouseButtonEventArgs.GetPosition(_audioWaveform?.OverviewElement));
        }

        private void OnOverviewLineCanvasMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (_audioWaveform?.OverviewElement == null || Mouse.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            SetOverviewWaveformClickPosition(mouseEventArgs.GetPosition(_audioWaveform?.OverviewElement));
        }

        private void OnFollowPlayHeadClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            _shouldFollowWaveform = _followPlayHeadButton?.IsChecked ?? true;
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
            if (_markerLines == null)
            {
                return;
            }

            var lineContainer = AddSnipToCanvas(_markerLines.Timestamp);
            if (lineContainer != null)
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
            if (args.NewItems == null)
            {
                return;
            }

            if (System.Windows.Application.Current == null)
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
            if (!(d is WaveformControl control))
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

        private void OnOverviewRectangleCanvasMouseMove(object sender, MouseEventArgs args)
        {
            if (_overviewRectangle is null || Mouse.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            var point = Mouse.GetPosition(_overviewRectangleCanvas);

            if (!_dragInProgress)
            {
                _mouseHitType = SetHitType(_overviewRectangle, point);
                SetMouseCursor();
                return;
            }

            var offsetX = point.X - _lastPoint.X;
            var newX = Canvas.GetLeft(_overviewRectangle);
            var newWidth = _overviewRectangle.Width;

            switch (_mouseHitType)
            {
                case HitType.Body:
                    newX += offsetX;
                    break;
                case HitType.LeftEdge:
                    newX += offsetX;
                    newWidth -= offsetX;
                    break;
                case HitType.RightEdge:
                    newWidth += offsetX;
                    break;
                case HitType.None:
                default:
                    break;
            }

            newX = Math.Clamp(newX, 0, _overviewRectangleCanvas.ActualWidth - _overviewRectangle.Width);
            newWidth = Math.Clamp(newWidth, 20, _overviewRectangleCanvas.ActualWidth);

            Canvas.SetLeft(_overviewRectangle, newX);
            _overviewRectangle.Width = newWidth;

            _lastPoint = point;
            if (_followPlayHeadButton is not null)
            {
                _followPlayHeadButton.IsChecked = false;
                _shouldFollowWaveform = false;
            }
        }

        private void OnOverviewRectangleCanvasMouseUp(object sender, MouseButtonEventArgs args)
        {
            _dragInProgress = false;
            Cursor = Cursors.Arrow;
        }

        private void OnOverviewRectangleCanvasMouseDown(object sender, MouseButtonEventArgs args)
        {
            _lastPoint = Mouse.GetPosition(_overviewRectangleCanvas);

            _mouseHitType = SetHitType(_overviewRectangle, _lastPoint);
            SetMouseCursor();

            if (_mouseHitType == HitType.None) return;

            _dragInProgress = true;
        }

        private void OnOverviewRectangleMouseLeave(object sender, MouseEventArgs args)
        {
            Cursor = Cursors.Arrow;
        }

        private void OnOverviewRectangleMouseEnter(object sender, MouseEventArgs args)
        {
            var x = args.GetPosition(_overviewRectangle).X;
            if (x > 10 && x < _overviewRectangle?.Width - 10)
            {
                Cursor = Cursors.ScrollWE;
                return;
            }

            Cursor = Cursors.SizeWE;
        }

        private void OnOverviewRectangleMouseMove(object sender, MouseEventArgs args)
        {
            var x = args.GetPosition(_overviewRectangle).X;
            if (x > 10 && x < _overviewRectangle?.Width - 10)
            {
                Cursor = Cursors.ScrollWE;
                return;
            }

            Cursor = Cursors.SizeWE;
        }
    }
}