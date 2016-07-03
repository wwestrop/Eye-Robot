using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EyeRobot
{
    internal interface IScorer {
        int Score(WrappedBitmap inputData);
        IScorer Mutate(TuningParams.Mutation tuningParams);
        IEnumerable<IScorer> MutateMany(TuningParams.Mutation tuningParams);
    }

    /// <summary>
    /// Assigns a score to a given image, by sampling a specific subset of set of pixels
    /// </summary>
    internal class Scorer : IScorer {
        private static readonly Random _randomNumGenerator = new Random();
        private readonly TuningParams.Scoring _tuningParams;
        private readonly List<Point> _pixelsToSample;

        /// <summary>
        /// Calculates a score for the given input data. The higher the score, the more 
        /// likely that the input image represents the value of <c>Symbol</c>.
        /// </summary>
        public int Score(WrappedBitmap inputData)
        {
            int score = 0;
            foreach (var point in _pixelsToSample)
            {
                if (inputData.IsPixelSet(point.X, point.Y))
                {
                    score += _tuningParams.PositiveScoreOffset;
                }
                else
                {
                    score -= _tuningParams.NegativeScoreOffset;
                }
            }

            return score;
        }

        /// <summary>
        /// Outputs a bitmap showing visaully which pixels will be sampled
        /// </summary>
        public Bitmap DrawSampledPixels()
        {
            var sampleMap = new Bitmap(TuningParams.ImageSize, TuningParams.ImageSize/*, PixelFormat.Format1bppIndexed*/);
            foreach (var point in _pixelsToSample)
            {
                sampleMap.SetPixel(point.X, point.Y, Color.Black);
            }

            return sampleMap;
        }

        /// <summary>
        /// Generate a random list of pixels to be sampled
        /// </summary>
        private List<Point> RandomisePixels()
        {
            int numPixels = TuningParams.Generation.MinPixels + _randomNumGenerator.Next(TuningParams.Generation.MaxPixels - TuningParams.Generation.MinPixels);

            var randomPixels = new List<Point>();
            for (int i = 0; i < numPixels; i++)
            {
                // NOTE - it's possible to get duplicates, in which case the same pixel is counted twice
                // TODO check for off-by-ones (can we hit the image edges?)
                randomPixels.Add(
                    new Point(
                        _randomNumGenerator.Next(TuningParams.ImageSize),
                        _randomNumGenerator.Next(TuningParams.ImageSize)
                    )
                );
            }

            return randomPixels;
        }

        public IEnumerable<IScorer> MutateMany(TuningParams.Mutation tuningParams)
        {
            return Enumerable.Range(0, tuningParams.SpawnedDescendants)
                .Select(i => Mutate(tuningParams));
        }

        /// <summary>
        /// Randomises the sampled pixels, but keeps them relatively close to their existing locations. 
        /// </summary>
        public IScorer Mutate(TuningParams.Mutation tuningParams)
        {
            var newPixels = new List<Point>(_pixelsToSample);

            int n = _randomNumGenerator.Next(tuningParams.PixelRandomChurn);
            for (int i = 0; i < n; i++)
            {
                // Remove one of the pixels at random
                newPixels.RemoveAt(_randomNumGenerator.Next(TuningParams.ImageSize));
            }

            // Skew some of the remaining ones
            n = _randomNumGenerator.Next(tuningParams.PixelRandomSkewNumber);
            for (int i = 0; i < n; i++)
            {
                // TODO the same pixels could be skewed more than once (does that even matter?)
                int xOffset = _randomNumGenerator.Next(tuningParams.PixelRandomSkewOffset) - tuningParams.PixelRandomSkewOffset;
                int yOffset = _randomNumGenerator.Next(tuningParams.PixelRandomSkewOffset) - tuningParams.PixelRandomSkewOffset;
                int pixIndex = _randomNumGenerator.Next(newPixels.Count - 1);
                newPixels[pixIndex] = ConstrainPixelToImage(
                    newPixels[pixIndex].X + xOffset,
                    newPixels[pixIndex].Y + yOffset);
            }

            // Add some more random new ones (approx the same number to replace those that were removed or "churned")
            n = _randomNumGenerator.Next(tuningParams.PixelRandomChurn);
            for (int i = 0; i < n; i++)
            {
                // NOTE - it's possible to get duplicates, in which case the same pixel is counted twice
                // TODO check for off-by-ones (can we hit the image edges?)
                newPixels.Add(
                    new Point(
                        _randomNumGenerator.Next(TuningParams.ImageSize),
                        _randomNumGenerator.Next(TuningParams.ImageSize)
                    )
                );
            }

            // modify the negative/positive offset randomly
            //_positiveScoreOffset = (byte)(_positiveScoreOffset + _randomNumGenerator.Next(tuningParams.PositiveRandOffset) - TuningParams.PositiveRandOffset / 2);
            //_negativeScoreOffset = (byte)(_negativeScoreOffset + _randomNumGenerator.Next(TuningParams.Scoring.NegativeRandOffset) - TuningParams.NegativeRandOffset / 2);
            // TODO keeping these params the same as our parent for now, and only randomoising on the sampled pixels. the randomisation of varying parameters could go on forever!!!

            return new Scorer(_tuningParams, newPixels);
        }

        /// <summary>
        /// Caps the given x and y co-ordinates to make sure they fit inside the image
        /// </summary>
        private Point ConstrainPixelToImage(int x, int y)
        {
            x = Math.Max(0, x);
            x = Math.Min(TuningParams.ImageSize, x);

            y = Math.Max(0, y);
            y = Math.Min(TuningParams.ImageSize, y);

            return new Point(x, y);
        }

        /// <summary>
        /// Intitialises randomly
        /// </summary>
        public Scorer(TuningParams.Scoring tuningParams)
        {
            _tuningParams = tuningParams;
            _pixelsToSample = RandomisePixels();
        }

        public Scorer(TuningParams.Scoring tuningParams, List<Point> pixelsToSample)         // TODO here, the caller is providing us a list. But it, nor its contents are immutable. Could we force that somehow???
        {
            _tuningParams = tuningParams;
            _pixelsToSample = pixelsToSample;
        }
    }
}
