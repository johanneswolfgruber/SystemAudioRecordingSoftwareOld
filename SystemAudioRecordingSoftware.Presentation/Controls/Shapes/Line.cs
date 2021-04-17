using SkiaSharp;

namespace SystemAudioRecordingSoftware.Presentation.Controls.Shapes
{
    internal class Line : Shape
    {
        public Line()
        {
            P0 = new SKPoint(0f, 0f);
            P1 = new SKPoint(0f, 0f);
        }
        
        public Line(float x, float height)
        {
            P0 = new SKPoint(x, 0f);;
            P1 = new SKPoint(x, height);
        }

        public Line(SKPoint p0, SKPoint p1)
        {
            P0 = p0;
            P1 = p1;
        }

        public SKPoint P0 { get; set; }
        public SKPoint P1 { get; set; }

        public void SetX(float x)
        {
            P0 = new SKPoint(x, P0.Y);
            P1 = new SKPoint(x, P1.Y);
        }
        
        public void SetY(float y)
        {
            P0 = new SKPoint(P0.X, y);
            P1 = new SKPoint(P1.X, y);
        }
        
        public override void Draw(SKCanvas canvas, SKPaint? paint = null)
        {
            canvas.DrawLine(P0, P1, paint ?? CreatePaint());
        }

        public override bool HitTest(double x, double y)
        {
            return y >= P0.Y &&
                   y <= P1.Y &&
                   x >= P0.X - 10f && 
                   x <= P0.X + 10f && 
                   x >= P1.X - 10f && 
                   x <= P1.X + 10f;
        }
    }
}