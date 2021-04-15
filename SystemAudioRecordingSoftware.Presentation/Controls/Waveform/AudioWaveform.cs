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

        public AudioWaveform(
            SKElement mainElement, 
            SKElement overviewElement, 
            AudioWaveformStyle mainStyle, 
            AudioWaveformStyle overviewStyle)
        {
            MainElement = mainElement;
            OverviewElement = overviewElement;
            MainStyle = mainStyle;
            OverviewStyle = overviewStyle;
            
            MainElement.PaintSurface += MainElementOnPaintSurface;
            OverviewElement.PaintSurface += OverviewElementOnPaintSurface;
        }

        public SKElement MainElement { get; }
        public SKElement OverviewElement { get; }
        public AudioWaveformStyle MainStyle { get; }
        public AudioWaveformStyle OverviewStyle { get; }

        public TimeSpan MainWaveformEndTime { get; private set; }

        public TimeSpan MainWaveformStartTime { get; private set; }

        public void RenderWaveform(AudioDataPoint[] audioData, TimeSpan length, RectangleEdges overviewRectangleEdges)
        {
            _audioData = audioData;
            _length = length;
            _overviewRectangleEdges = overviewRectangleEdges;

            OverviewElement.InvalidateVisual();
            MainElement.InvalidateVisual();
        }

        private void MainElementOnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            
            if (_mainWaveformAudioData.Length <= 0 || _length.TotalSeconds == 0)
            {
                return;
            }

            canvas.Clear(SKColors.Transparent);

            var dimensions = canvas.DeviceClipBounds;
            var midpoint = dimensions.Height / 2;
            var width = dimensions.Width;

            var audio = GetAudioDataToRender(_mainWaveformAudioData, width);

            var paint = CreatePaint(MainStyle.Color, MainStyle.StrokeWidth);
            var upperPath = new SKPath();
            var lowerPath = new SKPath();
            upperPath.MoveTo(new SKPoint(0, midpoint));
            lowerPath.MoveTo(new SKPoint(0, midpoint));

            for (var i = 0; i < audio.Count; i++)
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
            
            upperPath.LineTo(new SKPoint((audio.Count - 1) *  ((float)width / audio.Count), midpoint));
            lowerPath.LineTo(new SKPoint((audio.Count - 1) *  ((float)width / audio.Count), midpoint));
            canvas.DrawPath(upperPath, paint);
            canvas.DrawPath(lowerPath, paint);
        }

        private void OverviewElementOnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            
            if (_audioData.Length <= 0 || _length.TotalSeconds == 0)
            {
                return;
            }

            canvas.Clear(SKColors.Transparent);

            var dimensions = canvas.DeviceClipBounds;
            var width = dimensions.Width;

            var audio = GetAudioDataToRender(_audioData, width);

            var paint = CreatePaint(OverviewStyle.Color, OverviewStyle.StrokeWidth);
            var path = new SKPath();
            path.MoveTo(new SKPoint(0, dimensions.Height));

            var drawnPoints = new List<(float, AudioDataPoint)>(audio.Count);

            for (var i = 0; i < audio.Count; i++)
            {
                var amplitudeValue = audio[i].Data * dimensions.Height;

                var amplitudePoint = new SKPoint(i * ((float)width / audio.Count),
                    dimensions.Height - Math.Abs(amplitudeValue));

                drawnPoints.Add((amplitudePoint.X, audio[i]));

                path.LineTo(amplitudePoint);
            }

            path.LineTo(new SKPoint((audio.Count - 1) *  ((float)width / audio.Count), dimensions.Height));
            canvas.DrawPath(path, paint);

            // get points within rectangle
            var points = drawnPoints
                .Where(p => p.Item1 >= _overviewRectangleEdges.LeftEdge
                            && p.Item1 <= _overviewRectangleEdges.RightEdge)
                .ToList();

            _mainWaveformAudioData = points.Count == 0
                ? Array.Empty<AudioDataPoint>()
                : _audioData.Where(p =>
                    p.TimeStamp >= points[0].Item2.TimeStamp && p.TimeStamp <= points.Last().Item2.TimeStamp).ToArray();

            if (_mainWaveformAudioData.Length > 0)
            {
                MainWaveformStartTime = _mainWaveformAudioData[0].TimeStamp;
                MainWaveformEndTime = _mainWaveformAudioData[^1].TimeStamp;
            }
        }

        private static List<AudioDataPoint> GetAudioDataToRender(AudioDataPoint[] audioDataPoints, int width)
        {
            var audio = new List<AudioDataPoint>(width);

            if (audioDataPoints.Length > width)
            {
                var blockSize = (int) Math.Ceiling((double) audioDataPoints.Length / width);

                for (int i = 0; i < width; i++)
                {
                    if ((i + 1) * blockSize >= audioDataPoints.Length)
                    {
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