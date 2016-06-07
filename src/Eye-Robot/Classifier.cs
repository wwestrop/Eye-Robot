using System.Collections.Generic;
using System.Linq;

namespace EyeRobot
{
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
                .Select(s => new ScorerScorePair(s, s.Score(inputData)))
                .AsParallel()
                .OrderByDescending(s => s.Score);

            // Having acquired the initial set of sampling pixels via sheer randomness, we'll work with 
            // the best we've got and try and massage them into something better. 
            var mutParams = new TuningParams.Mutation();
            var initialBestCandidates = initialGeneration.Take(100);            // TODO parameterise magic value (which was decided totally arbitrarily anyway)
            var initialBestCandidatesWithMutatedVariants = from p in initialBestCandidates
                                                           let mutatedOffspring = p.Scorer.MutateMany(mutParams)
                                                           let mutatedScores = from m in mutatedOffspring
                                                                               let mutatedScore = m.Score(inputData)
                                                                               select new ScorerScorePair(m, mutatedScore)
                                                           select new ParentChildScoreCollection(p, mutatedScores);
            
            var bestOftheMutantStrains = initialBestCandidatesWithMutatedVariants
                .SelectMany(x => x.Children)
                .OrderByDescending(s => s.Score)
                .First();

            // Compute some stats for these mutations. These stats are for interest only, they don't affect any computation. 
            Benchmarking.Mutation.AddDataPoints(initialBestCandidatesWithMutatedVariants);

            var rec = new Recogniser<TSymbol>(representedValue, bestOftheMutantStrains.Scorer);
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
    /// Tuple which groups a scorer with the value it assigns to a given input image. 
    /// Useful to avoid re-scoring the same image data multiple times. 
    /// </summary>
    internal class ScorerScorePair
    {
        public readonly Scorer Scorer;
        public readonly int Score;

        public ScorerScorePair(Scorer scorer, int score)
        {
            this.Scorer = scorer;
            this.Score = score;
        }
    }

    /// <summary>
    /// Tuple which groups a parent scorer with the mutated variants derived from it
    /// </summary>
    internal class ParentChildScoreCollection
    {
        public readonly ScorerScorePair Parent;
        public readonly IEnumerable<ScorerScorePair> Children;

        public ParentChildScoreCollection(ScorerScorePair parent, IEnumerable<ScorerScorePair> children)
        {
            this.Parent = parent;
            this.Children = children;
        }
    }

}
