﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using SystemAudioRecordingSoftware.Presentation.Controls.Lines;

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
            _mainLineCanvas?.Children.Clear();
            _overviewLineCanvas?.Children.Clear();
            _snipLines.Clear();
            _currentTimestamp = TimeSpan.Zero;
        }

        private void RenderAudioWaveform()
        {
            if (_overviewRectangle is null || _audioArray.Length == 0)
            {
                return;
            }

            _overviewRectangle.Visibility = Visibility.Visible;

            if (_shouldFollowWaveform)
            {
                Canvas.SetLeft(_overviewRectangle, ActualWidth - _overviewRectangle.Width);
            }
            
            var leftEdge = Canvas.GetLeft(_overviewRectangle);
            var rightEdge = leftEdge + _overviewRectangle.Width;
            
            _audioWaveform?.RenderWaveform(_audioArray, _length, new RectangleEdges(leftEdge, rightEdge));

            _snipLines.ForEach(l => l.UpdateMainWaveformLineX(ToMainWaveformXFromTimeStamp, 0, ActualWidth));
            _markerLines?.UpdateMainWaveformLineX(ToMainWaveformXFromTimeStamp, 0, ActualWidth);
            _snipLines.ForEach(l => l.UpdateOverviewWaveformLineX(ToOverviewWaveformXFromTimeStamp));
            _markerLines?.UpdateOverviewWaveformLineX(ToOverviewWaveformXFromTimeStamp);
        }

        private void UpdateLength(TimeSpan newLength)
        {
            _length = newLength;
            if (_timeDisplayTextBlock != null)
            {
                _timeDisplayTextBlock.Text = $"TimeStamp: {_currentTimestamp.TotalSeconds}s, " +
                                             $"Length: {_length:mm\\:ss\\.f}s";
            }
        }

        private void SetOverviewWaveformClickPosition(Point clickPosition)
        {
            _shouldFollowWaveform = false;
            if (_followPlayHeadButton != null)
            {
                _followPlayHeadButton.IsChecked = _shouldFollowWaveform;
            }

            if (_audioWaveform is null)
            {
                throw new NullReferenceException("AudioWaveform cannot be null");
            }
            
            var maxTimeStamp = _length - WidthToTimeSpan(_audioWaveform.OverviewElement.CanvasSize.Width);
            _currentTimestamp = TimeSpan.FromMilliseconds(
                Math.Min(ToTimestampFromOverviewWaveformX(clickPosition.X).TotalMilliseconds, maxTimeStamp.TotalMilliseconds));
        }

        private void AddMarker(double x)
        {
            if (_mainLineCanvas == null || _overviewLineCanvas == null)
            {
                return;
            }

            var timestamp = ToTimestampFromMainWaveformX(x);

            if (_markerLines != null)
            {
                _mainLineCanvas.Children.Remove(_markerLines.MainWaveformLine);
                _overviewLineCanvas.Children.Remove(_markerLines.OverviewWaveformLine);
            }

            _markerLines = AddSnipToCanvas(timestamp);
            _markerLines?.SetStroke(MarkerLineBrush);
        }

        private void MoveLine(double newX)
        {
            if (_mainLineCanvas == null || _overviewLineCanvas == null)
            {
                return;
            }

            if (_selectedLines == null || !(Math.Abs(_selectedLines.MainWaveformLine.X1 - newX) < 10))
            {
                SetSelectedLines(null);
                MoveLine(_markerLines, newX);
                return;
            }

            if (_markerLines != null)
            {
                _mainLineCanvas.Children.Remove(_markerLines.MainWaveformLine);
                _overviewLineCanvas.Children.Remove(_markerLines.OverviewWaveformLine);
                _markerLines = null;
            }

            MoveLine(_selectedLines, newX);
        }

        private void MoveLine(LineContainer? line, double newX)
        {
            if (line == null)
            {
                return;
            }

            var timestamp = ToTimestampFromMainWaveformX(newX);

            var index = _snipLines.IndexOf(line);
            if (index > 0)
            {
                timestamp = TimeSpan.FromSeconds(Math.Clamp(timestamp.TotalSeconds,
                    _snipLines[index - 1].Timestamp.TotalSeconds + 0.05,
                    index < _snipLines.Count - 1
                        ? _snipLines[index + 1].Timestamp.TotalSeconds - 0.05
                        : _length.TotalSeconds));
            }
            else
            {
                timestamp = TimeSpan.FromSeconds(Math.Clamp(timestamp.TotalSeconds,
                    0,
                    _length.TotalSeconds));
            }

            line.Timestamp = timestamp;
            line.UpdateOverviewWaveformLineX(ToOverviewWaveformXFromTimeStamp);
            line.UpdateMainWaveformLineX(ToMainWaveformXFromTimeStamp, 0, ActualWidth);

            if (index >= 0 && index < SnipTimeStamps.Count)
            {
                SnipTimeStamps[index] = timestamp;
            }
        }

        private void SetSelectedLines(LineContainer? selectedLines)
        {
            foreach (var l in _snipLines)
            {
                l.SetStroke(LineBrush);
            }

            _selectedLines = selectedLines;
            _selectedLines?.SetStroke(SelectedLineBrush);
        }

        private LineContainer? AddSnipToCanvas(TimeSpan timestamp)
        {
            if (_mainLineCanvas == null || _overviewLineCanvas == null)
            {
                return null;
            }

            var waveformLine = CreateSnipLine(
                LineType.MainWaveformLine,
                ToMainWaveformXFromTimeStamp,
                timestamp,
                _mainLineCanvas.ActualHeight);

            var overviewLine = CreateSnipLine(
                LineType.OverviewWaveformLine,
                ToOverviewWaveformXFromTimeStamp,
                timestamp,
                _overviewLineCanvas.ActualHeight);

            if (waveformLine == null || overviewLine == null)
            {
                return null;
            }

            _mainLineCanvas.Children.Add(waveformLine);
            _overviewLineCanvas.Children.Add(overviewLine);

            return new LineContainer(timestamp, waveformLine, overviewLine);
        }

        private Line? CreateSnipLine(
            LineType lineType,
            Func<TimeSpan, double> timeToX,
            TimeSpan timestamp,
            double height)
        {
            var x = timeToX(timestamp);

            if (_snipLines.Any(l => Math.Abs(l.GetLine(lineType).X1 - x) < 1))
            {
                return null;
            }

            var line = new Line
            {
                X1 = x,
                X2 = x,
                Y1 = 0,
                Y2 = height,
                Stroke = LineBrush,
                StrokeThickness = LineThickness
            };

            line.MouseDown += (o, _) => SetSelectedLines(_snipLines.Find(l => l.GetLine(lineType) == (Line)o));

            return line;
        }

        private TimeSpan ToTimestampFromOverviewWaveformX(double x)
        {
            if (_audioWaveform is null)
            {
                throw new NullReferenceException("AudioWaveform cannot be null");
            }

            return TimeSpan.FromMilliseconds((x * _length.TotalMilliseconds) / _audioWaveform.OverviewElement.CanvasSize.Width);
        }

        private TimeSpan ToTimestampFromMainWaveformX(double x)
        {
            if (_audioWaveform is null)
            {
                throw new NullReferenceException("AudioWaveform cannot be null");
            }
            
            var timeSpan = _audioWaveform.MainWaveformEndTime - _audioWaveform.MainWaveformStartTime;
            return _audioWaveform.MainWaveformStartTime + 
                   TimeSpan.FromMilliseconds((x * timeSpan.TotalMilliseconds) / _audioWaveform.MainElement.CanvasSize.Width);
        }

        private double ToOverviewWaveformXFromTimeStamp(TimeSpan timestamp)
        {
            if (_audioWaveform is null)
            {
                throw new NullReferenceException("AudioWaveform cannot be null");
            }
            
            return (timestamp.TotalMilliseconds * _audioWaveform.OverviewElement.CanvasSize.Width) / _length.TotalMilliseconds;
        }

        private double ToMainWaveformXFromTimeStamp(TimeSpan timestamp)
        {
            if (_audioWaveform is null)
            {
                throw new NullReferenceException("AudioWaveform cannot be null");
            }
            
            var timeSpan = _audioWaveform.MainWaveformEndTime - _audioWaveform.MainWaveformStartTime;
            var t = timestamp - _audioWaveform.MainWaveformStartTime;
            return (t.TotalMilliseconds * _audioWaveform.MainElement.CanvasSize.Width) / timeSpan.TotalMilliseconds;
        }

        private TimeSpan WidthToTimeSpan(double width)
        {
            if (_audioArray.Length is 0)
            {
                return TimeSpan.Zero;
            }

            return TimeSpan.FromMilliseconds(width * _length.TotalMilliseconds / _audioArray.Length);
        }

        private enum HitType
        {
            None, Body, LeftEdge, RightEdge
        };

        private HitType _mouseHitType = HitType.None;

        private HitType SetHitType(Rectangle rect, Point point)
        {
            var left = Canvas.GetLeft(rect);
            var right = left + rect.Width;
            if (point.X < left) return HitType.None;
            if (point.X > right) return HitType.None;
            
            const double gap = 10;
            if (point.X - left < gap)
            {
                return HitType.LeftEdge;
            }
            
            if (right - point.X < gap)
            {
                return HitType.RightEdge;
            }
            
            return HitType.Body;
        }

        private void SetMouseCursor()
        {
            var cursor = Cursors.Arrow;
            switch (_mouseHitType)
            {
                case HitType.None:
                    cursor = Cursors.Arrow;
                    break;
                case HitType.Body:
                    cursor = Cursors.ScrollWE;
                    break;
                case HitType.LeftEdge:
                case HitType.RightEdge:
                    cursor = Cursors.SizeWE;
                    break;
            }

            if (Cursor != cursor) Cursor = cursor;
        }
    }
}