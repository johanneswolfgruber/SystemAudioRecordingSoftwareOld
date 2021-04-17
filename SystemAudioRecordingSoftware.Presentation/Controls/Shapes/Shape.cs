// unset

using SkiaSharp;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Shapes
{
    internal abstract class Shape
    {
        public SKColor Color { get; set; } = SKColors.Gray;
        public float StrokeWidth { get; set; } = 2f;
        public byte Opacity { get; set; } = 255;

        public abstract void Draw(SKCanvas canvas, SKPaint? paint = null);

        public abstract bool HitTest(double x, double y);

        protected virtual SKPaint CreatePaint()
            => new()
            {
                IsAntialias = true,
                Color = Color.WithAlpha(Opacity),
                StrokeCap = SKStrokeCap.Round,
                Style = SKPaintStyle.Fill,
                StrokeWidth = StrokeWidth
            };
    }
}