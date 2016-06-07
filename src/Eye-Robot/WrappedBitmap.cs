using System.Drawing;

namespace EyeRobot
{
    /// <summary>
    /// Copies a monochrome bitmap into memory. Unlike <c>System.Drawing.Bitmap</c>, 
    /// pixels can be read from here by multiple threads simultaneously. 
    /// </summary>
    internal class WrappedBitmap
    {
        private readonly bool[,] _pixels;

        /// <summary>
        /// Initialises a new <c>WrappedBitmap</c> based upon the contents of an image file
        /// </summary>
        public static WrappedBitmap FromFile(string sourceFile)
        {
            using (var imageData = Image.FromFile(sourceFile) as Bitmap)
            {
                return new WrappedBitmap(imageData);
            }
        }

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

        /// <summary>
        /// Gets input images into a consistent format for recognition or training 
        /// so we can compare like-for-like (involves trimming whitespace and 
        /// making a consistent size, could also involve correcting colours and rotating/deskewing)
        /// </summary>
        private static Bitmap NormaliseInput(Bitmap input)
        {
            // TODO: implement
            return input;
        }
    }
}