namespace EyeRobot
{
    internal static class TuningParams
    {
        public const int PositiveRandOffset = 50;
        public const int NegativeRandOffset = 10;

        public const int PixelRandomChurn = 10;
        public const int PixelRandomSkewNumber = 10;
        public const int PixelRandomSkewOffset = 6;

        public const int MinPixels = 80;
        public const int MaxPixels = 120;

        public const int ImageSize = 32;

        public const uint TrainingRounds = 20000;
    }
}