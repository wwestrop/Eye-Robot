using System.Collections.Generic;
using System.Linq;

namespace EyeRobot.Benchmarking
{
    /// <summary>
    /// Class for measuring how effective a mutated Scorer is compared with the Scorer it originated from. 
    /// </summary>
    internal static class Mutation
    {
        private static readonly List<ParentOffspingComparisonEntry> _parentOffspringComparisons = new List<ParentOffspingComparisonEntry>();

        public static void AddDataPoints(IEnumerable<ParentChildScoreCollection> dataPoints)
        {
            foreach(var f2 in dataPoints)
            {
                // Compare parents against their children, on an individual, per-parent level
                var comparisonStats = CompareOffspringAgainstParent(f2);
                _parentOffspringComparisons.Add(comparisonStats);
            }
            
            // Then compare the set of parents against their children aggregated as a whole

            // biggest improvement (abs/rel) overall
            var b = _parentOffspringComparisons.OrderByDescending(z => z.AbsoluteBestImprovement).First().AbsoluteBestImprovement;
            var c = _parentOffspringComparisons.OrderByDescending(z => z.RelativeBestImprovement).First().RelativeBestImprovement;

            // biggest regression (abs/rel) overall
            var d = _parentOffspringComparisons.OrderBy(z => z.AbsoluteWorstRegression).First().AbsoluteWorstRegression;
            var e = _parentOffspringComparisons.OrderBy(z => z.RelativeWorstRegression).First().RelativeWorstRegression;

            // average improvment (abs/rel) overall
            var f = (double)_parentOffspringComparisons.Sum(z => z.AbsoluteBestImprovement) / _parentOffspringComparisons.Count;
            var g = (double)_parentOffspringComparisons.Sum(z => z.RelativeBestImprovement) / _parentOffspringComparisons.Count;

            // average regression (abs/rel) overall
            var h = (double)_parentOffspringComparisons.Sum(z => z.AbsoluteWorstRegression) / _parentOffspringComparisons.Count;
            var i = (double)_parentOffspringComparisons.Sum(z => z.RelativeWorstRegression) / _parentOffspringComparisons.Count;
        }

        /// <summary>
        /// Analyses the mutated variants derived from a parent and produces some statistics to
        /// measure how much better or worse the children perform than their progenitor. 
        /// Appends the results to <c>_parentOffspringComparisons</c>
        /// </summary>
        private static ParentOffspingComparisonEntry CompareOffspringAgainstParent(ParentChildScoreCollection parentScorerWithChildren)
        {
            var parentScore = parentScorerWithChildren.Parent.Score;
            var bestChild = parentScorerWithChildren.Children.OrderByDescending(c => c.Score).First();
            var worstChild = parentScorerWithChildren.Children.OrderBy(c => c.Score).First();

            var absBest = bestChild.Score - parentScore;
            var absWorst = worstChild.Score - parentScore;

            var relBest = ((double)absBest / parentScore) * 100;
            var relWorst = ((double)absWorst / parentScore) * 100;

            return new ParentOffspingComparisonEntry(absBest, relBest, absWorst, relWorst);
        }

        /// <summary>
        /// Data structure holding statistics which compare a parent to its derived offspring (post mutation step)
        /// </summary>
        private class ParentOffspingComparisonEntry
        {
            public readonly int AbsoluteBestImprovement;
            public readonly double RelativeBestImprovement;
            public readonly int AbsoluteWorstRegression;
            public readonly double RelativeWorstRegression;

            public ParentOffspingComparisonEntry(int absBest, double relativeBest, int absWorst, double relativeWorst)
            {
                this.AbsoluteBestImprovement = absBest;
                this.RelativeBestImprovement = relativeBest;
                this.AbsoluteWorstRegression = absWorst;
                this.RelativeWorstRegression = relativeWorst;
            }
        }
    }
}
