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
            WrappedBitmap a1w = WrappedBitmap.FromFile(@"..\..\..\..\sample-data\a1.png");
            WrappedBitmap a2w = WrappedBitmap.FromFile(@"..\..\..\..\sample-data\a2.png");
            WrappedBitmap a3w = WrappedBitmap.FromFile(@"..\..\..\..\sample-data\a3.png");
            //WrappedBitmap bw = WrappedBitmap.FromFile(@"..\..\..\..\sample-data\b.png");
            WrappedBitmap cw = WrappedBitmap.FromFile(@"..\..\..\..\sample-data\c.png");
            //WrappedBitmap dw = WrappedBitmap.FromFile(@"..\..\..\..\sample-data\d.png");
            //WrappedBitmap ew = WrappedBitmap.FromFile(@"..\..\..\..\sample-data\e.png");
            WrappedBitmap fw = WrappedBitmap.FromFile(@"..\..\..\..\sample-data\f.png");


            var charClassifier_1 = new Classifier<char>();
            charClassifier_1.Train(a1w, 'a', TuningParams.TrainingRounds);
            TrainSampleCharacters(charClassifier_1);


            var x = charClassifier_1.Classify(cw).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
            Console.WriteLine("Input (C) is likely to be these letters (most likely first): {0}", string.Join(", ", x.Take(4)));

            var y = charClassifier_1.Classify(fw).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
            Console.WriteLine("Input (F) is likely to be these letters (most likely first): {0}", string.Join(", ", y.Take(4)));

            var z = charClassifier_1.Classify(a1w).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
            Console.WriteLine("Input (A) is likely to be these letters (most likely first): {0}", string.Join(", ", z.Take(4)));

            var z1 = charClassifier_1.Classify(a2w).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
            Console.WriteLine("Input (A2) is likely to be these letters (most likely first): {0}", string.Join(", ", z1.Take(4)));

            var z2 = charClassifier_1.Classify(a3w).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);
            Console.WriteLine("Input (A3) is likely to be these letters (most likely first): {0}", string.Join(", ", z2.Take(4)));

            //new Recogniser<char>('A').DrawSamplePoints().Save(@"..\..\..\..\sample-data\hello world.bmp");

            
            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        /// <summary>
        /// Trains the classifier to recognise a range of letters based on the image from our 'sample-data' folder
        /// </summary>
        private static void TrainSampleCharacters(Classifier<char> classifier) {
            // Starting at 'b'. Our 'a' sample data has multiple samples
            for(char c = 'b'; c <= 'q'; c++) {
                TrainSampleCharacter(classifier, c);
            }
        }

        /// <summary>
        /// Trains the classifier to recognise a single letter based on an image from our 'sample-data' folder
        /// </summary>
        private static void TrainSampleCharacter(Classifier<char> classifier, char character) {
            WrappedBitmap wb = WrappedBitmap.FromFile($@"..\..\..\..\sample-data\{character}.png");
            Console.Write(character);
            classifier.Train(wb, character, TuningParams.TrainingRounds);
        }
    }
}
