using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SystemAudioRecordingSoftware.Domain.Model;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    internal record AudioWaveformStyle(SKColor Color, float StrokeWidth);

    internal class AudioWaveform
    {
        private readonly SKElement _mainElement;
        private readonly SKElement _overviewElement;
        private readonly WaveformSlider _waveformSlider;
        private readonly ILineDisplay _mainLineDisplay;
        private readonly ILineDisplay _overviewLineDisplay;
        private readonly AudioWaveformStyle _mainStyle;
        private readonly AudioWaveformStyle _overviewStyle;
        private AudioDataPoint[] _audioData = Array.Empty<AudioDataPoint>();
        private AudioDataPoint[] _mainWaveformAudioData = Array.Empty<AudioDataPoint>();
        private TimeSpan _length;
        private TimeSpan _mainWaveformStartTime;
        private TimeSpan _mainWaveformEndTime;
        private double _lastScrollX;
        private TimeSpan _lastScrollTime;

        public AudioWaveform(
            SKElement mainElement,
            SKElement overviewElement,
            AudioWaveformStyle mainStyle,
            AudioWaveformStyle overviewStyle)
        {
            _mainElement = mainElement;
            _overviewElement = overviewElement;
            _waveformSlider = new WaveformSlider(_overviewElement, XToTime, TimeToX);
            _mainLineDisplay = new LineDisplay(_mainElement, MainWaveformXToTime, MainWaveformTimeToX);
            _overviewLineDisplay = new ReadonlyLineDisplay(TimeToX);
            _mainStyle = mainStyle;
            _overviewStyle = overviewStyle;

            _mainElement.PaintSurface += OnPaintMainSurface;
            _overviewElement.PaintSurface += OnPaintOverviewSurface;
            _mainElement.MouseWheel += OnMainSurfaceScroll;
        }
        
        public event EventHandler<EventArgs> SnipLinesChanged
        {
            add { _mainLineDisplay.SnipLinesChanged += value; }
            remove { _mainLineDisplay.SnipLinesChanged -= value; }
        }

        public bool ShouldFollowWaveform
        {
            get => _waveformSlider.ShouldFollowWaveform;
            set => _waveformSlider.ShouldFollowWaveform = value;
        }

        public IReadOnlyList<MarkerLine> Snips => _mainLineDisplay.Snips;

        public TimeSpan SelectedTimeStamp => _waveformSlider.SelectedTimeStamp;

        public void RenderWaveform(AudioDataPoint[] audioData, TimeSpan length)
        {
            _audioData = audioData;
            _length = length;

            if (_audioData.Length == 0 || _length.TotalMilliseconds == 0)
            {
                return;
            }

            _waveformSlider.SetRectangleVisibility(Visibility.Visible);
            _mainElement.InvalidateVisual();
            _overviewElement.InvalidateVisual();
        }
        
        public void ZoomIn(TimeSpan? zoomAround = null)
        {
            _waveformSlider.ZoomIn(zoomAround);
        }

        public void ZoomOut(TimeSpan? zoomAround = null)
        {
            _waveformSlider.ZoomOut(zoomAround);
        }
        
        public TimeSpan? AddSnipLine(TimeSpan? timeStamp = null)
        {
            return _mainLineDisplay.AddSnipLine(timeStamp);
        }

        public TimeSpan? RemoveSnipLine(TimeSpan? timeStamp = null)
        {
            return _mainLineDisplay.RemoveSnipLine(timeStamp);
        }

        public void Reset()
        {
            _mainLineDisplay.Reset();
            _overviewLineDisplay.Reset();
            _waveformSlider.Reset();
            _audioData = Array.Empty<AudioDataPoint>();
            _mainWaveformAudioData = Array.Empty<AudioDataPoint>();
            _length = TimeSpan.Zero;
            _mainWaveformStartTime = TimeSpan.Zero;
            _mainWaveformEndTime = TimeSpan.Zero;
            _lastScrollX = 0;
            _lastScrollTime = TimeSpan.Zero;
        }

        private TimeSpan MainWaveformXToTime(double x)
        {
            var timeSpan = _mainWaveformEndTime - _mainWaveformStartTime;
            return _mainWaveformStartTime +
                   TimeSpan.FromMilliseconds((x / _mainElement.ActualWidth) * timeSpan.TotalMilliseconds);
        }

        private double MainWaveformTimeToX(TimeSpan timestamp)
        {
            var timeSpan = _mainWaveformEndTime - _mainWaveformStartTime;
            var t = timestamp - _mainWaveformStartTime;
            return (t.TotalMilliseconds / timeSpan.TotalMilliseconds) * _mainElement.ActualWidth;
        }

        private TimeSpan XToTime(double x) => 
            TimeSpan.FromMilliseconds((x / _mainElement.ActualWidth) * _length.TotalMilliseconds);

        private double TimeToX(TimeSpan time) => 
            (time.TotalMilliseconds / _length.TotalMilliseconds) * _mainElement.ActualWidth;

        private void OnPaintMainSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            RenderMainWaveform(canvas);
            _mainLineDisplay.Render(canvas);
        }

        private void OnPaintOverviewSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            RenderOverview(canvas);
            _waveformSlider.Render(canvas);
            _overviewLineDisplay.SetLines(_mainLineDisplay.Marker, _mainLineDisplay.Snips.ToList());
            _overviewLineDisplay.Render(canvas);
        }

        private void OnMainSurfaceScroll(object sender, MouseWheelEventArgs args)
        {
            var x = Mouse.GetPosition(_mainElement).X;
            if (x < _lastScrollX - 10 || x > _lastScrollX + 10)
            {
                _lastScrollX = x;
                _lastScrollTime = MainWaveformXToTime(_lastScrollX);
            }
            
            if (args.Delta > 0) ZoomIn(_lastScrollTime);
            if (args.Delta < 0) ZoomOut(_lastScrollTime);
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

            var mainLeftTime = XToTime(_waveformSlider.RectangleLeft);
            var mainRightTime = XToTime(_waveformSlider.RectangleRight);
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