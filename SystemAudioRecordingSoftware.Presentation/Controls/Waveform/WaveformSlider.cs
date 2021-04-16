using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    internal class WaveformSlider
    {
        private enum HitType
        {
            None, Body, LeftEdge, RightEdge
        };

        private readonly Canvas _canvas;
        private readonly Rectangle _rectangle;
        private HitType _mouseHitType = HitType.None;
        private Point _lastPoint;
        private bool _dragInProgress;
        
        public WaveformSlider(Canvas canvas)
        {
            _canvas = canvas;

            _rectangle = new Rectangle
            {
                Width = 300,
                Height = _canvas.Height,
                Fill = Brushes.White,
                Opacity = 0.6,
                Visibility = Visibility.Hidden
            };

            _canvas.Children.Add(_rectangle);
            Canvas.SetLeft(_rectangle, _lastPoint.X);
            
            _rectangle.MouseEnter += OnRectangleMouseEnterAndMove;
            _rectangle.MouseLeave += OnRectangleMouseLeave;
            _rectangle.MouseMove += OnRectangleMouseEnterAndMove;
            _canvas.MouseDown += OnRectangleCanvasMouseDown;
            _canvas.MouseUp += OnRectangleCanvasMouseUp;
            _canvas.MouseMove += OnRectangleCanvasMouseMove;
        }

        public double RectangleWidth => _rectangle.Width;
        public double RectangleLeft => Canvas.GetLeft(_rectangle);
        public double RectangleRight => RectangleLeft + RectangleWidth;
        public bool ShouldFollowWaveform { get; set; } = true;


        public void SetRectangleLeft(double left)
        {
            Canvas.SetLeft(_rectangle, left);
        }

        public void SetRectangleWidth(double width)
        {
            _rectangle.Width = width;
        }

        public void SetRectangleVisibility(Visibility visibility)
        {
            _rectangle.Visibility = visibility;
        }

        public void SnapToRight()
        {
            SetRectangleLeft(_canvas.ActualWidth - RectangleWidth);
        }

        private HitType SetHitType(Point point)
        {
            if (point.X < RectangleLeft) return HitType.None;
            if (point.X > RectangleRight) return HitType.None;
            
            const double gap = 10;
            if (point.X - RectangleLeft < gap)
            {
                return HitType.LeftEdge;
            }
            
            if (RectangleRight - point.X < gap)
            {
                return HitType.RightEdge;
            }
            
            return HitType.Body;
        }

        private void SetMouseCursor(double x)
        {
            if (x > 10 && x < RectangleWidth - 10)
            {
                Mouse.OverrideCursor = Cursors.ScrollWE;
                return;
            }

            Mouse.OverrideCursor = Cursors.SizeWE;
        }

        public void OnRectangleCanvasMouseMove(object sender, MouseEventArgs args)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            var point = Mouse.GetPosition(_canvas);

            if (!_dragInProgress)
            {
                _mouseHitType = SetHitType(point);
                return;
            }

            var offsetX = point.X - _lastPoint.X;
            var newX = RectangleLeft;
            var newWidth = RectangleWidth;

            switch (_mouseHitType)
            {
                case HitType.Body:
                    newX += offsetX;
                    break;
                case HitType.LeftEdge:
                    newX += offsetX;
                    newWidth -= offsetX;
                    break;
                case HitType.RightEdge:
                    newWidth += offsetX;
                    break;
                case HitType.None:
                    break;
                default:
                    throw new InvalidOperationException("Unknown hit type");
            }

            newX = Math.Clamp(newX, 0, _canvas.ActualWidth - RectangleWidth);
            newWidth = Math.Clamp(newWidth, 20, _canvas.ActualWidth);

            SetRectangleWidth(newWidth);
            SetRectangleLeft(newX);

            _lastPoint = point;
            ShouldFollowWaveform = false;
        }

        public void OnRectangleCanvasMouseUp(object sender, MouseButtonEventArgs args)
        {
            _dragInProgress = false;
            Mouse.OverrideCursor = null;
        }

        public void OnRectangleCanvasMouseDown(object sender, MouseButtonEventArgs args)
        {
            _lastPoint = Mouse.GetPosition(_canvas);

            _mouseHitType = SetHitType(_lastPoint);

            if (_mouseHitType == HitType.None) return;

            _dragInProgress = true;
        }

        private void OnRectangleMouseLeave(object sender, MouseEventArgs args)
        {
            Mouse.OverrideCursor = null;
        }

        private void OnRectangleMouseEnterAndMove(object sender, MouseEventArgs args)
        {
            var x = args.GetPosition(_rectangle).X;
            SetMouseCursor(x);
        }
    }
}