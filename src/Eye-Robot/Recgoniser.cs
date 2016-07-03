using System;
using System.Drawing;

namespace EyeRobot
{
    /// <summary>
    /// Recognises a single symbol and produces a score for an input image indicating 
    /// how likely that image is to be the symbol that we are trained to recognise. 
    /// </summary>
    internal class Recogniser<TSymbol>
    {
        private readonly IScorer _scorer;

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
            if (inputData == null) throw new ArgumentNullException(nameof(inputData));

            return _scorer.Score(inputData);
        }

        /// <param name="symbol">The symbol which the given scorer is optimised for</param>
        /// <param name="scorer">An object which is trained to recognise the symbol specified by the <c>symbol</c> param</param>
        public Recogniser(TSymbol symbol, IScorer scorer)
        {
            if (symbol == null) throw new ArgumentNullException(nameof(symbol));
            if (scorer == null) throw new ArgumentNullException(nameof(scorer));

            this.Symbol = symbol;
            _scorer = scorer;
        }        
    }
}
