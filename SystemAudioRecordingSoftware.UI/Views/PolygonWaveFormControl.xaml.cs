// (c) Johannes Wolfgruber, 2020

using MaterialDesignThemes.Wpf;
using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SystemAudioRecordingSoftware.UI.Views
{
    /// <summary>
    /// Interaction logic for PolygonWaveFormControl.xaml
    /// </summary>
    public partial class PolygonWaveFormControl
    {
        private readonly int _blankZone = 10;
        private readonly Polygon _waveForm = new Polygon();
        private readonly double _xScale = 2;
        private int _renderPosition;
        private double _yScale = 40;
        private double _yTranslate = 40;

        public PolygonWaveFormControl()
        {
            Observable
                .FromEventPattern<SizeChangedEventArgs>(this, nameof(SizeChanged))
                .Subscribe(x => OnSizeChanged());

            InitializeComponent();

            _waveForm.Stroke = Foreground;
            _waveForm.StrokeThickness = 1;
            var palette = new PaletteHelper().GetTheme();
            _waveForm.Fill = new SolidColorBrush(palette.BodyLight);
            MainCanvas.Children.Add(_waveForm);
        }

        private int Points
        {
            get { return _waveForm.Points.Count / 2; }
        }

        public void AddValue(float maxValue, float minValue)
        {
            int visiblePixels = (int)(ActualWidth / _xScale);
            if (visiblePixels > 0)
            {
                CreatePoint(maxValue, minValue);

                if (_renderPosition > visiblePixels)
                {
                    _renderPosition = 0;
                }
                int erasePosition = (_renderPosition + _blankZone) % visiblePixels;
                if (erasePosition < Points)
                {
                    double yPos = SampleToYPosition(0);
                    _waveForm.Points[erasePosition] = new Point(erasePosition * _xScale, yPos);
                    _waveForm.Points[BottomPointIndex(erasePosition)] = new Point(erasePosition * _xScale, yPos);
                }
            }
        }

        public void Reset()
        {
            _renderPosition = 0;
            ClearAllPoints();
        }

        private int BottomPointIndex(int position)
        {
            return _waveForm.Points.Count - position - 1;
        }

        private void ClearAllPoints()
        {
            _waveForm.Points.Clear();
        }

        private void CreatePoint(float topValue, float bottomValue)
        {
            double topYPos = SampleToYPosition(topValue);
            double bottomYPos = SampleToYPosition(bottomValue);
            double xPos = _renderPosition * _xScale;
            if (_renderPosition >= Points)
            {
                int insertPos = Points;
                _waveForm.Points.Insert(insertPos, new Point(xPos, topYPos));
                _waveForm.Points.Insert(insertPos + 1, new Point(xPos, bottomYPos));
            }
            else
            {
                _waveForm.Points[_renderPosition] = new Point(xPos, topYPos);
                _waveForm.Points[BottomPointIndex(_renderPosition)] = new Point(xPos, bottomYPos);
            }
            _renderPosition++;
        }

        private void OnSizeChanged()
        {
            _renderPosition = 0;
            ClearAllPoints();

            _yTranslate = ActualHeight / 2;
            _yScale = ActualHeight / 2;
        }

        private double SampleToYPosition(float value)
        {
            return _yTranslate + value * _yScale;
        }
    }
}
