using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SystemAudioRecordingSoftware.Domain.Model;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    internal record AudioWaveformStyle(SKColor Color, float StrokeWidth);

    internal class AudioWaveform
    {
        private AudioDataPoint[] _audioData = Array.Empty<AudioDataPoint>();
        private AudioDataPoint[] _mainWaveformAudioData = Array.Empty<AudioDataPoint>();
        private TimeSpan _length;
        private TimeSpan _mainWaveformEndTime;
        private TimeSpan _mainWaveformStartTime;
        private readonly AudioWaveformStyle _mainStyle;
        private readonly AudioWaveformStyle _overviewStyle;

        public AudioWaveform(
            SKElement mainElement,
            SKElement overviewElement,
            AudioWaveformStyle mainStyle,
            AudioWaveformStyle overviewStyle)
        {
            MainElement = mainElement;
            OverviewElement = overviewElement;
            WaveformSlider = new WaveformSlider(OverviewElement, XToTime);
            MainLineDisplay = new LineDisplay(MainElement, MainWaveformXToTime, MainWaveformTimeToX);
            OverviewLineDisplay = new ReadonlyLineDisplay(TimeToX);
            _mainStyle = mainStyle;
            _overviewStyle = overviewStyle;

            MainElement.PaintSurface += OnPaintMainSurface;
            OverviewElement.PaintSurface += OnPaintOverviewSurface;
        }

        public SKElement MainElement { get; }
        public SKElement OverviewElement { get; }
        public WaveformSlider WaveformSlider { get; }
        public ILineDisplay MainLineDisplay { get; }
        public ILineDisplay OverviewLineDisplay { get; }

        public bool ShouldFollowWaveform
        {
            get => WaveformSlider.ShouldFollowWaveform;
            set => WaveformSlider.ShouldFollowWaveform = value;
        }

        public TimeSpan SelectedTimeStamp => WaveformSlider.SelectedTimeStamp;

        public void RenderWaveform(AudioDataPoint[] audioData, TimeSpan length)
        {
            _audioData = audioData;
            _length = length;

            if (_audioData.Length == 0 || _length.TotalMilliseconds == 0)
            {
                return;
            }

            WaveformSlider.SetRectangleVisibility(Visibility.Visible);
            MainElement.InvalidateVisual();
            OverviewElement.InvalidateVisual();
        }

        private TimeSpan MainWaveformXToTime(double x)
        {
            var timeSpan = _mainWaveformEndTime - _mainWaveformStartTime;
            return _mainWaveformStartTime +
                   TimeSpan.FromMilliseconds((x / MainElement.ActualWidth) * timeSpan.TotalMilliseconds);
        }

        private double MainWaveformTimeToX(TimeSpan timestamp)
        {
            var timeSpan = _mainWaveformEndTime - _mainWaveformStartTime;
            var t = timestamp - _mainWaveformStartTime;
            return (t.TotalMilliseconds / timeSpan.TotalMilliseconds) * MainElement.ActualWidth;
        }

        private TimeSpan XToTime(double x) => 
            TimeSpan.FromMilliseconds((x / MainElement.ActualWidth) * _length.TotalMilliseconds);

        private double TimeToX(TimeSpan time) => 
            (time.TotalMilliseconds / _length.TotalMilliseconds) * MainElement.ActualWidth;

        private void OnPaintMainSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            RenderMainWaveform(canvas);
            MainLineDisplay.Render(canvas);
        }

        private void OnPaintOverviewSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            RenderOverview(canvas);
            WaveformSlider.Render(canvas);
            OverviewLineDisplay.SetLines(MainLineDisplay.Marker, MainLineDisplay.SnipLines.ToList());
            OverviewLineDisplay.Render(canvas);
        }

        private void RenderMainWaveform(SKCanvas canvas)
        {
            _mainWaveformAudioData = GetMainWaveformAudioData();
            SetMainWaveformStartAndEndTime();

            if (_mainWaveformAudioData.Length == 0 || _length.TotalSeconds == 0)
            {
                return;
            }

            DrawWaveform(
                canvas, 
                GetAudioDataToRender(_mainWaveformAudioData, canvas.DeviceClipBounds.Width), 
                CreatePaint(_mainStyle.Color, _mainStyle.StrokeWidth));
        }

        private void SetMainWaveformStartAndEndTime()
        {
            if (_mainWaveformAudioData.Length == 0)
            {
                return;
            }

            _mainWaveformStartTime = _mainWaveformAudioData[0].TimeStamp;
            _mainWaveformEndTime = _mainWaveformAudioData[^1].TimeStamp;
        }

        private void RenderOverview(SKCanvas canvas)
        {
            if (_audioData.Length <= 0 || _length.TotalSeconds == 0)
            {
                return;
            }

            DrawWaveform(
                canvas, 
                GetAudioDataToRender(_audioData, canvas.DeviceClipBounds.Width), 
                CreatePaint(_overviewStyle.Color, _overviewStyle.StrokeWidth));
        }

        private static void DrawWaveform(
            SKCanvas canvas, 
            IReadOnlyList<AudioDataPoint> audio,
            SKPaint? paint)
        {
            var dimensions = canvas.DeviceClipBounds;
            var midpoint = dimensions.Height / 2;
            var width = dimensions.Width;
            
            var upperPath = new SKPath();
            var lowerPath = new SKPath();
            var startPoint = new SKPoint(0, midpoint);
            upperPath.MoveTo(startPoint);
            lowerPath.MoveTo(startPoint);

            for (var i = 0; i < audio.Count; i++)
            {
                var amplitudeValue = audio[i].Data * midpoint;
                var x = i * ((float)width / audio.Count);
                var upperY = midpoint - Math.Abs(amplitudeValue);
                var lowerY = midpoint + Math.Abs(amplitudeValue);

                var upperAmplitudePoint = new SKPoint { X = x, Y = upperY };
                var lowerAmplitudePoint = new SKPoint { X = x, Y = lowerY };

                upperPath.LineTo(upperAmplitudePoint);
                lowerPath.LineTo(lowerAmplitudePoint);
            }

            var endPoint = new SKPoint((audio.Count - 1) * ((float)width / audio.Count), midpoint);
            upperPath.LineTo(endPoint);
            lowerPath.LineTo(endPoint);
            canvas.DrawPath(upperPath, paint);
            canvas.DrawPath(lowerPath, paint);
        }

        private AudioDataPoint[] GetMainWaveformAudioData()
        {
            if (_audioData.Length == 0)
            {
                return Array.Empty<AudioDataPoint>();
            }

            var mainLeftTime = XToTime(WaveformSlider.RectangleLeft);
            var mainRightTime = XToTime(WaveformSlider.RectangleRight);
            return _audioData
                .Where(p => p.TimeStamp >= mainLeftTime &&
                                        p.TimeStamp <= mainRightTime)
                .ToArray();
        }

        private static List<AudioDataPoint> GetAudioDataToRender(AudioDataPoint[] audioDataPoints, int width)
        {
            var audio = new List<AudioDataPoint>(width);

            if (audioDataPoints.Length > width)
            {
                var blockSize = (int)Math.Ceiling((double)audioDataPoints.Length / width);

                for (int i = 0; i < width; i++)
                {
                    if ((i + 1) * blockSize >= audioDataPoints.Length)
                    {
                        audio.Add(GetAbsoluteMaxValue(audioDataPoints, i * blockSize, audioDataPoints.Length - 1));
                        break;
                    }

                    audio.Add(GetAbsoluteMaxValue(audioDataPoints, i * blockSize, (i + 1) * blockSize));
                }
            }
            else
            {
                audio.AddRange(audioDataPoints);
            }

            return audio;
        }

        private static AudioDataPoint GetAbsoluteMaxValue(AudioDataPoint[] audioData, int from, int to)
        {
            var slice = new Span<AudioDataPoint>(audioData, @from, to - @from);
            AudioDataPoint max = new(TimeSpan.Zero, 0f);
            foreach (var point in slice)
            {
                var abs = Math.Abs(point.Data);
                if (abs > max.Data)
                {
                    max = new AudioDataPoint(point.TimeStamp, abs);
                }
            }

            return max;
        }

        private static SKPaint CreatePaint(SKColor color, float strokeWidth)
            => new()
            {
                IsAntialias = true,
                Color = color,
                StrokeCap = SKStrokeCap.Round,
                Style = SKPaintStyle.Fill,
                StrokeWidth = strokeWidth
            };
    }
}