namespace EyeRobot
{
    internal static class TuningParams
    {
        public const int ImageSize = 32;
        public const uint TrainingRounds = 25000;

        public class Scoring
        {
            /// <summary>
            /// The amount that will be added to the score when one of our expected 
            /// pixels is present on the input image
            /// </summary>
            public readonly byte PositiveScoreOffset = 50;

            /// <summary>
            /// The amount that will be subtracted from the score when one of our expected 
            /// pixels isn't present on the input image
            /// </summary>
            public readonly byte NegativeScoreOffset = 10;
        }

        public static class Generation
        {
            public const int MinPixels = 80;
            public const int MaxPixels = 120;
        }

        public class Mutation
        {
            /// <summary>
            /// The number of sampled pixels in a mutated generation which are totally 
            /// replaced in-full with totally different pixels
            /// </summary>
            public readonly int PixelRandomChurn = 10;

            /// <summary>
            /// The number of pixels in a mutated generation which are randomly offset
            /// from their current location
            /// </summary>
            public readonly int PixelRandomSkewNumber = 10;

            /// <summary>
            /// For the randomly moved pixels in a mutated generation, specifies
            /// the maximum distance the pixels might move in either direction
            /// </summary>
            public readonly int PixelRandomSkewOffset = 16;

            public readonly int SpawnedDescendants = 300;
        }
    }
}