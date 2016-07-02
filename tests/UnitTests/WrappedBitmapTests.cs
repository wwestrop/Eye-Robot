using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using EyeRobot;

namespace UnitTests {

    [TestClass]
    public class WrappedBitmapTests {
        
        private readonly Color DarkGrey = Color.FromArgb(127, 127, 127);
        private readonly Color LightGrey = Color.FromArgb(128, 128, 128);            // LightGrey is impercebtibly lighter than dark grey, but hey-ho, it's the edge case

        private const int X1 = 12;
        private const int Y1 = 27;

        private Bitmap _bitmap;
        private Graphics _graphics;


        [TestInitialize]
        public void TestInitialize() {
            _bitmap = new Bitmap(TuningParams.ImageSize, TuningParams.ImageSize);
            _graphics = Graphics.FromImage(_bitmap);

            // Blank out the initial canvas
            using(var brush = new SolidBrush(Color.White)) {
                _graphics.FillRectangle(brush, 0, 0, _bitmap.Width, _bitmap.Height);
            }
        }

        [TestCleanup]
        public void TestCleanup() {
            _graphics.Dispose();
            _bitmap.Dispose();
        }
        
        /// <summary>
        /// Asserts that WrappedBitmap copies the state of the input bitmap as at instantiation. 
        /// If the source bitmap later changes, WrappedBitmap is isolated from that. 
        /// </summary>
        [TestMethod]
        public void WrappedBitmap_Immutable() {
            WrappedBitmap wb = new WrappedBitmap(_bitmap);
            Assert.IsFalse(wb.IsPixelSet(X1, Y1));

            _bitmap.SetPixel(X1, Y1, Color.Black);
            Assert.IsFalse(wb.IsPixelSet(X1, Y1));
        }

        [TestMethod]
        public void Brightness_Less_Than_128() {
            _bitmap.SetPixel(X1, Y1, DarkGrey);
            WrappedBitmap wb = new WrappedBitmap(_bitmap);
            
            Assert.IsTrue(wb.IsPixelSet(X1, Y1));
        }

        [TestMethod]
        public void Brightness_Greater_Than_127() {
            _bitmap.SetPixel(X1, Y1, LightGrey);
            WrappedBitmap wb = new WrappedBitmap(_bitmap);
            
            Assert.IsFalse(wb.IsPixelSet(X1, Y1));
        }
        
        [TestMethod]
        public void Axes_Not_Swapped() {
            _bitmap.SetPixel(X1, Y1, DarkGrey);
            WrappedBitmap wb = new WrappedBitmap(_bitmap);

            Assert.IsFalse(wb.IsPixelSet(Y1, X1));              // Note, X and Y swapped around here. This pixel expected *NOT* to be set
        }

        [TestMethod]
        public void White_Pixel() {
            _bitmap.SetPixel(X1, Y1, Color.White);
            WrappedBitmap wb = new WrappedBitmap(_bitmap);

            Assert.IsFalse(wb.IsPixelSet(X1, Y1));
        }

        [TestMethod]
        public void Black_Pixel() {
            _bitmap.SetPixel(X1, Y1, Color.Black);
            WrappedBitmap wb = new WrappedBitmap(_bitmap);

            Assert.IsTrue(wb.IsPixelSet(X1, Y1));
        }
    }

}
