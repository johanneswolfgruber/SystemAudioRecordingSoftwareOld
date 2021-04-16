﻿using System;
using System.Linq;
using System.Windows;
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
            if (_waveformSlider is null || _audioWaveform is null || _audioArray.Length == 0)
            {
                return;
            }

            _waveformSlider.SetRectangleVisibility(Visibility.Visible);
            SetShouldFollowWaveform(_waveformSlider.ShouldFollowWaveform);

            if (_shouldFollowWaveform)
            {
                _waveformSlider.SnapToRight();
            }

            _audioWaveform.RenderWaveform(
                _audioArray, 
                _length, 
                new RectangleEdges(_waveformSlider.RectangleLeft, _waveformSlider.RectangleRight));

            _snipLines.ForEach(l => l.UpdateMainWaveformLineX(_audioWaveform.MainWaveformTimeToX, 0, ActualWidth));
            _markerLines?.UpdateMainWaveformLineX(_audioWaveform.MainWaveformTimeToX, 0, ActualWidth);
            _snipLines.ForEach(l => l.UpdateOverviewWaveformLineX(_audioWaveform.TimeToX));
            _markerLines?.UpdateOverviewWaveformLineX(_audioWaveform.TimeToX);
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
            SetShouldFollowWaveform(false);
        
            if (_audioWaveform is null)
            {
                throw new NullReferenceException("AudioWaveform cannot be null");
            }
            
            _currentTimestamp = _audioWaveform.XToTime(clickPosition.X);
        }

        private void AddMarker(double x)
        {
            if (_mainLineCanvas is null || _overviewLineCanvas is null || _audioWaveform is null)
            {
                return;
            }

            var timestamp = _audioWaveform.MainWaveformXToTime(x);

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
            if (_mainLineCanvas is null || _overviewLineCanvas is null)
            {
                return;
            }

            if (_selectedLines is null || !(Math.Abs(_selectedLines.MainWaveformLine.X1 - newX) < 10))
            {
                SetSelectedLines(null);
                MoveLine(_markerLines, newX);
                return;
            }

            if (_markerLines is not null)
            {
                _mainLineCanvas.Children.Remove(_markerLines.MainWaveformLine);
                _overviewLineCanvas.Children.Remove(_markerLines.OverviewWaveformLine);
                _markerLines = null;
            }

            MoveLine(_selectedLines, newX);
        }

        private void MoveLine(LineContainer? line, double newX)
        {
            if (line is null || _audioWaveform is null)
            {
                return;
            }

            var timestamp = _audioWaveform.MainWaveformXToTime(newX);

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
            line.UpdateOverviewWaveformLineX(_audioWaveform.TimeToX);
            line.UpdateMainWaveformLineX(_audioWaveform.MainWaveformTimeToX, 0, ActualWidth);

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
            if (_mainLineCanvas is null || _overviewLineCanvas is null || _audioWaveform is null)
            {
                return null;
            }

            var waveformLine = CreateSnipLine(
                LineType.MainWaveformLine,
                _audioWaveform.MainWaveformTimeToX,
                timestamp,
                _mainLineCanvas.ActualHeight);

            var overviewLine = CreateSnipLine(
                LineType.OverviewWaveformLine,
                _audioWaveform.TimeToX,
                timestamp,
                _overviewLineCanvas.ActualHeight);

            if (waveformLine is null || overviewLine is null)
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

        private void SetShouldFollowWaveform(bool shouldFollowWaveform)
        {
            _shouldFollowWaveform = shouldFollowWaveform;
            if (_followPlayHeadButton is not null)
            {
                _followPlayHeadButton.IsChecked = _shouldFollowWaveform;
            }
        }
    }
}