using SkiaSharp;
using System;
using System.Collections.Generic;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Waveform
{
    internal interface ILineDisplay
    {
        MarkerLine Marker { get; }
        IReadOnlyList<MarkerLine> Snips { get; }
        void SetLines(MarkerLine marker, List<MarkerLine> snips);
        TimeSpan? AddSnipLine(TimeSpan? timeStamp = null);
        TimeSpan? RemoveSnipLine(TimeSpan? timeStamp = null);
        void Render(SKCanvas canvas);
        void Reset();
    }

    internal class LineDisplayBase : ILineDisplay
    {
        protected readonly Func<TimeSpan, double> TimeToX;
        protected List<MarkerLine> SnipLines = new();

        protected LineDisplayBase(Func<TimeSpan, double> timeToX)
        {
            TimeToX = timeToX;
        }

        public MarkerLine Marker { get; protected set; } = new();
        public IReadOnlyList<MarkerLine> Snips => SnipLines;
        
        public void SetLines(MarkerLine marker, List<MarkerLine> snips)
        {
            Marker = marker;
            SnipLines = snips;
        }

        public virtual TimeSpan? AddSnipLine(TimeSpan? timeStamp = null)
        {
            return null;
        }

        public virtual TimeSpan? RemoveSnipLine(TimeSpan? timeStamp = null)
        {
            return null;
        }

        public void Render(SKCanvas canvas)
        {
            Marker.Line.SetX((float)TimeToX(Marker.TimeStamp));
            Marker.Line.Draw(canvas, CreatePaint(false, true));

            SnipLines.ForEach(x =>
            {
                x.Line.SetX((float)TimeToX(x.TimeStamp));
                x.Line.Draw(canvas, CreatePaint(x.IsSelected));
            });
        }

        public void Reset()
        {
            Marker = new();
            SnipLines = new();
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