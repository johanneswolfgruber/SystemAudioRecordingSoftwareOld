// (c) Johannes Wolfgruber, 2020

using NAudio.Wave;
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
        private readonly List<double> _snipTimestamps = new List<double>();
        private float[]? _audioData;
        private int _zoomFactor = 1000;
        private double _lengthInSeconds;
        private double _currentWidth;
        private double _currentTimestamp;

        public float StrokeWidth { get; set; } = 1f;
        public SKColor Color { get; set; } = SKColors.White;

        public void SetWaveFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            using var reader = new AudioFileReader(filePath);
            float[] buffer = new float[reader.Length / 2];
            reader.Read(buffer, 0, buffer.Length);
            _audioData = new float[buffer.Length / 2];
            Array.Copy(buffer, 0, _audioData, 0, buffer.Length / 2);
            _lengthInSeconds = (double)_audioData.Length / reader.WaveFormat.SampleRate;
        }

        public void SetClickPosition(Point clickPosition, TextBlock textBlock)
        {
            _currentTimestamp = ToTimestamp(clickPosition.X);
            textBlock.Text = $"X: {clickPosition.X}, Y: {clickPosition.Y}, timestamp: {_currentTimestamp}s";
        }

        private double ToTimestamp(double x)
        {
            return (_lengthInSeconds * x) / _currentWidth;
        }

        private double ToX(double timestamp)
        {
            return (_currentWidth * timestamp) / _lengthInSeconds;
        }

        public void ZoomOut()
        {
            _zoomFactor += 100;
        }

        public void ZoomIn()
        {
            if (_zoomFactor != 100)
            {
                _zoomFactor -= 100;
            }
        }

        public void SetWidth(double parentWidth, SKElement element)
        {
            if (_audioData == null)
            {
                return;
            }

            var newWidth = _audioData.Length / _zoomFactor;
            if (newWidth >= parentWidth)
            {
                element.Width = newWidth;
            }

            _currentWidth = element.ActualWidth;
        }

        public void AddSnipPosition()
        {
            _snipTimestamps.Add(_currentTimestamp);
        }

        public void RemoveLastSnipPosition()
        {
            if (_snipTimestamps.Count <= 0)
            {
                return;
            }

            _snipTimestamps.RemoveAt(_snipTimestamps.Count - 1);
        }

        public void DrawOnCanvas(SKCanvas canvas)
        {
            if (_audioData == null)
            {
                return;
            }

            canvas.Clear(SKColors.Transparent);

            var dimensions = canvas.DeviceClipBounds;
            var midpoint = dimensions.Height / 2;
            var width = dimensions.Width;
            var hopSize = (int)Math.Round((double)_audioData.Length / width);

            var paint = CreatePaint(Color);
            var path = new SKPath();
            path.MoveTo(new SKPoint(0, midpoint));

            for (var i = 0; i < width; i++)
            {
                var from = i * hopSize;
                var to = (i + 1) * hopSize;
                var amplitudeValue = to >= _audioData.Length ? 0 : GetAbsoluteMaxValue(_audioData, from, to);

                var multiplier = i % 2 == 0 ? 1 : -1;

                var controlPoint = new SKPoint
                {
                    X = (float)(i - 0.5),
                    Y = midpoint + (amplitudeValue * 100 * multiplier)
                };

                var amplitudePoint = new SKPoint
                {
                    X = i,
                    Y = midpoint
                };

                path.QuadTo(controlPoint, amplitudePoint);
            }

            canvas.DrawPath(path, paint);
            canvas.DrawLine(
                new SKPoint((float)(ToX(_currentTimestamp)), 0), 
                new SKPoint((float)(ToX(_currentTimestamp)), canvas.DeviceClipBounds.Height), 
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

        private static float GetAbsoluteMaxValue(float[] audioData, int from, int to)
        {
            var temp = new float[to - from];
            Array.Copy(audioData, from, temp, 0, to - from);
            return temp.Select(Math.Abs).Max();
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
    }

    public partial class WaveformRenderer
    {
        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(WaveformRenderer), new PropertyMetadata(default));

        private readonly Waveform _waveform = new Waveform();

        public WaveformRenderer()
        {
            InitializeComponent();
        }

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set
            {
                SetValue(FilePathProperty, value);
                OnFilePathChanged(value);
            }
        }

        private void OnFilePathChanged(string filePath)
        {
            _waveform.SetWaveFile(filePath);
            SkiaElement.InvalidateVisual();
        }

        private void SkiaElement_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            _waveform.SetWidth(ActualWidth, SkiaElement);
            _waveform.DrawOnCanvas(e.Surface.Canvas);
        }

        private void SkiaElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _waveform.SetClickPosition(e.GetPosition(SkiaElement), LastClickPosition);
            SkiaElement.InvalidateVisual();
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            _waveform.ZoomOut();
            SkiaElement.InvalidateVisual();
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            _waveform.ZoomIn();
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

        private void SnipButton_Click(object sender, RoutedEventArgs e)
        {
            _waveform.AddSnipPosition();
            SkiaElement.InvalidateVisual();
        }

        private void RemoveSnipButton_Click(object sender, RoutedEventArgs e)
        {
            _waveform.RemoveLastSnipPosition();
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
    }
}
