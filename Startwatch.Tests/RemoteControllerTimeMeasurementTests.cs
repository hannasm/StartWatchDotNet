using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StartwatchDiagnostics.Tests
{
    [TestClass]
    public class RemoteControllerTimeMeasurementTests : TestBase
    {
        public void Log(string msg, params object[] format)
        {
            Trace.WriteLine(string.Format(msg, format));
        }
        public void AssertStopwatch(long expectedTicks, Startwatch actual)
        {
            this.AssertStopwatch(expectedTicks, actual, null);
        }
        public void AssertStopwatch(long expectedTicks, Startwatch actual, string message, params object[] fmt)
        {
            if (expectedTicks + 10000 < actual.ElapsedTicks || expectedTicks - 10000 > actual.ElapsedTicks)
            {
                var msg = "Within 10,0000 micro seconds.";
                if (message != null && fmt != null)
                {
                    msg += string.Format(message, fmt);
                }
                Assert.AreEqual(expectedTicks, actual.ElapsedTicks, msg);
            }
        }

        [TestMethod]
        public void MeasureStandard()
        {
            Startwatch measure = new Startwatch();
            var assertComparisonTimer = Stopwatch.StartNew();
            var loopTimer = Stopwatch.StartNew();
            using (measure.CreateController()) {
                    while (loopTimer.ElapsedTicks < Stopwatch.Frequency)
                    {
                        ;
                    }
            }
            assertComparisonTimer.Stop();
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }
        [TestMethod]
        public void MeasureStandardWithOverplay()
        {
            Startwatch measure = new Startwatch();
            var assertComparisonTimer = Stopwatch.StartNew();
            var loopTimer = Stopwatch.StartNew();

            while (loopTimer.ElapsedTicks < 2 * Stopwatch.Frequency)
            {
                if (loopTimer.ElapsedTicks > Stopwatch.Frequency)
                {
                    using (measure.CreateController()) {} // this is kind of contrived...
                    assertComparisonTimer.Stop();
                }
            }
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }

        [TestMethod]
        public void MeasureChild()
        {
            var assertComparisonTimer = Stopwatch.StartNew();
            var parent = new Startwatch();
            Startwatch measure = parent.CreateChild();
            var loopTimer = Stopwatch.StartNew();
            using (measure.CreateController()) {
                while (loopTimer.ElapsedTicks < Stopwatch.Frequency)
                {
                    ;
                }
            }
            assertComparisonTimer.Stop();
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }
        [TestMethod]
        public void MeasureChildWithOverplay()
        {
            var assertComparisonTimer = Stopwatch.StartNew();
            var parent = new Startwatch();
            Startwatch measure = parent.CreateChild();
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < 2 * Stopwatch.Frequency)
            {
                if (loopTimer.ElapsedTicks > Stopwatch.Frequency)
                {
                    measure.Stop(); assertComparisonTimer.Stop();
                }
            }
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }


        [TestMethod]
        public void MeasureSibling()
        {
            var first = new Startwatch();
            Startwatch measure = first.CreateSibling();
            var assertComparisonTimer = Stopwatch.StartNew();
            first.Stop();
            var loopTimer = Stopwatch.StartNew();
            using (measure.CreateController()) {
                while (loopTimer.ElapsedTicks < Stopwatch.Frequency)
                {
                    ;
                }
            }
            assertComparisonTimer.Stop();
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }
        [TestMethod]
        public void MeasureSiblingWithOverplay()
        {
            var first = new Startwatch();
            Startwatch measure = first.CreateSibling();
            var assertComparisonTimer = Stopwatch.StartNew();
            first.Stop();
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < 2 * Stopwatch.Frequency)
            {
                if (loopTimer.ElapsedTicks > Stopwatch.Frequency)
                {
                    using (measure.CreateController()) {} // this is kind of contrived
                    assertComparisonTimer.Stop();
                }
            }
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }


        [TestMethod]
        public void MeasureLastSibling01()
        {
            var first = new Startwatch();
            var second = first.CreateChild();
            Startwatch measure = second.CreateLastSibling(first);
            var assertComparisonTimer = Stopwatch.StartNew();
            second.Stop();
            var loopTimer = Stopwatch.StartNew();
            using (measure.CreateController()) {
                while (loopTimer.ElapsedTicks < Stopwatch.Frequency)
                {
                    ;
                }
            }
            assertComparisonTimer.Stop();
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }
        [TestMethod]
        public void MeasureLastSibling02()
        {
            var first = new Startwatch();
            var second = first.CreateChild();
            Startwatch measure = second.CreateLastSibling(first);
            var assertComparisonTimer = Stopwatch.StartNew();
            second.Stop();
            var loopTimer = Stopwatch.StartNew();
            using (first.CreateController()) {
                while (loopTimer.ElapsedTicks < Stopwatch.Frequency)
                {
                    ;
                }
            // checking first.stop() stops measure
            }
            assertComparisonTimer.Stop();
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }
        [TestMethod]
        public void MeasureLastSiblingWithOverplay01()
        {
            var first = new Startwatch();
            var second = first.CreateChild();
            Startwatch measure = second.CreateLastSibling(first);
            var assertComparisonTimer = Stopwatch.StartNew();
            second.Stop();
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < 2 * Stopwatch.Frequency)
            {
                if (loopTimer.ElapsedTicks > Stopwatch.Frequency)
                {
                    using (measure.CreateController()) {} // this is kind of contrived...
                    assertComparisonTimer.Stop();
                }
            }
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }

        [TestMethod]
        public void MeasureLastSiblingWithOverplay()
        {
            var first = new Startwatch();
            var second = first.CreateChild();
            Startwatch measure = second.CreateLastSibling(first);
            var assertComparisonTimer = Stopwatch.StartNew();
            second.Stop();
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < 2 * Stopwatch.Frequency)
            {
                if (loopTimer.ElapsedTicks > Stopwatch.Frequency)
                {
                    // checking first.stop() stops measure
                    using (first.CreateController()) { } // this is kind of contrived...
                    assertComparisonTimer.Stop();
                }
            }
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }
    }
}
