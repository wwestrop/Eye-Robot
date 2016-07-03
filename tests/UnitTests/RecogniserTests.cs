using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Drawing;
using System;

namespace EyeRobot.UnitTests {

    /// <summary>
    /// All a <c>Recogniser</c> does is to bind a <c>Scorer</c> to the symbol that it recognises.
    /// Thus these tests verify that calls are all deferred to the Scorer
    /// </summary>
    [TestClass]
    public class RecogniserTests {

        private IScorer _scorer;
        private Recogniser<int> _recogniser;

        [TestInitialize]
        public void TestInitialize() {
            _scorer = Substitute.For<IScorer>();
            _recogniser = new Recogniser<int>(42, _scorer);

            var img = new Bitmap(TuningParams.ImageSize, TuningParams.ImageSize);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_Null_Symbol_Not_Acceptable_For_Reference_Types() {
            new Recogniser<object>(null, _scorer);
            // ExpectedException attribute will catch
        }

        [TestMethod]
        public void Ctor_Default_Symbol_Acceptable_For_Value_Types() {
            // Null_Ctor_Symbol_Not_Acceptable tests that we prevent against null references 
            // For the case of structs, the default value may well be a valid thing to recognise (e.g. 0)
            // This test passes if no exception is thrown.
            new Recogniser<int>(0, _scorer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_Null_Scorer_Not_Acceptable() {
            new Recogniser<object>(new object(), null);
            // ExpectedException attribute will catch
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null_Input_Not_Acceptable() {
            _recogniser.Score(null);
            // ExpectedException attribute will catch
        }

        [TestMethod]
        public void Defers_Scoring_To_Scorer() {
            const int mockedScore = 555;
            WrappedBitmap wb = new WrappedBitmap(new Bitmap(1,1));
            _scorer.Score(null).ReturnsForAnyArgs(mockedScore);

            var score = _recogniser.Score(wb);

            _scorer.Received(1).Score(wb);
            Assert.AreEqual(mockedScore, score);
        }

    }

}
