// unset

using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SystemAudioRecordingSoftware.Domain.Model;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    public partial class WaveformControl
    {
        public static readonly DependencyProperty LengthInSecondsProperty = DependencyProperty.Register(
            nameof(LengthInSeconds), typeof(TimeSpan), typeof(WaveformControl),
            new PropertyMetadata(default(TimeSpan), OnLengthInSecondsChanged));

        public TimeSpan LengthInSeconds
        {
            get { return (TimeSpan)GetValue(LengthInSecondsProperty); }
            set { SetValue(LengthInSecondsProperty, value); }
        }

        public static readonly DependencyProperty DisplayAudioDataProperty = DependencyProperty.Register(
            nameof(DisplayAudioData), typeof(ObservableCollection<AudioDataPoint>), typeof(WaveformControl),
            new PropertyMetadata(default(ObservableCollection<AudioDataPoint>), OnDisplayAudioDataPropertyChanged));

        public ObservableCollection<AudioDataPoint> DisplayAudioData
        {
            get { return (ObservableCollection<AudioDataPoint>)GetValue(DisplayAudioDataProperty); }
            set { SetValue(DisplayAudioDataProperty, value); }
        }

        public static readonly DependencyProperty SnipTimeStampsProperty = DependencyProperty.Register(
            nameof(SnipTimeStamps), typeof(ObservableCollection<TimeSpan>), typeof(WaveformControl),
            new PropertyMetadata(default(ObservableCollection<TimeSpan>), OnSnipTimeStampsPropertyChanged));

        public ObservableCollection<TimeSpan> SnipTimeStamps
        {
            get { return (ObservableCollection<TimeSpan>)GetValue(SnipTimeStampsProperty); }
            set { SetValue(SnipTimeStampsProperty, value); }
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

        public static readonly DependencyProperty ResetProperty = DependencyProperty.Register(
            nameof(Reset), typeof(IObservable<Unit>), typeof(WaveformControl),
            new PropertyMetadata(default(IObservable<Unit>), OnResetPropertyChanged));

        public IObservable<Unit> Reset
        {
            get { return (IObservable<Unit>)GetValue(ResetProperty); }
            set { SetValue(ResetProperty, value); }
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
        
        public static readonly DependencyProperty SnipsChangedProperty = DependencyProperty.Register(
            nameof(SnipsChanged), typeof(ICommand), typeof(WaveformControl), new PropertyMetadata(default(ICommand)));

        public ICommand SnipsChanged
        {
            get { return (ICommand)GetValue(SnipsChangedProperty); }
            set { SetValue(SnipsChangedProperty, value); }
        }

        public static readonly DependencyProperty SnipAddedProperty = DependencyProperty.Register(
            nameof(SnipAdded), typeof(ICommand), typeof(WaveformControl), new PropertyMetadata(default(ICommand)));

        public ICommand SnipAdded
        {
            get { return (ICommand)GetValue(SnipAddedProperty); }
            set { SetValue(SnipAddedProperty, value); }
        }

        public static readonly DependencyProperty SnipRemovedProperty = DependencyProperty.Register(
            nameof(SnipRemoved), typeof(ICommand), typeof(WaveformControl), new PropertyMetadata(default(ICommand)));

        public ICommand SnipRemoved
        {
            get { return (ICommand)GetValue(SnipRemovedProperty); }
            set { SetValue(SnipRemovedProperty, value); }
        }
    }
}