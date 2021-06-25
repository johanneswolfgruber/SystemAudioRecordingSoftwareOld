using SkiaSharp.Views.WPF;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using SystemAudioRecordingSoftware.Domain.Model;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    [TemplatePart(Name = ContentPart, Type = typeof(Grid))]
    [TemplatePart(Name = MainWaveformPart, Type = typeof(SKElement))]
    [TemplatePart(Name = OverviewWaveformPart, Type = typeof(SKElement))]
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
        private const string OverviewWaveformPart = "PART_OverviewWaveform";
        private const string ButtonsPart = "PART_Buttons";
        private const string ZoomInPart = "PART_ZoomInButton";
        private const string ZoomOutPart = "PART_ZoomOutButton";
        private const string AddSnipPart = "PART_AddSnipButton";
        private const string RemoveSnipPart = "PART_RemoveSnipButton";
        private const string FollowPlayHeadPart = "PART_FollowPlayHeadButton";
        private const string TimeDisplay = "PART_TimeDisplay";

        private readonly IDisposable _redrawTimer;

        private AudioDataPoint[] _audioArray = Array.Empty<AudioDataPoint>();
        private TimeSpan _length;
        private AudioWaveform? _audioWaveform;
        private Button? _addSnipButton;
        private Button? _removeSnipButton;
        private IDisposable? _resetSubscription;
        private ToggleButton? _followPlayHeadButton;
        private TextBlock? _timeDisplayTextBlock;
        private Button? _zoomInButton;
        private Button? _zoomOutButton;

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

            var mainElement = GetTemplateChild(MainWaveformPart) as SKElement;
            var overviewElement = GetTemplateChild(OverviewWaveformPart) as SKElement;

            _zoomInButton = GetTemplateChild(ZoomInPart) as Button;
            _zoomOutButton = GetTemplateChild(ZoomOutPart) as Button;
            _addSnipButton = GetTemplateChild(AddSnipPart) as Button;
            _removeSnipButton = GetTemplateChild(RemoveSnipPart) as Button;
            _followPlayHeadButton = GetTemplateChild(FollowPlayHeadPart) as ToggleButton;
            _timeDisplayTextBlock = GetTemplateChild(TimeDisplay) as TextBlock;

            if (mainElement is null || overviewElement is null || _zoomInButton is null || 
                _zoomOutButton is null || _addSnipButton is null || _removeSnipButton is null || 
                _followPlayHeadButton is null || _timeDisplayTextBlock is null)
            {
                throw new InvalidOperationException("Missing template part");
            }

            _audioWaveform = new AudioWaveform(
                mainElement,
                overviewElement,
                new AudioWaveformStyle(MainWaveformColor, MainWaveformStrokeWidth),
                new AudioWaveformStyle(OverviewWaveformColor, OverviewWaveformStrokeWidth));

            _audioWaveform.SnipLinesChanged += OnSnipLinesChanged;

            _zoomInButton.Click += OnZoomInClicked;
            _zoomOutButton.Click += OnZoomOutClicked;
            _addSnipButton.Click += OnAddSnipClicked;
            _removeSnipButton.Click += OnRemoveSnipClicked;
            _followPlayHeadButton.Click += OnFollowPlayHeadClicked;

            _followPlayHeadButton.IsChecked = true;
        }
    }
}