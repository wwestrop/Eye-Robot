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
}
