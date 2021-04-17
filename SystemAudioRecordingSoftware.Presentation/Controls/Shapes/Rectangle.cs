using SkiaSharp;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Shapes
{
    internal class Rectangle : Shape
    {
        private SKRect _rectangle;
        public float Left { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Top { get; set; }
        
        public override void Draw(SKCanvas canvas, SKPaint? paint = null)
        {
            _rectangle = new SKRect(Left, Top, Left + Width, Top + Height);
            canvas.DrawRect(_rectangle, paint ?? CreatePaint());
        }

        public override bool HitTest(double x, double y)
        {
            return _rectangle.Contains((float)x, (float)y);
        }
    }
}