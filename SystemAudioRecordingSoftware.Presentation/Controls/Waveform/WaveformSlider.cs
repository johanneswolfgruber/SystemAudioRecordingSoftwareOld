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
        private readonly Func<TimeSpan, double> _timeToX;
        private readonly Rectangle _rectangle;
        private HitType _mouseHitType = HitType.None;
        private Point _lastPoint;
        private bool _dragInProgress;
        
        public WaveformSlider(SKElement skElement, Func<double, TimeSpan> xToTime, Func<TimeSpan, double> timeToX)
        {
            _skElement = skElement;
            _xToTime = xToTime;
            _timeToX = timeToX;

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
            _skElement.MouseWheel += OnMouseScroll;
        }

        public double RectangleWidth => _rectangle.Width;
        public double RectangleHeight => _rectangle.Height;
        public double RectangleLeft => _rectangle.Left;
        public double RectangleRight => RectangleLeft + RectangleWidth;
        public double RectangleMid => RectangleLeft + (RectangleWidth / 2);
        public bool ShouldFollowWaveform { get; set; } = true;
        public TimeSpan SelectedTimeStamp { get; private set; }

        public void ZoomIn(TimeSpan? zoomAround = null)
        {
            var oldMidTime = _xToTime(RectangleMid);
            
            SetRectangleWidth(RectangleWidth - 10);

            CenterAround(zoomAround, oldMidTime);
        }

        public void ZoomOut(TimeSpan? zoomAround = null)
        {
            var oldMidTime = _xToTime(RectangleMid);
            
            SetRectangleWidth(RectangleWidth + 10);
            while (RectangleRight > _skElement.ActualWidth)
            {
                SetRectangleLeft(RectangleLeft - 10);
            }
            
            CenterAround(zoomAround, oldMidTime);
        }

        public void SetRectangleLeft(double left)
        {
            _rectangle.Left = (float)Math.Clamp(left, 0f, _skElement.ActualWidth - RectangleWidth);
        }

        public void SetRectangleWidth(double width)
        {
            _rectangle.Width = (float)Math.Clamp(width, 20, _skElement.ActualWidth);
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
            var leftLine = new Line((float)RectangleLeft, (float)RectangleHeight)
            {
                Color = SKColors.Black, 
                Opacity = _rectangle.Opacity,
                StrokeWidth = 4f
            };
            var rightLine = new Line((float)RectangleRight, (float)RectangleHeight)
            {
                Color = SKColors.Black,
                Opacity = _rectangle.Opacity,
                StrokeWidth = 4f
            };
            leftLine.Draw(canvas);
            rightLine.Draw(canvas);
        }

        public void Reset()
        {
            SetRectangleVisibility(Visibility.Hidden);
            SetRectangleWidth(300);
            ShouldFollowWaveform = true;
            _lastPoint = new Point(0, 0);
        }

        private void CenterAround(TimeSpan? zoomAround, TimeSpan oldMidTime)
        {
            var midTime = zoomAround ?? oldMidTime;
            var mid = _timeToX(midTime);
            SetRectangleLeft(mid - (RectangleWidth / 2));
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

        private void OnMouseScroll(object sender, MouseWheelEventArgs args)
        {
            var point = Mouse.GetPosition(_skElement);
            
            if (!_rectangle.HitTest(point.X, point.Y))
            {
                return;
            }

            var t = _xToTime(point.X);
            if (args.Delta > 0) ZoomIn(t);
            if (args.Delta < 0) ZoomOut(t);
        }
    }
}