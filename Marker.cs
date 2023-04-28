using SkiaSharp;

namespace SampleSkiaMaui
{
    internal class Marker
    {
        public SKPoint Centre { get; private set; }

        public Marker(float x, float y)
        {
            Centre = new SKPoint(x, y);
        }

        public Marker(SKPoint centre)
        {
            Centre = centre;
        }

        public void MoveTo(float x, float y)
        {
            Centre = new SKPoint(x, y);
        }

        public void MoveTo(SKPoint point)
        {
            Centre = point;
        }
    }
}
