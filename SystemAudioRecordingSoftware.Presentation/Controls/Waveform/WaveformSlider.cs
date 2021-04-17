using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Windows;
using System.Windows.Input;
using SystemAudioRecordingSoftware.Presentation.Controls.Shapes;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    internal class WaveformSlider
    {
        private enum HitType
        {
            None, Body, LeftEdge, RightEdge
        };
        
        private readonly SKElement _skElement;
        private readonly Func<double, TimeSpan> _xToTime;
        private readonly Rectangle _rectangle;
        private HitType _mouseHitType = HitType.None;
        private Point _lastPoint;
        private bool _dragInProgress;
        
        public WaveformSlider(SKElement skElement, Func<double, TimeSpan> xToTime)
        {
            _skElement = skElement;
            _xToTime = xToTime;

            _rectangle = new Rectangle
            {
                Width = 300,
                Height = _skElement.CanvasSize.Height,
                Left = (float)_lastPoint.X,
                Opacity = 0
            };
            
            _skElement.MouseDown += OnMouseDown;
            _skElement.MouseUp += OnMouseUp;
            _skElement.MouseMove += OnMouseMove;
            _skElement.MouseLeave += OnMouseLeave;
        }

        public double RectangleWidth => _rectangle.Width;
        public double RectangleLeft => _rectangle.Left;
        public double RectangleRight => RectangleLeft + RectangleWidth;
        public bool ShouldFollowWaveform { get; set; } = true;
        public TimeSpan SelectedTimeStamp { get; private set; }

        public void SetRectangleLeft(double left)
        {
            _rectangle.Left = (float)left;
        }

        public void SetRectangleWidth(double width)
        {
            _rectangle.Width = (float)width;
        }

        public void SetRectangleVisibility(Visibility visibility)
        {
            _rectangle.Opacity = visibility == Visibility.Visible ? (byte)150 : (byte)0;
        }

        public void SnapToRight()
        {
            SetRectangleLeft(_skElement.ActualWidth - RectangleWidth);
        }

        public void Render(SKCanvas canvas)
        {
            if (ShouldFollowWaveform)
            {
                SnapToRight();
            }
            
            _rectangle.Height = canvas.DeviceClipBounds.Height;
            _rectangle.Draw(canvas);
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

        private void SetMouseCursor(double x, double y)
        {
            if (!_rectangle.HitTest(x, y))
            {
                Mouse.OverrideCursor = null;
                return;
            }
            
            if (x > RectangleLeft + 10 && x < RectangleRight - 10)
            {
                Mouse.OverrideCursor = Cursors.ScrollWE;
                return;
            }

            Mouse.OverrideCursor = Cursors.SizeWE;
        }

        private void OnMouseMove(object sender, MouseEventArgs args)
        {
            var point = Mouse.GetPosition(_skElement);
            SetMouseCursor(point.X, point.Y);

            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

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

            newX = Math.Clamp(newX, 0, _skElement.ActualWidth - RectangleWidth);
            newWidth = Math.Clamp(newWidth, 20, _skElement.ActualWidth);

            SetRectangleLeft(newX);
            SetRectangleWidth(newWidth);

            _lastPoint = point;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs args)
        {
            _dragInProgress = false;
            Mouse.OverrideCursor = null;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs args)
        {
            _lastPoint = Mouse.GetPosition(_skElement);
            ShouldFollowWaveform = false;
            SelectedTimeStamp = _xToTime(_lastPoint.X);

            if (!_rectangle.HitTest(_lastPoint.X, _lastPoint.Y))
            {
                SetRectangleLeft(_lastPoint.X);
                if (_lastPoint.X + RectangleWidth > _skElement.ActualWidth)
                {
                    SetRectangleWidth(_skElement.ActualWidth - RectangleLeft);
                }
            }

            _mouseHitType = SetHitType(_lastPoint);

            if (_mouseHitType == HitType.None) return;

            _dragInProgress = true;
        }

        private void OnMouseLeave(object sender, MouseEventArgs args)
        {
            Mouse.OverrideCursor = null;
        }
    }
}