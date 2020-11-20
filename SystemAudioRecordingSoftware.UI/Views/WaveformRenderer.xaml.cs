// (c) Johannes Wolfgruber, 2020

using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SystemAudioRecordingSoftware.UI.Views
{
    public class Waveform
    {
        private const double MaxWidth = 2500;
        private readonly List<float> _audioData = new();
        private readonly List<TimeSpan> _snipTimestamps = new();
        private double _currentAudioWidth;
        private TimeSpan _currentTimestamp;
        private TimeSpan _lengthInSeconds;
        private int _numberOfSamples;
        private int _zoomFactor = 10;

        public float StrokeWidth { get; set; } = .5f;
        public SKColor Color { get; set; } = SKColors.White;

        public void AddAudioData(IEnumerable<float> buffer, int totalNumberOfSamples, int sampleRate)
        {
            _audioData.AddRange(buffer);
            _numberOfSamples += totalNumberOfSamples;
            _lengthInSeconds = TimeSpan.FromSeconds((double)_numberOfSamples / sampleRate);
        }

        public void AddLiveSnipPosition()
        {
            _snipTimestamps.Add(_lengthInSeconds);
        }

        public void AddSnipPosition()
        {
            _snipTimestamps.Add(_currentTimestamp);
        }

        public void DrawOnCanvas(SKCanvas canvas)
        {
            if (_audioData.Count <= 0)
            {
                return;
            }

            canvas.Clear(SKColors.Transparent);

            var dimensions = canvas.DeviceClipBounds;
            var midpoint = dimensions.Height / 2;
            var width = dimensions.Width;
            var hopSize = _zoomFactor;

            var paint = CreatePaint(Color);

            for (var i = 0; i < width; i++)
            {
                var amplitudeValue = (i + 1) * hopSize >= _audioData.Count
                    ? 0
                    : GetAbsoluteMaxValue(_audioData.ToArray(), i * hopSize, (i + 1) * hopSize) * midpoint;
                canvas.DrawLine(
                    new SKPoint(i, midpoint + Math.Abs(amplitudeValue)),
                    new SKPoint(i, midpoint - Math.Abs(amplitudeValue)),
                    paint);
            }

            canvas.DrawLine(
                new SKPoint((float)(ToX(_currentTimestamp)), 0),
                new SKPoint((float)(ToX(_currentTimestamp)), canvas.DeviceClipBounds.Height),
                paint);

            canvas.DrawLine(
                new SKPoint((float)(ToX(_lengthInSeconds)), 0),
                new SKPoint((float)(ToX(_lengthInSeconds)), canvas.DeviceClipBounds.Height),
                paint);

            var redPaint = CreatePaint(SKColors.Red);
            foreach (var timestamp in _snipTimestamps)
            {
                canvas.DrawLine(
                    new SKPoint((float)(ToX(timestamp)), 0),
                    new SKPoint((float)(ToX(timestamp)), canvas.DeviceClipBounds.Height),
                    redPaint);
            }
        }

        public void RemoveLastSnipPosition()
        {
            if (_snipTimestamps.Count <= 0)
            {
                return;
            }

            _snipTimestamps.RemoveAt(_snipTimestamps.Count - 1);
        }

        public void Reset()
        {
            _audioData.Clear();
            _numberOfSamples = 0;
            _lengthInSeconds = TimeSpan.Zero;
            _currentTimestamp = TimeSpan.Zero;
            _snipTimestamps.Clear();
            _zoomFactor = 10;
        }

        public void SetClickPosition(Point clickPosition, TextBlock textBlock)
        {
            _currentTimestamp = ToTimestamp(clickPosition.X);
            textBlock.Text =
                $"X: {clickPosition.X}, Y: {clickPosition.Y}, timestamp: {_currentTimestamp.TotalSeconds}s";
        }

        public void SetWidth(double parentWidth, SKElement element)
        {
            if (_audioData.Count <= 0)
            {
                return;
            }

            _currentAudioWidth = (double)_audioData.Count / _zoomFactor;
            if (_currentAudioWidth >= parentWidth && _currentAudioWidth <= MaxWidth)
            {
                element.Width = _currentAudioWidth;
            }

            if (_currentAudioWidth >= MaxWidth)
            {
                _zoomFactor++;
                SetWidth(parentWidth, element);
            }
        }

        public void ZoomIn()
        {
            if (_zoomFactor != 2)
            {
                _zoomFactor -= 1;
            }
        }

        public void ZoomOut()
        {
            _zoomFactor += 1;
        }

        private SKPaint CreatePaint(SKColor color)
            => new SKPaint
            {
                IsAntialias = true,
                Color = color,
                StrokeCap = SKStrokeCap.Round,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = StrokeWidth
            };

        private static float GetAbsoluteMaxValue(float[] audioData, int from, int to)
        {
            var temp = new float[to - from];
            Array.Copy(audioData, from, temp, 0, to - from);
            return temp.Select(Math.Abs).Max();
        }

        private TimeSpan ToTimestamp(double x)
        {
            return TimeSpan.FromSeconds((_lengthInSeconds.TotalSeconds * x) / ((double)_audioData.Count / _zoomFactor));
        }

        private double ToX(TimeSpan timestamp)
        {
            return (((double)_audioData.Count / _zoomFactor) * timestamp.TotalSeconds) / _lengthInSeconds.TotalSeconds;
        }
    }

    public partial class WaveformRenderer
    {
        private readonly Waveform _waveform = new Waveform();
        private bool _autoScroll = true;

        public WaveformRenderer()
        {
            InitializeComponent();
        }

        public void AddAudioData(IEnumerable<float> buffer, int totalNumberOfSamples, int sampleRate)
        {
            _waveform.AddAudioData(buffer, totalNumberOfSamples, sampleRate);
            SkiaElement.InvalidateVisual();
        }

        public void AddLiveSnip()
        {
            _waveform.AddLiveSnipPosition();
            SkiaElement.InvalidateVisual();
        }

        public void Reset()
        {
            SkiaElement.Width = ActualWidth;
            _waveform.Reset();
            SkiaElement.InvalidateVisual();
        }

        private void RemoveSnipButton_Click(object sender, RoutedEventArgs e)
        {
            _waveform.RemoveLastSnipPosition();
            SkiaElement.InvalidateVisual();
        }

        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!(sender is ScrollViewer scrollViewer))
            {
                return;
            }

            if (e.ExtentWidthChange == 0)
            {
                _autoScroll = scrollViewer.HorizontalOffset == scrollViewer.ScrollableWidth;
            }

            if (_autoScroll && e.ExtentWidthChange != 0)
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.ExtentWidth);
            }
        }

        private void SkiaElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _waveform.SetClickPosition(e.GetPosition(SkiaElement), LastClickPosition);
            SkiaElement.InvalidateVisual();
        }

        private void SkiaElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            _waveform.SetClickPosition(e.GetPosition(SkiaElement), LastClickPosition);
            SkiaElement.InvalidateVisual();
        }

        private void SkiaElement_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                _waveform.ZoomIn();
                SkiaElement.InvalidateVisual();
            }
            else if (e.Delta < 0)
            {
                _waveform.ZoomOut();
                SkiaElement.InvalidateVisual();
            }
        }

        private void SkiaElement_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            _waveform.SetWidth(ActualWidth, SkiaElement);
            _waveform.DrawOnCanvas(e.Surface.Canvas);
        }

        private void SnipButton_Click(object sender, RoutedEventArgs e)
        {
            _waveform.AddSnipPosition();
            SkiaElement.InvalidateVisual();
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            _waveform.ZoomIn();
            SkiaElement.InvalidateVisual();
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            _waveform.ZoomOut();
            SkiaElement.InvalidateVisual();
        }
    }
}