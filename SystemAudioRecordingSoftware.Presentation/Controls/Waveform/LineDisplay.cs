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

        public override TimeSpan? AddSnipLine(TimeSpan? timeStamp = null)
        {
            var time = timeStamp ?? Marker.TimeStamp;
            if (SnipLines.Any(x => 
                time > x.TimeStamp - TimeSpan.FromSeconds(0.1) && time < x.TimeStamp + TimeSpan.FromSeconds(0.1)))
            {
                return null;
            }
            
            SnipLines.ForEach(x => x.IsSelected = false);
            
            var line = timeStamp is null
                ? Marker.Line
                : new Line((float)TimeToX(timeStamp.Value), _skElement.CanvasSize.Height);
            SnipLines.Add(new MarkerLine(line, timeStamp ?? Marker.TimeStamp));
            SnipLines.Last().IsSelected = true;

            return SnipLines.Last().TimeStamp;
        }

        public override TimeSpan? RemoveSnipLine(TimeSpan? timeStamp = null)
        {
            var snip = timeStamp is null ? 
                SnipLines.FirstOrDefault(x => x.IsSelected) : 
                SnipLines.FirstOrDefault(x => x.TimeStamp == timeStamp);
            
            if (snip is null)
            {
                return null;
            }
                
            SnipLines.Remove(snip);

            if (SnipLines.Count > 0)
            {
                SnipLines.Last().IsSelected = true;
            }

            return snip.TimeStamp;
        }

        private void OnMouseMove(object sender, MouseEventArgs args)
        {
            var point = args.GetPosition(_skElement);
            
            if (Marker.Line.HitTest(point.X, point.Y) || SnipLines.Any(x => x.Line.HitTest(point.X, point.Y)))
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
            
            SnipLines.ForEach(x => x.IsSelected = x.Line.HitTest(point.X, point.Y));

            if (!SnipLines.Any(x => x.IsSelected))
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

            SnipLines.ForEach(x =>
            {
                if (!x.IsSelected)
                {
                    return;
                }

                x.Line.SetX((float)newX);
                x.TimeStamp = _xToTime(newX);
                OnSnipLinesChanged(EventArgs.Empty);
            });
        }
    }
}