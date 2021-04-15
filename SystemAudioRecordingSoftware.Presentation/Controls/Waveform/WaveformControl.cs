using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using SystemAudioRecordingSoftware.Domain.Model;
using SystemAudioRecordingSoftware.Presentation.Controls.Lines;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    // TODO: factor out line handling
    // TODO: rethink reset handling

    [TemplatePart(Name = ContentPart, Type = typeof(Grid))]
    [TemplatePart(Name = MainWaveformPart, Type = typeof(SKElement))]
    [TemplatePart(Name = MainLineCanvasPart, Type = typeof(Canvas))]
    [TemplatePart(Name = OverviewWaveformPart, Type = typeof(SKElement))]
    [TemplatePart(Name = OverviewLineCanvasPart, Type = typeof(Canvas))]
    [TemplatePart(Name = OverviewRectangleCanvasPart, Type = typeof(Canvas))]
    [TemplatePart(Name = ButtonsPart, Type = typeof(StackPanel))]
    [TemplatePart(Name = ZoomInPart, Type = typeof(Button))]
    [TemplatePart(Name = ZoomOutPart, Type = typeof(Button))]
    [TemplatePart(Name = AddSnipPart, Type = typeof(Button))]
    [TemplatePart(Name = RemoveSnipPart, Type = typeof(Button))]
    [TemplatePart(Name = FollowPlayHeadPart, Type = typeof(Button))]
    [TemplatePart(Name = TimeDisplay, Type = typeof(TextBlock))]
    public partial class WaveformControl : Control, IDisposable
    {
        private const string ContentPart = "PART_Content";
        private const string MainWaveformPart = "PART_MainWaveform";
        private const string MainLineCanvasPart = "PART_MainLineCanvas";
        private const string OverviewWaveformPart = "PART_OverviewWaveform";
        private const string OverviewLineCanvasPart = "PART_OverviewLineCanvas";
        private const string OverviewRectangleCanvasPart = "PART_OverviewRectangleCanvas";
        private const string ButtonsPart = "PART_Buttons";
        private const string ZoomInPart = "PART_ZoomInButton";
        private const string ZoomOutPart = "PART_ZoomOutButton";
        private const string AddSnipPart = "PART_AddSnipButton";
        private const string RemoveSnipPart = "PART_RemoveSnipButton";
        private const string FollowPlayHeadPart = "PART_FollowPlayHeadButton";
        private const string TimeDisplay = "PART_TimeDisplay";

        private readonly IDisposable _redrawTimer;

        private readonly List<LineContainer> _snipLines = new();
        private Button? _addSnipButton;
        private AudioDataPoint[] _audioArray = Array.Empty<AudioDataPoint>();
        private TimeSpan _currentTimestamp;
        private ToggleButton? _followPlayHeadButton;
        private TimeSpan _length;
        private Canvas? _mainLineCanvas;
        private LineContainer? _markerLines;
        private Canvas? _overviewLineCanvas;
        private Canvas? _overviewRectangleCanvas;
        private Point _lastPoint;
        private bool _dragInProgress;
        private Rectangle? _overviewRectangle;
        // private Rectangle? _overviewRectangleLeft;
        // private Rectangle? _overviewRectangleRight;
        private Button? _removeSnipButton;
        private IDisposable? _resetSubscription;
        private LineContainer? _selectedLines;
        private bool _shouldFollowWaveform = true;
        private TextBlock? _timeDisplayTextBlock;
        private Button? _zoomInButton;
        private Button? _zoomOutButton;
        private AudioWaveform? _audioWaveform;

        public WaveformControl()
        {
            DisplayAudioData = new ObservableCollection<AudioDataPoint>();
            SnipTimeStamps = new ObservableCollection<TimeSpan>();

            _redrawTimer = Observable
                .Interval(TimeSpan.FromMilliseconds(60))
                .ObserveOnDispatcher()
                .Subscribe(_ => RenderAudioWaveform());
        }

        public void Dispose()
        {
            _redrawTimer.Dispose();
            _resetSubscription?.Dispose();
            GC.SuppressFinalize(this);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var mainSkElement = GetTemplateChild(MainWaveformPart) as SKElement;
            var overviewSkElement = GetTemplateChild(OverviewWaveformPart) as SKElement;
            _mainLineCanvas = GetTemplateChild(MainLineCanvasPart) as Canvas;
            _overviewLineCanvas = GetTemplateChild(OverviewLineCanvasPart) as Canvas;
            _overviewRectangleCanvas = GetTemplateChild(OverviewRectangleCanvasPart) as Canvas;

            _zoomInButton = GetTemplateChild(ZoomInPart) as Button;
            _zoomOutButton = GetTemplateChild(ZoomOutPart) as Button;
            _addSnipButton = GetTemplateChild(AddSnipPart) as Button;
            _removeSnipButton = GetTemplateChild(RemoveSnipPart) as Button;
            _followPlayHeadButton = GetTemplateChild(FollowPlayHeadPart) as ToggleButton;
            _timeDisplayTextBlock = GetTemplateChild(TimeDisplay) as TextBlock;

            if (mainSkElement is null || _mainLineCanvas is null || overviewSkElement is null ||
                _overviewLineCanvas is null || _overviewRectangleCanvas is null || _zoomInButton is null || 
                _zoomOutButton is null || _addSnipButton is null || _removeSnipButton is null || 
                _followPlayHeadButton is null || _timeDisplayTextBlock is null)
            {
                throw new InvalidOperationException("Missing template part");
            }

            _audioWaveform = new AudioWaveform(
                mainSkElement, 
                overviewSkElement, 
                new AudioWaveformStyle(MainWaveformColor, MainWaveformStrokeWidth),
                new AudioWaveformStyle(OverviewWaveformColor, OverviewWaveformStrokeWidth));

            mainSkElement.MouseDown += OnMainLineCanvasMouseDown;
            mainSkElement.MouseMove += OnMainLineCanvasMouseMove;
            overviewSkElement.MouseDown += OnOverviewLineCanvasMouseDown;
            overviewSkElement.MouseMove += OnOverviewLineCanvasMouseMove;

            _overviewRectangleCanvas.Width = overviewSkElement.Width;

            _overviewRectangle = new Rectangle
            {
                Width = 100,
                Height = _overviewRectangleCanvas.Height,
                Fill = Brushes.White,
                Opacity = 0.6,
                Visibility = Visibility.Hidden
            };

            _overviewRectangleCanvas.Children.Add(_overviewRectangle);
            Canvas.SetLeft(_overviewRectangle, _lastPoint.X);
            
            _overviewRectangle.MouseEnter += OnOverviewRectangleMouseEnter;
            _overviewRectangle.MouseLeave += OnOverviewRectangleMouseLeave;
            _overviewRectangle.MouseMove += OnOverviewRectangleMouseMove;
            _overviewRectangleCanvas.MouseDown += OnOverviewRectangleCanvasMouseDown;
            _overviewRectangleCanvas.MouseUp += OnOverviewRectangleCanvasMouseUp;
            _overviewRectangleCanvas.MouseMove += OnOverviewRectangleCanvasMouseMove;

            _zoomInButton.Click += OnZoomInClicked;
            _zoomOutButton.Click += OnZoomOutClicked;
            _addSnipButton.Click += OnAddSnipClicked;
            _removeSnipButton.Click += OnRemoveSnipClicked;
            _followPlayHeadButton.Click += OnFollowPlayHeadClicked;

            _followPlayHeadButton.IsChecked = _shouldFollowWaveform;
        }
    }
}