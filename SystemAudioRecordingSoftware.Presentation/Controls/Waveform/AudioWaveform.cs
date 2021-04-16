using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemAudioRecordingSoftware.Domain.Model;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    internal record AudioWaveformStyle(SKColor Color, float StrokeWidth);

    internal readonly struct RectangleEdges
    {
        public RectangleEdges(double leftEdge, double rightEdge)
        {
            LeftEdge = leftEdge;
            RightEdge = rightEdge;
        }

        public double LeftEdge { get; }
        public double RightEdge { get; }
    }

    internal class AudioWaveform
    {
        private AudioDataPoint[] _audioData = Array.Empty<AudioDataPoint>();
        private AudioDataPoint[] _mainWaveformAudioData = Array.Empty<AudioDataPoint>();
        private TimeSpan _length;
        private RectangleEdges _overviewRectangleEdges;
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
            _mainStyle = mainStyle;
            _overviewStyle = overviewStyle;

            MainElement.PaintSurface += OnPaintMainSurface;
            OverviewElement.PaintSurface += OnPaintOverviewSurface;
        }

        public SKElement MainElement { get; }
        public SKElement OverviewElement { get; }

        public void RenderWaveform(AudioDataPoint[] audioData, TimeSpan length, RectangleEdges overviewRectangleEdges)
        {
            _audioData = audioData;
            _length = length;
            _overviewRectangleEdges = overviewRectangleEdges;

            if (_audioData.Length == 0 || _length.TotalMilliseconds == 0)
            {
                return;
            }

            MainElement.InvalidateVisual();
            OverviewElement.InvalidateVisual();
        }

        public TimeSpan MainWaveformXToTime(double x)
        {
            var timeSpan = _mainWaveformEndTime - _mainWaveformStartTime;
            return _mainWaveformStartTime +
                   TimeSpan.FromMilliseconds((x / MainElement.ActualWidth) * timeSpan.TotalMilliseconds);
        }

        public double MainWaveformTimeToX(TimeSpan timestamp)
        {
            var timeSpan = _mainWaveformEndTime - _mainWaveformStartTime;
            var t = timestamp - _mainWaveformStartTime;
            return (t.TotalMilliseconds / timeSpan.TotalMilliseconds) * MainElement.ActualWidth;
        }

        public TimeSpan XToTime(double x)
        {
            return TimeSpan.FromMilliseconds((x / MainElement.ActualWidth) * _length.TotalMilliseconds);
        }

        public double TimeToX(TimeSpan time)
        {
            return (time.TotalMilliseconds / _length.TotalMilliseconds) * MainElement.ActualWidth;
        }

        private void OnPaintMainSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            RenderMainWaveform(canvas);
        }

        private void OnPaintOverviewSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            RenderOverview(canvas);
        }

        private void RenderMainWaveform(SKCanvas canvas)
        {
            _mainWaveformAudioData = GetMainWaveformAudioData();

            if (_mainWaveformAudioData.Length > 0)
            {
                _mainWaveformStartTime = _mainWaveformAudioData[0].TimeStamp;
                _mainWaveformEndTime = _mainWaveformAudioData[^1].TimeStamp;
            }

            if (_mainWaveformAudioData.Length == 0 || _length.TotalSeconds == 0)
            {
                return;
            }

            var dimensions = canvas.DeviceClipBounds;
            var midpoint = dimensions.Height / 2;
            var width = dimensions.Width;

            var audio = GetAudioDataToRender(_mainWaveformAudioData, width);

            var paint = CreatePaint(_mainStyle.Color, _mainStyle.StrokeWidth);
            var upperPath = new SKPath();
            var lowerPath = new SKPath();
            upperPath.MoveTo(new SKPoint(0, midpoint));
            lowerPath.MoveTo(new SKPoint(0, midpoint));

            var loopCount = audio.Count < width ? audio.Count : width;

            for (var i = 0; i < loopCount; i++)
            {
                var amplitudeValue = audio[i].Data * midpoint;

                var upperAmplitudePoint = new SKPoint
                {
                    X = i * ((float)width / audio.Count), Y = midpoint - Math.Abs(amplitudeValue)
                };
                var lowerAmplitudePoint = new SKPoint
                {
                    X = i * ((float)width / audio.Count), Y = midpoint + Math.Abs(amplitudeValue)
                };

                upperPath.LineTo(upperAmplitudePoint);
                lowerPath.LineTo(lowerAmplitudePoint);
            }

            upperPath.LineTo(new SKPoint((audio.Count - 1) * ((float)width / audio.Count), midpoint));
            lowerPath.LineTo(new SKPoint((audio.Count - 1) * ((float)width / audio.Count), midpoint));
            canvas.DrawPath(upperPath, paint);
            canvas.DrawPath(lowerPath, paint);
        }

        private void RenderOverview(SKCanvas canvas)
        {
            if (_audioData.Length <= 0 || _length.TotalSeconds == 0)
            {
                return;
            }

            var dimensions = canvas.DeviceClipBounds;
            var height = dimensions.Height;
            var width = dimensions.Width;

            var audio = GetAudioDataToRender(_audioData, width);

            var paint = CreatePaint(_overviewStyle.Color, _overviewStyle.StrokeWidth);
            var path = new SKPath();
            path.MoveTo(new SKPoint(0, height));

            var loopCount = audio.Count < width ? audio.Count : width;

            for (var i = 0; i < loopCount; i++)
            {
                var amplitudeValue = audio[i].Data * height;

                var amplitudePoint = new SKPoint(i * ((float)width / audio.Count),
                    height - Math.Abs(amplitudeValue));

                path.LineTo(amplitudePoint);
            }

            path.LineTo(new SKPoint((audio.Count - 1) * ((float)width / audio.Count), height));
            canvas.DrawPath(path, paint);
        }

        private AudioDataPoint[] GetMainWaveformAudioData()
        {
            if (_audioData.Length == 0)
            {
                return Array.Empty<AudioDataPoint>();
            }

            var mainLeftTime = XToTime(_overviewRectangleEdges.LeftEdge);
            var mainRightTime = XToTime(_overviewRectangleEdges.RightEdge);
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