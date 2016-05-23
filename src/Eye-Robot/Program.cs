using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EyeRobot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var a1 = Bitmap.FromFile(@"..\..\..\..\sample-data\a1.png") as Bitmap;           // TODO bitmaps are disposable
            var charClassifier_1 = new Classifier<char>();
            charClassifier_1.Train(a1, 'a', TuningParams.TrainingRounds);

            var a2 = Bitmap.FromFile(@"..\..\..\..\sample-data\a2.png") as Bitmap;
            var a3 = Bitmap.FromFile(@"..\..\..\..\sample-data\a3.png") as Bitmap;

            var b = Bitmap.FromFile(@"..\..\..\..\sample-data\b.png") as Bitmap;
            charClassifier_1.Train(b, 'b', TuningParams.TrainingRounds);
            var c = Bitmap.FromFile(@"..\..\..\..\sample-data\c.png") as Bitmap;
            charClassifier_1.Train(c, 'c', TuningParams.TrainingRounds);
            var d = Bitmap.FromFile(@"..\..\..\..\sample-data\d.png") as Bitmap;
            charClassifier_1.Train(d, 'd', TuningParams.TrainingRounds);
            var e = Bitmap.FromFile(@"..\..\..\..\sample-data\e.png") as Bitmap;
            charClassifier_1.Train(e, 'e', TuningParams.TrainingRounds);
            var f = Bitmap.FromFile(@"..\..\..\..\sample-data\f.png") as Bitmap;
            charClassifier_1.Train(f, 'f', TuningParams.TrainingRounds);


            var x = charClassifier_1.Classify(c).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
            Console.WriteLine("Input (C) is likely to be these letters (most likely first): {0}", string.Join(", ", x.Take(10)));

            var y = charClassifier_1.Classify(f).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
            Console.WriteLine("Input (F) is likely to be these letters (most likely first): {0}", string.Join(", ", y.Take(10)));

            var z = charClassifier_1.Classify(a1).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
            Console.WriteLine("Input (A) is likely to be these letters (most likely first): {0}", string.Join(", ", z.Take(10)));

            var z1 = charClassifier_1.Classify(a2).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
            Console.WriteLine("Input (A2) is likely to be these letters (most likely first): {0}", string.Join(", ", z1.Take(10)));

            var z2 = charClassifier_1.Classify(a3).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
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
        private readonly List<Recogniser<TSymbol>> _recognisers = new List<Recogniser<TSymbol>>();

        /// <summary>
        /// A collection of every symbol that this classifier is trained to recognise
        /// </summary>
        public IEnumerable<TSymbol> KnownSymbols => _recognisers.Select(r => r.Symbol);

        /// <summary>
        /// Returns a list of symbols the classifier recognises, paired
        /// with the score that the given image matches that symbol
        /// </summary>
        public Dictionary<TSymbol, int> Classify(Bitmap input)
        {
            return _recognisers
                .Select(r => new
                {
                    Symbol = r.Symbol,
                    Score = r.Score(input)
                })
                .ToDictionary(kvp => kvp.Symbol, kvp => kvp.Score);
        }

        /// <summary>
        /// Trains the model to recognise the specified input as the specified symbol
        /// </summary>
        public void Train(Bitmap inputData, TSymbol representedValue, uint rounds)
        {
            var rec = GetRecogniser(representedValue);

            var highestScoringPixelWeights = Enumerable.Range(0, (int) rounds)                 //.AsParallel()  -- Bitmap reading is not thread safe !??!?!??
                .Select(i => new Recogniser<TSymbol>(representedValue))
                .Select(r => new
                {
                    Recogniser = r,
                    Score = r.Score(inputData)
                })
                .OrderByDescending(s => s.Score)
                .First();
            
            // TODO: don't like this bit. rework
            _recognisers.Remove(rec);
            _recognisers.Add(highestScoringPixelWeights.Recogniser);
        }

        // Get the recogniser for this symbol, or create one if we don't already have one
        private Recogniser<TSymbol> GetRecogniser(TSymbol forSymbol)
        {
            var recogniser = _recognisers.SingleOrDefault(r => forSymbol.Equals(r.Symbol));
            if (recogniser == null)
            {
                recogniser = new Recogniser<TSymbol>(forSymbol);
                _recognisers.Add(recogniser);
            }

            return recogniser;
        }
    }
    
    /// <summary>
    /// Recognises a single symbol and produces a score for an input image indicating 
    /// how likely that image is to be the symbol that we are trained to recognise. 
    /// </summary>
    internal class Recogniser<TSymbol>
    {
        private static readonly Random _randomNumGenerator = new Random();

        /// <summary>
        /// The list of pixels we expect to be set in order to consider a given
        /// input image as the symbol we are searching for
        /// </summary>
        public List<Point> SampledPixels { get; set; }

        /// <summary>
        /// The symbol which this object is trained to recognise
        /// </summary>
        public TSymbol Symbol { get; set; }

        /// <summary>
        /// The amount that will be subtracted from the score when one of our expected 
        /// pixels isn't present on the input image
        /// </summary>
        public byte NegativeScoreOffset { get; set; }

        /// <summary>
        /// The amount that will be added to the score when one of our expected 
        /// pixels is present on the input image
        /// </summary>
        public byte PositiveScoreOffset { get; set; }

        /// <summary>
        /// Calculates a score for the given input data. The higher the score, the more 
        /// likely that the input image represents the value of <c>Symbol</c>.
        /// </summary>
        public int Score(Bitmap inputData)
        {
            int score = 0;
            foreach (var point in this.SampledPixels)
            {
                var pixel = inputData.GetPixel(point.X, point.Y);
                if (IsPixelSet(pixel))
                {
                    score += PositiveScoreOffset;
                }
                else
                {
                    score = Math.Min(0, score - NegativeScoreOffset);
                }
            }

            return score;
        }

        public Recogniser(TSymbol symbol)
        {
            this.Symbol = symbol;

            PositiveScoreOffset = (byte) _randomNumGenerator.Next(TuningParams.PositiveRandOffset);
            NegativeScoreOffset = (byte) _randomNumGenerator.Next(TuningParams.NegativeRandOffset);
            
            RandomisePixels();
        }

        /// <summary>
        /// Randomly initialises the locations of the pixels that will be sampled
        /// </summary>
        private void RandomisePixels()
        {
            int numPixels = TuningParams.MinPixels + _randomNumGenerator.Next(TuningParams.MaxPixels - TuningParams.MinPixels);

            this.SampledPixels = new List<Point>();
            for (int i = 0; i < numPixels; i++)
            {
                // NOTE - it's possible to get duplicates, in which case the same pixel is counted twice
                // TODO check for off-by-ones (can we hit the image edges?)
                this.SampledPixels.Add(
                    new Point(
                        _randomNumGenerator.Next(TuningParams.ImageSize),
                        _randomNumGenerator.Next(TuningParams.ImageSize)
                    )
                );
            }
        }

        /// <summary>
        /// We effectively treat the image as monochrome, a pixel is either below
        /// half-brightness (set), or above it (un-set). 
        /// </summary>
        private bool IsPixelSet(Color color)
        {
            return color.GetBrightness() < 0.5f;
        }

        /// <summary>
        /// Outputs a bitmap showing visaully which pixels this recogniser will sample from
        /// </summary>
        public Bitmap DrawSamplePoints()
        {
            var sampleMap = new Bitmap(TuningParams.ImageSize, TuningParams.ImageSize/*, PixelFormat.Format1bppIndexed*/);
            foreach(var point in this.SampledPixels)
            {
                sampleMap.SetPixel(point.X, point.Y, Color.Black);
            }

            return sampleMap;
        }
        
        /// <summary>
        /// Randomises the sampled pixels, but keeps them relatively close to their existing locations. 
        /// </summary>
        public void Mutate()
        {
            int n = _randomNumGenerator.Next(TuningParams.PixelRandomChurn);
            for(int i = 0 ; i < n; i++)
            {
                // Remove one of the pixels at random
                this.SampledPixels.RemoveAt(_randomNumGenerator.Next(TuningParams.ImageSize));
            }

            // Skew some of the remaining ones
            n = _randomNumGenerator.Next(TuningParams.PixelRandomSkewNumber);
            for(int i = 0; i < n; i++)
            {
                // TODO the same pixels could be skewed more than once (does that even matter?)
                int pixIndex = _randomNumGenerator.Next(this.SampledPixels.Count - 1);
                this.SampledPixels[pixIndex] = new Point(
                    this.SampledPixels[pixIndex].X + TuningParams.PixelRandomSkewOffset - TuningParams.PixelRandomSkewOffset / 2, 
                    this.SampledPixels[pixIndex].Y + TuningParams.PixelRandomSkewOffset - TuningParams.PixelRandomSkewOffset / 2);
            }

            // Add some more random new ones (approx the same number to replace those that were removed or "churned")
            n = _randomNumGenerator.Next(TuningParams.PixelRandomChurn);
            for (int i = 0; i < n; i++)
            {
                // NOTE - it's possible to get duplicates, in which case the same pixel is counted twice
                // TODO check for off-by-ones (can we hit the image edges?)
                this.SampledPixels.Add(
                    new Point(
                        _randomNumGenerator.Next(TuningParams.ImageSize),
                        _randomNumGenerator.Next(TuningParams.ImageSize)
                    )
                );
            }
            
            // modify the negative/positive offset randomly
            PositiveScoreOffset = (byte)(PositiveScoreOffset + _randomNumGenerator.Next(TuningParams.PositiveRandOffset) - TuningParams.PositiveRandOffset / 2);
            NegativeScoreOffset = (byte)(NegativeScoreOffset + _randomNumGenerator.Next(TuningParams.NegativeRandOffset) - TuningParams.NegativeRandOffset / 2);
        }
    }
}
