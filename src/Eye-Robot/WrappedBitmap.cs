using System.Drawing;

namespace EyeRobot
{
    /// <summary>
    /// Copies a monochrome bitmap into memory. Unlike <c>System.Drawing.Bitmap</c>, 
    /// pixels can be read from here by multiple threads simultaneously. 
    /// </summary>
    class WrappedBitmap
    {
        private readonly bool[,] _pixels;

        public WrappedBitmap(Bitmap input)
        {
            _pixels = new bool[input.Width, input.Height];

            for (int y = 0; y < input.Height; y++)
            {
                for (int x = 0; x < input.Width; x++)
                {
                    _pixels[x, y] = IsPixelSet(input, x, y);
                }
            }
        }

        /// <summary>
        /// We effectively treat the image as monochrome, a pixel is either below
        /// half-brightness (set), or above it (un-set). 
        /// </summary>
        private bool IsPixelSet(Bitmap input, int x, int y)
        {
            return input.GetPixel(x,y).GetBrightness() < 0.5f;
        }

        public bool IsPixelSet(int x, int y)
        {
            return _pixels[x, y];
        }
    }
}