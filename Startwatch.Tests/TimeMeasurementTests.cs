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
    public class TimeMeasurementTests : TestBase
    {
        [TestMethod]
        public void MeasureStandard()
        {
            Startwatch measure = new Startwatch();
            var assertComparisonTimer = Stopwatch.StartNew();
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < Stopwatch.Frequency)
            {
                ;
            }
            measure.Stop(); assertComparisonTimer.Stop();
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
                    measure.Stop(); assertComparisonTimer.Stop();
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
            while (loopTimer.ElapsedTicks < Stopwatch.Frequency)
            {
                ;
            }
            measure.Stop(); assertComparisonTimer.Stop();
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
            while (loopTimer.ElapsedTicks < Stopwatch.Frequency)
            {
                ;
            }
            measure.Stop(); assertComparisonTimer.Stop();
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
                    measure.Stop(); assertComparisonTimer.Stop();
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
            while (loopTimer.ElapsedTicks < Stopwatch.Frequency)
            {
                ;
            }
            measure.Stop(); assertComparisonTimer.Stop();
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
            while (loopTimer.ElapsedTicks < Stopwatch.Frequency)
            {
                ;
            }
            // checking first.stop() stops measure
            first.Stop(); assertComparisonTimer.Stop();
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
                    measure.Stop(); assertComparisonTimer.Stop();
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
                    first.Stop(); assertComparisonTimer.Stop();
                }
            }
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }


        [TestMethod]
        public void MeasureInactiveStopwatchConstruction()
        {
            Startwatch measure = null;
            var assertComparisonTimer = Stopwatch.StartNew();
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < Stopwatch.Frequency)
            {
                ;
            }
            assertComparisonTimer.Stop();
            measure = Startwatch.FromStopwatch(assertComparisonTimer);
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }
        [TestMethod]
        public void MeasureInactiveStopwatchConstructionWithOverplay()
        {
            Startwatch measure = null;
            var assertComparisonTimer = Stopwatch.StartNew();
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < 2 * Stopwatch.Frequency)
            {
                if (loopTimer.ElapsedTicks > Stopwatch.Frequency)
                {
                    assertComparisonTimer.Stop();
                    measure = Startwatch.FromStopwatch(assertComparisonTimer);
                }
            }
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }

        [TestMethod]
        public void MeasureActiveStopwatchConstruction()
        {
            var assertComparisonTimer = Stopwatch.StartNew();
            Startwatch measure = Startwatch.FromStopwatch(assertComparisonTimer);
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < Stopwatch.Frequency)
            {
                ;
            }
            assertComparisonTimer.Stop();
            measure.Stop();
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }
        [TestMethod]
        public void MeasureActiveStopwatchConstructionWithOverplay()
        {
            var assertComparisonTimer = Stopwatch.StartNew();
            Startwatch measure = Startwatch.FromStopwatch(assertComparisonTimer);
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < 2 * Stopwatch.Frequency)
            {
                if (loopTimer.ElapsedTicks > Stopwatch.Frequency)
                {
                    assertComparisonTimer.Stop();
                    measure.Stop();
                }
            }
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }
        [TestMethod]
        public void MeasureActiveStopwatchConstructionWithUnderplay()
        {
            var assertComparisonTimer = Stopwatch.StartNew();
            Startwatch measure = null;
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < 2 * Stopwatch.Frequency)
            {
                if (loopTimer.ElapsedTicks > Stopwatch.Frequency) {
                    measure = Startwatch.FromStopwatch(assertComparisonTimer);
                }
            }
            assertComparisonTimer.Stop();
            measure.Stop();
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }
        [TestMethod]
        public void MeasureActiveStopwatchConstructionWithUnderplayAndOverplay()
        {
            var assertComparisonTimer = Stopwatch.StartNew();
            Startwatch measure = null;
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < 3 * Stopwatch.Frequency)
            {
                if (loopTimer.ElapsedTicks > Stopwatch.Frequency)
                {
                    measure = Startwatch.FromStopwatch(assertComparisonTimer);
                } else if (loopTimer.ElapsedTicks > 2 * Stopwatch.Frequency) {
                    assertComparisonTimer.Stop();
                    measure.Stop();
                }
            }
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }

        [TestMethod]
        public void MeasureStopwatchConstructionFromTicks()
        {
            var assertComparisonTimer = Stopwatch.StartNew();
            Startwatch measure = null;
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < Stopwatch.Frequency)
            {
                ;
            }
            assertComparisonTimer.Stop();
            measure = Startwatch.FromTicks(assertComparisonTimer.ElapsedTicks);
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }
        [TestMethod]
        public void MeasureStopwatchConstructionFromTicksWithOverplay()
        {
            var assertComparisonTimer = Stopwatch.StartNew();
            Startwatch measure = null;
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < Stopwatch.Frequency * 2)
            {
                if (loopTimer.ElapsedTicks > Stopwatch.Frequency)
                {
                    assertComparisonTimer.Stop();
                    measure = Startwatch.FromTicks(assertComparisonTimer.ElapsedTicks);
                }
            }
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }

        [TestMethod]
        public void MeasureInactiveStopwatch()
        {
            var assertComparisonTimer = new Stopwatch();
            var parent = new Startwatch();
            var measure = parent.CreateSibling();
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }


        [TestMethod]
        public void MeasureIndirectStartStopWithUnderplayAndOverplay01()
        {
            var p1 = new Startwatch();
            var parent = p1.CreateSibling();
            Startwatch measure = parent.CreateChild();
            var c2 = measure.CreateChild();
            var c3 = c2.CreateLastSibling(measure);

            Stopwatch assertComparisonTimer = null;
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < 3 * Stopwatch.Frequency)
            {
                if (assertComparisonTimer == null && loopTimer.ElapsedTicks > Stopwatch.Frequency)
                {
                    p1.Stop();
                    assertComparisonTimer = Stopwatch.StartNew();
                    c2.Stop();
                }
                else if (loopTimer.ElapsedTicks > 2 * Stopwatch.Frequency)
                {
                    assertComparisonTimer.Stop();
                    c3.Stop();
                }
            }
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }
        [TestMethod]
        public void MeasureIndirectStartStopWithUnderplayAndOverplay02()
        {
            var p1 = new Startwatch();
            Startwatch measure = p1.CreateSibling();
            var c2 = measure.CreateChild();
            var c3 = c2.CreateLastSibling(measure);

            Stopwatch assertComparisonTimer = null;
            var loopTimer = Stopwatch.StartNew();
            while (loopTimer.ElapsedTicks < 3 * Stopwatch.Frequency)
            {
                if (assertComparisonTimer == null && loopTimer.ElapsedTicks > Stopwatch.Frequency)
                {
                    p1.Stop();
                    assertComparisonTimer = Stopwatch.StartNew();
                    c2.Stop();
                }
                else if (loopTimer.ElapsedTicks > 2 * Stopwatch.Frequency)
                {
                    assertComparisonTimer.Stop();
                    c3.Stop();
                }
            }
            AssertStopwatch(assertComparisonTimer.ElapsedTicks, measure);
        }
    }
}
