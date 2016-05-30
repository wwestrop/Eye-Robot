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
}
