using SkiaSharp;
using System;
using System.Collections.Generic;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    internal interface ILineDisplay
    {
        MarkerLine Marker { get; }
        IReadOnlyList<MarkerLine> SnipLines { get; }
        void SetLines(MarkerLine marker, List<MarkerLine> snipLines);
        void AddSnipLine(TimeSpan? timeStamp = null);
        void RemoveSnipLine(TimeSpan? timeStamp = null);
        void Render(SKCanvas canvas);
    }

    internal class LineDisplayBase : ILineDisplay
    {
        protected readonly Func<TimeSpan, double> _timeToX;
        protected List<MarkerLine> _snipLines = new();

        public LineDisplayBase(Func<TimeSpan, double> timeToX)
        {
            _timeToX = timeToX;
        }

        public MarkerLine Marker { get; protected set; } = new();
        public IReadOnlyList<MarkerLine> SnipLines => _snipLines;
        
        public void SetLines(MarkerLine marker, List<MarkerLine> snipLines)
        {
            Marker = marker;
            _snipLines = snipLines;
        }

        public virtual void AddSnipLine(TimeSpan? timeStamp = null)
        {
        }

        public virtual void RemoveSnipLine(TimeSpan? timeStamp = null)
        {
        }

        public void Render(SKCanvas canvas)
        {
            Marker.Line.SetX((float)_timeToX(Marker.TimeStamp));
            Marker.Line.Draw(canvas, CreatePaint(false, true));

            _snipLines.ForEach(x =>
            {
                x.Line.SetX((float)_timeToX(x.TimeStamp));
                x.Line.Draw(canvas, CreatePaint(x.IsSelected));
            });
        }

        private static SKPaint CreatePaint(bool isSelected, bool isMarker = false)
            => new()
            {
                IsAntialias = true,
                Color = isMarker ? 
                    SKColors.Gray : 
                    isSelected ? SKColors.Aquamarine : SKColors.White,
                StrokeCap = SKStrokeCap.Round,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 2f
            };
    }
}