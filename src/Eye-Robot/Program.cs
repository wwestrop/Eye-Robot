﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EyeRobot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WrappedBitmap a1w;
            WrappedBitmap a2w;
            WrappedBitmap a3w;
            WrappedBitmap bw;
            WrappedBitmap cw;
            WrappedBitmap dw;
            WrappedBitmap ew;
            WrappedBitmap fw;
            using (var a1 = Bitmap.FromFile(@"..\..\..\..\sample-data\a1.png") as Bitmap)
            using (var a2 = Bitmap.FromFile(@"..\..\..\..\sample-data\a2.png") as Bitmap)
            using (var a3 = Bitmap.FromFile(@"..\..\..\..\sample-data\a3.png") as Bitmap)
            using (var b = Bitmap.FromFile(@"..\..\..\..\sample-data\b.png") as Bitmap)
            using (var c = Bitmap.FromFile(@"..\..\..\..\sample-data\c.png") as Bitmap)
            using (var d = Bitmap.FromFile(@"..\..\..\..\sample-data\d.png") as Bitmap)
            using (var e = Bitmap.FromFile(@"..\..\..\..\sample-data\e.png") as Bitmap)
            using (var f = Bitmap.FromFile(@"..\..\..\..\sample-data\f.png") as Bitmap)
            {
                a1w = new WrappedBitmap(a1);
                a2w = new WrappedBitmap(a2);
                a3w = new WrappedBitmap(a3);
                bw = new WrappedBitmap(b);
                cw = new WrappedBitmap(c);
                dw = new WrappedBitmap(d);
                ew = new WrappedBitmap(e);
                fw = new WrappedBitmap(f);
            }


            var charClassifier_1 = new Classifier<char>();
            charClassifier_1.Train(a1w, 'a', TuningParams.TrainingRounds);
            
            charClassifier_1.Train(bw, 'b', TuningParams.TrainingRounds);
            charClassifier_1.Train(cw, 'c', TuningParams.TrainingRounds);
            charClassifier_1.Train(dw, 'd', TuningParams.TrainingRounds);
            charClassifier_1.Train(ew, 'e', TuningParams.TrainingRounds);
            charClassifier_1.Train(fw, 'f', TuningParams.TrainingRounds);


            var x = charClassifier_1.Classify(cw).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
            Console.WriteLine("Input (C) is likely to be these letters (most likely first): {0}", string.Join(", ", x.Take(10)));

            var y = charClassifier_1.Classify(fw).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
            Console.WriteLine("Input (F) is likely to be these letters (most likely first): {0}", string.Join(", ", y.Take(10)));

            var z = charClassifier_1.Classify(a1w).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
            Console.WriteLine("Input (A) is likely to be these letters (most likely first): {0}", string.Join(", ", z.Take(10)));

            var z1 = charClassifier_1.Classify(a2w).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
            Console.WriteLine("Input (A2) is likely to be these letters (most likely first): {0}", string.Join(", ", z1.Take(10)));

            var z2 = charClassifier_1.Classify(a3w).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
            Console.WriteLine("Input (A3) is likely to be these letters (most likely first): {0}", string.Join(", ", z2.Take(10)));

            //new Recogniser<char>('A').DrawSamplePoints().Save(@"..\..\..\..\sample-data\hello world.bmp");

            
            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
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

    internal class Classifier<TSymbol>
    {
        private readonly Dictionary<TSymbol, Recogniser<TSymbol>> _recognisers = new Dictionary<TSymbol, Recogniser<TSymbol>>();

        /// <summary>
        /// A collection of every symbol that this classifier is trained to recognise
        /// </summary>
        public IEnumerable<TSymbol> KnownSymbols => _recognisers.Select(r => r.Key);

        /// <summary>
        /// Returns a list of symbols the classifier recognises, paired
        /// with the score that the given image matches that symbol
        /// </summary>
        public Dictionary<TSymbol, int> Classify(WrappedBitmap input)
        {
            return _recognisers
                .Select(r => new
                {
                    Symbol = r.Key,
                    Score = r.Value.Score(input)
                })
                .ToDictionary(kvp => kvp.Symbol, kvp => kvp.Score);
        }

        /// <summary>
        /// Trains the model to recognise the specified input as the specified symbol
        /// </summary>
        public void Train(WrappedBitmap inputData, TSymbol representedValue, uint rounds)
        {
            var initialGeneration = Enumerable.Range(0, (int)rounds)
                .Select(i => new Scorer(new TuningParams.Scoring()))                // TODO nested constructors is smelly
                .Select(s => new
                {
                    Scorer = s,
                    Score = s.Score(inputData)
                })
                .AsParallel()
                .OrderByDescending(s => s.Score);
            
            // Having acquired the initial set of sampling pixels via sheer randomness, we'll work with 
            // the best we've got and try and massage them into something better. 
            var mutParams = new TuningParams.Mutation();
            var initialBestCandidates = initialGeneration.Take(100);            // TODO parameterise magic value (which was decided totally arbitrarily anyway)
            var initialBestCandidatesWithMutatedVariants = initialBestCandidates
                .Select(s => new {
                    Parent = s.Scorer,
                    Children = s.Scorer.MutateMany(mutParams)
                });
            var bestOftheMutantStrains = initialBestCandidatesWithMutatedVariants
                .SelectMany(x => x.Children)
                .Select(s => new
                {
                    Scorer = s,
                    Score = s.Score(inputData)
                })
                .OrderByDescending(s => s.Score)
                .Select(x => x.Scorer)
                .First();
            
            var rec = new Recogniser<TSymbol>(representedValue, bestOftheMutantStrains);
            RegisterRecogniser(rec);
        }

        /// <summary>
        /// Adds a recogniser to the collection, replacing it if it already exists
        /// </summary>
        private void RegisterRecogniser(Recogniser<TSymbol> recogniser)
        {
            // TODO could change to Dictionary<TSymbol, List<Recogniser<TSymbol>>> for training multiple inputs for the same symbol??
            _recognisers[recogniser.Symbol] = recogniser;
        }
    }

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

    internal class Scorer
    {
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

        public IEnumerable<Scorer> MutateMany(TuningParams.Mutation tuningParams)
        {
            return Enumerable.Range(0, tuningParams.SpawnedDescendants)
                .Select(i => Mutate(tuningParams));
        }

        /// <summary>
        /// Randomises the sampled pixels, but keeps them relatively close to their existing locations. 
        /// </summary>
        public Scorer Mutate(TuningParams.Mutation tuningParams)
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
