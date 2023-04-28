using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Diagnostics;
using System.Reflection;

namespace SampleSkiaMaui
{
    internal class PointDragger : SKCanvasView
    {

        /// <summary>
        /// Background image of the canvas
        /// </summary>
        public SKImage BackgroundImage { get; private set; }

        public SKRect BackgroundRect { get; private set; }

        public float MarkerSizeScaledUnits { get; set; } = 25f;

        // Fields
        private List<Marker> markers = new List<Marker>();
        private int canvasWidth;
        private int canvasHeight;
        private Marker markerSelected;

        public PointDragger()
        {
            PaintSurface += OnDraw;
            EnableTouchEvents = true;
        }

        // OnDraw
        private void OnDraw(object sender, SKPaintSurfaceEventArgs e)
        {
            // Basics
            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;
            canvas.Clear();

            // Read in the background the first time round or if size has changed
            if (BackgroundImage == null || info.Width != canvasWidth || info.Height != canvasHeight)
            {
                canvasWidth = info.Width;
                canvasHeight = info.Height;
                BackgroundImage = GetImageFromResource("SampleSkiaMaui.Resources.xray.png");
                var aspectImage = (float)BackgroundImage.Width / BackgroundImage.Height;
                var aspectCanvas = (float)canvasWidth / canvasHeight;

                // Compute marker halfsize as this is the padding we need to stop markers getting clipped
                int pad = (int)(0.5 * MarkerSizeScaledUnits * DeviceDisplay.MainDisplayInfo.Density);

                // Always fit width?
                BackgroundRect = new SKRect(
                        pad,
                        pad,
                        pad + (canvasWidth - 2 * pad),
                        pad + ((canvasWidth - 2 * pad) / aspectImage)
                        );

                // Auto size the height of the control based on the background rectangle
                HeightRequest = (BackgroundRect.Height + 2 * pad) / DeviceDisplay.MainDisplayInfo.Density;
            }

            // Seed some markers if there aren't any for this demo
            if (markers.Count == 0)
            {
                var spacing = BackgroundRect.Width / 4f;
                var centreline = BackgroundRect.Height / 2f;
                for (int i = 0; i < 3; ++i)
                {
                    markers.Add(new Marker(new SKPoint((i + 1) * spacing, centreline)));
                }
            }

            // Use anti-aliasing
            using(var imgPaint = new SKPaint
            {
                IsAntialias = true
            })
            using (var markerPaint = new SKPaint
            {
                IsAntialias = true,
                Color = Colors.Red.ToSKColor()
            })
            using (var linePaint = new SKPaint
            {
                IsAntialias = true,
                Color = Colors.Blue.ToSKColor()
            })
            {
                // Draw the background
                canvas.DrawImage(BackgroundImage, BackgroundRect, imgPaint);

                // Draw the lines and markers
                for (int i = 0; i < markers.Count; ++i)
                {
                    // Get marker
                    var mStart = markers[i];

                    // Do not draw line between start and end marker
                    if (i + 1 < markers.Count)
                    {
                        // Draw line first
                        var mEnd = markers[i + 1];
                        canvas.DrawLine(mStart.Centre, mEnd.Centre, linePaint);
                    }

                    // Draw maker
                    canvas.DrawCircle(mStart.Centre, MarkerSizeScaledUnits * 0.5f, markerPaint);
                }
            }
        }

        internal SKImage GetImageFromResource(string resource, Assembly resourceAssembly = null)
        {
            // Load in resource
            try
            {
                // Load textures in their native format
                if (resourceAssembly == null) resourceAssembly = Assembly.GetCallingAssembly();
                using (Stream stream = resourceAssembly.GetManifestResourceStream(resource))
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        stream.CopyTo(memStream);
                        return SKImage.FromEncodedData(memStream.ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"VPI: Failed to load texture {resource}: {e}");
                return null;
            }
        }

        protected override void OnTouch(SKTouchEventArgs e)
        {
            base.OnTouch(e);

            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:

                    // If we aren't dragging a marker then look for one to select
                    if (markerSelected == null)
                    {
                        // Look for marker within a radius away and select it
                        foreach (var m in markers)
                        {
                            if (SKPoint.Distance(m.Centre, e.Location) < MarkerSizeScaledUnits)
                            {
                                markerSelected = m;
                            }
                        }
                    }
                    break;

                case SKTouchAction.Moved:

                    // If we are dragging a marker
                    if (markerSelected != null)
                    {
                        markerSelected.MoveTo(e.Location);
                    }
                    break;

                case SKTouchAction.Released:

                    // Deselect the marker
                    markerSelected = null;
                    break;
            }

            e.Handled = true;
            InvalidateSurface();
        }
    }
}
