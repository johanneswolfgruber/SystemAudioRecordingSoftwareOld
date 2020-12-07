// (c) Johannes Wolfgruber, 2020

using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SystemAudioRecordingSoftware.Common.UI.Controls
{
    [TemplatePart(Name = ContentPart, Type = typeof(Grid))]
    [TemplatePart(Name = MainWaveformPart, Type = typeof(SKElement))]
    [TemplatePart(Name = MainLineCanvasPart, Type = typeof(Canvas))]
    [TemplatePart(Name = OverviewWaveformPart, Type = typeof(SKElement))]
    [TemplatePart(Name = OverviewLineCanvasPart, Type = typeof(Canvas))]
    [TemplatePart(Name = ButtonsPart, Type = typeof(StackPanel))]
    [TemplatePart(Name = ZoomInPart, Type = typeof(Button))]
    [TemplatePart(Name = ZoomOutPart, Type = typeof(Button))]
    [TemplatePart(Name = AddSnipPart, Type = typeof(Button))]
    [TemplatePart(Name = RemoveSnipPart, Type = typeof(Button))]
    [TemplatePart(Name = FollowPlayHeadPart, Type = typeof(Button))]
    [TemplatePart(Name = LastClickPositionPart, Type = typeof(TextBlock))]
    public class WaveformControl : Control
    {
        public const string ContentPart = "PART_Content";
        public const string MainWaveformPart = "PART_MainWaveform";
        public const string MainLineCanvasPart = "PART_MainLineCanvas";
        public const string OverviewWaveformPart = "PART_OverviewWaveform";
        public const string OverviewLineCanvasPart = "PART_OverviewLineCanvas";
        public const string ButtonsPart = "PART_Buttons";
        public const string ZoomInPart = "PART_ZoomInButton";
        public const string ZoomOutPart = "PART_ZoomOutButton";
        public const string AddSnipPart = "PART_AddSnipButton";
        public const string RemoveSnipPart = "PART_RemoveSnipButton";
        public const string FollowPlayHeadPart = "PART_FollowPlayHeadButton";
        public const string LastClickPositionPart = "PART_LastClickPosition";

        private readonly List<LineContainer> _snipLines = new();

        // private int _zoomFactor = 10; // TODO: implement zooming
        private Button? _addSnipButton;
        private float[] _audioArray = Array.Empty<float>();
        private TimeSpan _currentTimestamp;
        private ToggleButton? _followPlayHeadButton;
        private TextBlock? _lastClickPositionTextBlock;
        private TimeSpan _lengthInSeconds;
        private Canvas? _mainLineCanvas;
        private SKElement? _mainWaveform;
        private LineContainer? _markerLines;
        private double _overviewAudioWidth;
        private Canvas? _overviewLineCanvas;
        private SKElement? _overviewWaveform;
        private Button? _removeSnipButton;
        private LineContainer? _selectedLines;
        private bool _shouldFollowWaveform = true;
        private double _waveformAudioWidth;
        private Button? _zoomInButton;
        private Button? _zoomOutButton;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _mainWaveform = GetTemplateChild(MainWaveformPart) as SKElement;
            _mainLineCanvas = GetTemplateChild(MainLineCanvasPart) as Canvas;
            _overviewWaveform = GetTemplateChild(OverviewWaveformPart) as SKElement;
            _overviewLineCanvas = GetTemplateChild(OverviewLineCanvasPart) as Canvas;

            _zoomInButton = GetTemplateChild(ZoomInPart) as Button;
            _zoomOutButton = GetTemplateChild(ZoomOutPart) as Button;
            _addSnipButton = GetTemplateChild(AddSnipPart) as Button;
            _removeSnipButton = GetTemplateChild(RemoveSnipPart) as Button;
            _followPlayHeadButton = GetTemplateChild(FollowPlayHeadPart) as ToggleButton;
            _lastClickPositionTextBlock = GetTemplateChild(LastClickPositionPart) as TextBlock;

            if (_mainWaveform == null || _mainLineCanvas == null || _overviewWaveform == null ||
                _overviewLineCanvas == null || _zoomInButton == null || _zoomOutButton == null ||
                _addSnipButton == null || _removeSnipButton == null || _followPlayHeadButton == null ||
                _lastClickPositionTextBlock == null)
            {
                throw new InvalidOperationException("Missing template part");
            }

            _mainWaveform.PaintSurface += OnMainWaveformPaintSurface;
            _mainWaveform.MouseDown += OnMainLineCanvasMouseDown;
            _mainWaveform.MouseMove += OnMainLineCanvasMouseMove;

            _overviewWaveform.PaintSurface += OnOverviewWaveformPaintSurface;
            _overviewWaveform.MouseDown += OnOverviewLineCanvasMouseDown;
            _overviewWaveform.MouseMove += OnOverviewLineCanvasMouseMove;

            _zoomInButton.Click += OnZoomInClicked;
            _zoomOutButton.Click += OnZoomOutClicked;
            _addSnipButton.Click += OnAddSnipClicked;
            _removeSnipButton.Click += OnRemoveSnipClicked;
            _followPlayHeadButton.Click += OnFollowPlayHeadClicked;

            _followPlayHeadButton.IsChecked = _shouldFollowWaveform;
        }

        #region Handlers

        private void OnMainWaveformPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            if (_audioArray.Length <= 0 || _lengthInSeconds.TotalSeconds == 0)
            {
                return;
            }

            canvas.Clear(SKColors.Transparent);

            var dimensions = canvas.DeviceClipBounds;
            var midpoint = dimensions.Height / 2;
            var width = dimensions.Width;

            var start = _shouldFollowWaveform
                ? _audioArray.Length < width ? 0 : _audioArray.Length - width
                : (int)Math.Floor(GetMainWaveformStartIndex());

            _waveformAudioWidth = width;

            var dataToRender = _audioArray.Length < start + width
                ? _audioArray
                : new Span<float>(_audioArray, start, width);

            var paint = CreatePaint(MainWaveformColor, MainWaveformStrokeWidth);

            for (var i = 0; i < width; i++)
            {
                if (i >= dataToRender.Length)
                {
                    _waveformAudioWidth = i;
                    break;
                }

                var amplitudeValue = i >= dataToRender.Length ? 0 : dataToRender[i] * midpoint;

                canvas.DrawLine(
                    new SKPoint(i, midpoint + Math.Abs(amplitudeValue)),
                    new SKPoint(i, midpoint - Math.Abs(amplitudeValue)),
                    paint);
            }

            _snipLines.ForEach(l => l.UpdateMainWaveformLineX(ToMainWaveformXFromTimeStamp, 0, ActualWidth));
            _markerLines?.UpdateMainWaveformLineX(ToMainWaveformXFromTimeStamp, 0, ActualWidth);
        }

        private void OnOverviewWaveformPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            if (_audioArray.Length <= 0 || _lengthInSeconds.TotalSeconds == 0)
            {
                return;
            }

            canvas.Clear(SKColors.Transparent);

            var dimensions = canvas.DeviceClipBounds;
            var midpoint = dimensions.Height / 2;
            var width = dimensions.Width;
            var blockSize = (int)Math.Ceiling((double)_audioArray.Length / width);

            var paint = CreatePaint(OverviewWaveformColor, OverviewWaveformStrokeWidth);
            _overviewAudioWidth = width;

            for (var i = 0; i < width; i++)
            {
                if ((i + 1) * blockSize >= _audioArray.Length)
                {
                    _overviewAudioWidth = i;
                    break;
                }

                var amplitudeValue = GetAbsoluteMaxValue(_audioArray, i * blockSize, (i + 1) * blockSize) * midpoint;
                canvas.DrawLine(
                    new SKPoint(i, midpoint + Math.Abs(amplitudeValue)),
                    new SKPoint(i, midpoint - Math.Abs(amplitudeValue)),
                    paint);
            }

            var x = _shouldFollowWaveform
                ? (float)ToOverviewWaveformXFromTimeStamp(_lengthInSeconds) -
                  (float)ToOverviewWaveformXFromTimeStamp(WidthToTimeSpan(_waveformAudioWidth))
                : (float)ToOverviewWaveformXFromTimeStamp(_currentTimestamp);

            var fill = new SKPaint {Style = SKPaintStyle.Fill, Color = SKColors.White.WithAlpha(90)};
            canvas.DrawRect(SKRect.Create(
                    new SKPoint(x, 0),
                    new SKSize((float)ToOverviewWaveformXFromTimeStamp(WidthToTimeSpan(_waveformAudioWidth)),
                        dimensions.Height)),
                fill);

            _snipLines.ForEach(l => l.UpdateOverviewWaveformLineX(ToOverviewWaveformXFromTimeStamp));
            _markerLines?.UpdateOverviewWaveformLineX(ToOverviewWaveformXFromTimeStamp);
        }

        private void OnMainLineCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_mainWaveform == null)
            {
                return;
            }

            AddMarker(e.GetPosition(_mainWaveform).X);
        }

        private void OnMainLineCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_mainWaveform == null || Mouse.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            MoveLine(e.GetPosition(_mainWaveform).X);
        }

        private void OnOverviewLineCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_overviewWaveform == null)
            {
                return;
            }

            SetOverviewWaveformClickPosition(e.GetPosition(_overviewWaveform));
            InvalidateSkiaVisuals();
        }

        private void OnOverviewLineCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_overviewWaveform == null || Mouse.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            SetOverviewWaveformClickPosition(e.GetPosition(_overviewWaveform));
            InvalidateSkiaVisuals();
        }

        private void OnFollowPlayHeadClicked(object sender, RoutedEventArgs e)
        {
            _shouldFollowWaveform = _followPlayHeadButton?.IsChecked ?? true;
        }

        private void OnRemoveSnipClicked(object sender, RoutedEventArgs e)
        {
            if (_snipLines.Count <= 0 || _selectedLines == null || _mainLineCanvas == null ||
                _overviewLineCanvas == null)
            {
                return;
            }

            _snipLines.Remove(_selectedLines);

            _mainLineCanvas.Children.Remove(_selectedLines.MainWaveformLine);
            _overviewLineCanvas.Children.Remove(_selectedLines.OverviewWaveformLine);

            if (_snipLines.Count > 0)
            {
                SetSelectedLines(_snipLines.Last());
            }
            else
            {
                _selectedLines = null;
            }
        }

        private void OnAddSnipClicked(object sender, RoutedEventArgs e)
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
            }
        }

        private void OnZoomOutClicked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnZoomInClicked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void OnDisplayAudioDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is WaveformControl control))
            {
                return;
            }

            control._audioArray = (float[])e.NewValue;
            control.InvalidateSkiaVisuals();
        }

        private static void OnLengthInSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is WaveformControl control))
            {
                return;
            }

            control._lengthInSeconds = (TimeSpan)e.NewValue;
            control.InvalidateSkiaVisuals();
        }

        #endregion

        #region Private Methods

        private void InvalidateSkiaVisuals()
        {
            _mainWaveform?.InvalidateVisual();
            _overviewWaveform?.InvalidateVisual();
        }

        private void SetOverviewWaveformClickPosition(Point clickPosition)
        {
            _shouldFollowWaveform = false;
            if (_followPlayHeadButton != null)
            {
                _followPlayHeadButton.IsChecked = _shouldFollowWaveform;
            }

            var maxTimeStamp = _lengthInSeconds - WidthToTimeSpan(_waveformAudioWidth);
            _currentTimestamp = TimeSpan.FromSeconds(
                Math.Min(ToTimestampFromOverviewWaveformX(clickPosition.X).TotalSeconds, maxTimeStamp.TotalSeconds));

            if (_lastClickPositionTextBlock != null)
            {
                _lastClickPositionTextBlock.Text =
                    $"X: {clickPosition.X}, Y: {clickPosition.Y}, timestamp: {_currentTimestamp.TotalSeconds}s";
            }
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
                        : _lengthInSeconds.TotalSeconds));
            }

            line.Timestamp = timestamp;
            line.UpdateOverviewWaveformLineX(ToOverviewWaveformXFromTimeStamp);
            line.UpdateMainWaveformLineX(ToMainWaveformXFromTimeStamp, 0, ActualWidth);
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
            return TimeSpan.FromSeconds((x * _lengthInSeconds.TotalSeconds) / _overviewAudioWidth);
        }

        private TimeSpan ToTimestampFromMainWaveformX(double x)
        {
            var timeSpan = WidthToTimeSpan(_waveformAudioWidth);
            var t = _shouldFollowWaveform ? _lengthInSeconds - timeSpan : _currentTimestamp;

            return t + TimeSpan.FromSeconds((x * timeSpan.TotalSeconds) / _waveformAudioWidth);
        }

        private double ToOverviewWaveformXFromTimeStamp(TimeSpan timestamp)
        {
            return (timestamp.TotalSeconds * _overviewAudioWidth) / _lengthInSeconds.TotalSeconds;
        }

        private double ToMainWaveformXFromTimeStamp(TimeSpan timestamp)
        {
            var timeSpan = WidthToTimeSpan(_waveformAudioWidth);
            var t = _shouldFollowWaveform ? timestamp - (_lengthInSeconds - timeSpan) : timestamp - _currentTimestamp;
            return (t.TotalSeconds * _waveformAudioWidth) / timeSpan.TotalSeconds;
        }

        private TimeSpan WidthToTimeSpan(double width)
        {
            return TimeSpan.FromSeconds(width * _lengthInSeconds.TotalSeconds / _audioArray.Length);
        }

        private double GetMainWaveformStartIndex()
        {
            return (_currentTimestamp.TotalSeconds * _audioArray.Length) / _lengthInSeconds.TotalSeconds;
        }

        private static float GetAbsoluteMaxValue(float[] audioData, int from, int to)
        {
            var slice = new Span<float>(audioData, from, to - from);
            float max = 0f;
            for (int i = 0; i < slice.Length; i++)
            {
                var abs = Math.Abs(slice[i]);
                if (abs > max)
                {
                    max = abs;
                }
            }

            return max;
        }

        private SKPaint CreatePaint(SKColor color, float strokeWidth)
            => new()
            {
                IsAntialias = true,
                Color = color,
                StrokeCap = SKStrokeCap.Round,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth
            };

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty LengthInSecondsProperty = DependencyProperty.Register(
            nameof(LengthInSeconds), typeof(TimeSpan), typeof(WaveformControl),
            new PropertyMetadata(default(TimeSpan), OnLengthInSecondsChanged));

        public TimeSpan LengthInSeconds
        {
            get { return (TimeSpan)GetValue(LengthInSecondsProperty); }
            set { SetValue(LengthInSecondsProperty, value); }
        }

        public static readonly DependencyProperty DisplayAudioDataProperty = DependencyProperty.Register(
            nameof(DisplayAudioData), typeof(float[]), typeof(WaveformControl),
            new PropertyMetadata(default(float[]), OnDisplayAudioDataChanged));

        public float[] DisplayAudioData
        {
            get { return (float[])GetValue(DisplayAudioDataProperty); }
            set { SetValue(DisplayAudioDataProperty, value); }
        }

        public static readonly DependencyProperty MarkerLineBrushProperty = DependencyProperty.Register(
            nameof(MarkerLineBrush), typeof(Brush), typeof(WaveformControl), new PropertyMetadata(default(Brush)));

        public Brush MarkerLineBrush
        {
            get { return (Brush)GetValue(MarkerLineBrushProperty); }
            set { SetValue(MarkerLineBrushProperty, value); }
        }

        public static readonly DependencyProperty SelectedLineBrushProperty = DependencyProperty.Register(
            nameof(SelectedLineBrush), typeof(Brush), typeof(WaveformControl), new PropertyMetadata(default(Brush)));

        public Brush SelectedLineBrush
        {
            get { return (Brush)GetValue(SelectedLineBrushProperty); }
            set { SetValue(SelectedLineBrushProperty, value); }
        }

        public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register(
            nameof(LineBrush), typeof(Brush), typeof(WaveformControl), new PropertyMetadata(default(Brush)));

        public Brush LineBrush
        {
            get { return (Brush)GetValue(LineBrushProperty); }
            set { SetValue(LineBrushProperty, value); }
        }

        public static readonly DependencyProperty LineThicknessProperty = DependencyProperty.Register(
            nameof(LineThickness), typeof(double), typeof(WaveformControl), new PropertyMetadata(default(double)));

        public double LineThickness
        {
            get { return (double)GetValue(LineThicknessProperty); }
            set { SetValue(LineThicknessProperty, value); }
        }

        public static readonly DependencyProperty OverviewWaveformStrokeWidthProperty = DependencyProperty.Register(
            nameof(OverviewWaveformStrokeWidth), typeof(float), typeof(WaveformControl),
            new PropertyMetadata(default(float)));

        public float OverviewWaveformStrokeWidth
        {
            get { return (float)GetValue(OverviewWaveformStrokeWidthProperty); }
            set { SetValue(OverviewWaveformStrokeWidthProperty, value); }
        }

        public static readonly DependencyProperty MainWaveformStrokeWidthProperty = DependencyProperty.Register(
            nameof(MainWaveformStrokeWidth), typeof(float), typeof(WaveformControl),
            new PropertyMetadata(default(float)));

        public float MainWaveformStrokeWidth
        {
            get { return (float)GetValue(MainWaveformStrokeWidthProperty); }
            set { SetValue(MainWaveformStrokeWidthProperty, value); }
        }

        public static readonly DependencyProperty OverviewWaveformColorProperty = DependencyProperty.Register(
            nameof(OverviewWaveformColor), typeof(SKColor), typeof(WaveformControl),
            new PropertyMetadata(default(SKColor)));

        public SKColor OverviewWaveformColor
        {
            get { return (SKColor)GetValue(OverviewWaveformColorProperty); }
            set { SetValue(OverviewWaveformColorProperty, value); }
        }

        public static readonly DependencyProperty MainWaveformColorProperty = DependencyProperty.Register(
            nameof(MainWaveformColor), typeof(SKColor), typeof(WaveformControl),
            new PropertyMetadata(default(SKColor)));

        public SKColor MainWaveformColor
        {
            get { return (SKColor)GetValue(MainWaveformColorProperty); }
            set { SetValue(MainWaveformColorProperty, value); }
        }

        public static readonly DependencyProperty ButtonMarginProperty = DependencyProperty.Register(
            nameof(ButtonMargin), typeof(Thickness), typeof(WaveformControl), new PropertyMetadata(default(Thickness)));

        public Thickness ButtonMargin
        {
            get { return (Thickness)GetValue(ButtonMarginProperty); }
            set { SetValue(ButtonMarginProperty, value); }
        }

        public static readonly DependencyProperty OverviewBorderBrushProperty = DependencyProperty.Register(
            nameof(OverviewBorderBrush), typeof(Brush), typeof(WaveformControl), new PropertyMetadata(default(Brush)));

        public Brush OverviewBorderBrush
        {
            get { return (Brush)GetValue(OverviewBorderBrushProperty); }
            set { SetValue(OverviewBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty OverviewBorderThicknessProperty = DependencyProperty.Register(
            nameof(OverviewBorderThickness), typeof(Thickness), typeof(WaveformControl),
            new PropertyMetadata(default(Thickness)));

        public Thickness OverviewBorderThickness
        {
            get { return (Thickness)GetValue(OverviewBorderThicknessProperty); }
            set { SetValue(OverviewBorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty OverviewHeightProperty = DependencyProperty.Register(
            nameof(OverviewHeight), typeof(double), typeof(WaveformControl), new PropertyMetadata(default(double)));

        public double OverviewHeight
        {
            get { return (double)GetValue(OverviewHeightProperty); }
            set { SetValue(OverviewHeightProperty, value); }
        }

        public static readonly DependencyProperty PlayProperty = DependencyProperty.Register(
            nameof(Play), typeof(ICommand), typeof(WaveformControl), new PropertyMetadata(default(ICommand)));

        public ICommand Play
        {
            get { return (ICommand)GetValue(PlayProperty); }
            set { SetValue(PlayProperty, value); }
        }

        public static readonly DependencyProperty PauseProperty = DependencyProperty.Register(
            nameof(Pause), typeof(ICommand), typeof(WaveformControl), new PropertyMetadata(default(ICommand)));

        public ICommand Pause
        {
            get { return (ICommand)GetValue(PauseProperty); }
            set { SetValue(PauseProperty, value); }
        }

        public static readonly DependencyProperty StopProperty = DependencyProperty.Register(
            nameof(Stop), typeof(ICommand), typeof(WaveformControl), new PropertyMetadata(default(ICommand)));

        public ICommand Stop
        {
            get { return (ICommand)GetValue(StopProperty); }
            set { SetValue(StopProperty, value); }
        }

        #endregion
    }
}