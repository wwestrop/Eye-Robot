using System.Drawing;

namespace EyeRobot
{
    /// <summary>
    /// Recognises a single symbol and produces a score for an input image indicating 
    /// how likely that image is to be the symbol that we are trained to recognise. 
    /// </summary>
    internal class Recogniser<TSymbol>
    {
        private readonly Scorer _scorer;

        /// <summary>
        /// The symbol which this object is trained to recognise
        /// </summary>
        public TSymbol Symbol { get; private set; }

        /// <summary>
        /// Calculates a score for the given input data. The higher the score, the more 
        /// likely that the input image represents the value of <c>Symbol</c>.
        /// </summary>
        public int Score(WrappedBitmap inputData)
        {
            return _scorer.Score(inputData);
        }

        /// <param name="symbol">The symbol which the given scorer is optimised for</param>
        /// <param name="scorer">An object which is trained to recognise the symbol specified by the <c>symbol</c> param</param>
        public Recogniser(TSymbol symbol, Scorer scorer)
        {
            this.Symbol = symbol;
            _scorer = scorer;
        }

        /// <summary>
        /// Outputs a bitmap showing visaully which pixels will be sampled
        /// </summary>
        public Bitmap DrawSampledPixels()
        {
            return _scorer.DrawSampledPixels();
        }
    }
}
