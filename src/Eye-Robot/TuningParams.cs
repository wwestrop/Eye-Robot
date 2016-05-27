namespace EyeRobot
{
    internal static class TuningParams
    {
        public const int ImageSize = 32;
        public const uint TrainingRounds = 25000;

        public class Scoring
        {
            public readonly byte PositiveRandOffset = 50;
            public readonly byte NegativeRandOffset = 10;
        }

        public static class Generation
        {
            public const int MinPixels = 80;
            public const int MaxPixels = 120;
        }

        public class Mutation
        {
            public readonly int PixelRandomChurn = 10;
            public readonly int PixelRandomSkewNumber = 10;
            public readonly int PixelRandomSkewOffset = 6;
        }
    }
}