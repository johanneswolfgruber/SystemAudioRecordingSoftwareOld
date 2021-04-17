using SkiaSharp.Views.WPF;
using System;
using System.Linq;
using System.Windows.Input;
using SystemAudioRecordingSoftware.Presentation.Controls.Shapes;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    internal class LineDisplay : LineDisplayBase
    {
        private readonly SKElement _skElement;
        private readonly Func<double, TimeSpan> _xToTime;

        public LineDisplay(SKElement skElement, Func<double, TimeSpan> xToTime, Func<TimeSpan, double> timeToX)
            : base(timeToX)
        {
            _skElement = skElement;
            _xToTime = xToTime;

            _skElement.MouseDown += OnMouseDown;
            _skElement.MouseMove += OnMouseMove;
            _skElement.MouseLeave += OnMouseLeave;
        }

        public override void AddSnipLine(TimeSpan? timeStamp = null)
        {
            _snipLines.ForEach(x => x.IsSelected = false);
            
            var line = timeStamp is null
                ? Marker.Line
                : new Line((float)_timeToX(timeStamp.Value), _skElement.CanvasSize.Height);
            _snipLines.Add(new MarkerLine(line, timeStamp ?? Marker.TimeStamp));
            _snipLines.Last().IsSelected = true;
        }

        public override void RemoveSnipLine(TimeSpan? timeStamp = null)
        {
            if (timeStamp is null)
            {
                _snipLines.RemoveAll(x => x.IsSelected);
            }
            else
            {
                _snipLines.RemoveAll(x => x.TimeStamp == timeStamp);
            }

            if (_snipLines.Count > 0)
            {
                _snipLines.Last().IsSelected = true;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs args)
        {
            var point = args.GetPosition(_skElement);
            
            if (Marker.Line.HitTest(point.X, point.Y) || _snipLines.Any(x => x.Line.HitTest(point.X, point.Y)))
            {
                Mouse.OverrideCursor = Cursors.SizeWE;
            }
            else
            {
                Mouse.OverrideCursor = null;
            }

            if (args.LeftButton == MouseButtonState.Pressed)
            {
                MoveLine(args.GetPosition(_skElement).X);
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs args)
        {
            var point = args.GetPosition(_skElement);
            
            _snipLines.ForEach(x => x.IsSelected = x.Line.HitTest(point.X, point.Y));

            if (!_snipLines.Any(x => x.IsSelected))
            {
                SetMarker(point.X);
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs args)
        {
            Mouse.OverrideCursor = null;
        }
        
        private void SetMarker(double x)
        {
            Marker = new MarkerLine(new Line((float)x, _skElement.CanvasSize.Height), _xToTime(x));
        }
        
        private void MoveLine(double newX)
        {
            Marker.Line.SetX((float)newX);
            Marker.TimeStamp = _xToTime(newX);

            _snipLines.ForEach(x =>
            {
                if (!x.IsSelected)
                {
                    return;
                }

                x.Line.SetX((float)newX);
                x.TimeStamp = _xToTime(newX);
            });
        }
    }
}