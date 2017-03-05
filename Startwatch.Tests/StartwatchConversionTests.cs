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
    public class StartwatchConversionTests
    {
        public void AssertTimespansClose(TimeSpan expect, TimeSpan actual)
        {
            if (expect.TotalMilliseconds - 10 > actual.TotalMilliseconds || expect.TotalMilliseconds + 10 < actual.TotalMilliseconds) {
                Assert.AreEqual(expect, actual, "with threshold of 10 milliseconds both directions");
            }
        }

        public void AssertMillisecondsClose(double expected, long actual)
        {
            long expect = (long)expected;
            if (expect - 10 > actual || expect + 10 < actual)
            {
                Assert.AreEqual(expect, actual, "with threshold of 10 milliseconds both directions");
            }
        }

        [TestMethod]
        public void TestTicksToTimespan01()
        {
            var stopwatch = Startwatch.FromTicks(Stopwatch.Frequency * 20);

            AssertTimespansClose(new TimeSpan(0, 0, 20), stopwatch.Elapsed);
        }
        [TestMethod]
        public void TestTicksToTimespan02()
        {
            var stopwatch = Startwatch.FromTicks(Stopwatch.Frequency * 25);

            AssertTimespansClose(new TimeSpan(0, 0, 25), stopwatch.Elapsed);
        }
        [TestMethod]
        public void TestTicksToTimespan03()
        {
            var stopwatch = Startwatch.FromTicks((long)(Stopwatch.Frequency * 5M + 50M * Stopwatch.Frequency / 1000M));

            AssertTimespansClose(new TimeSpan(0, 0, 0, 5, 50), stopwatch.Elapsed);
        }
        [TestMethod]
        public void TestTimespanToTimespan01()
        {
            var stopwatch = Startwatch.FromTimeSpan(new TimeSpan(0, 0, 20));

            Assert.AreEqual(new TimeSpan(0, 0, 20), stopwatch.Elapsed);
        }
        [TestMethod]
        public void TestTimespanToTimespan02()
        {
            var stopwatch = Startwatch.FromTimeSpan(new TimeSpan(0, 0, 35));

            Assert.AreEqual(new TimeSpan(0, 0, 35), stopwatch.Elapsed);
        }
        [TestMethod]
        public void TestTimespanToTimespan03()
        {
            var stopwatch = Startwatch.FromTimeSpan(new TimeSpan(0, 0, 0, 5, 150));

            AssertTimespansClose(new TimeSpan(0, 0, 0, 5, 150), stopwatch.Elapsed);
        }
        [TestMethod]
        public void TestMillisecondsToTimespan01()
        {
            var stopwatch = Startwatch.FromMilliseconds(20 * 1000);

            Assert.AreEqual(new TimeSpan(0, 0, 20), stopwatch.Elapsed);
        }
        [TestMethod]
        public void TestMillisecondsToTimespan02()
        {
            var stopwatch = Startwatch.FromMilliseconds(25 * 1000 + 200);

            AssertTimespansClose(new TimeSpan(0, 0, 0, 25, 200), stopwatch.Elapsed);
        }
        [TestMethod]
        public void Test100NSTicksToTimespan01()
        {
            var stopwatch = Startwatch.FromTimeSpanTicks(20000);

            AssertTimespansClose(new TimeSpan(20000), stopwatch.Elapsed);
        }
        [TestMethod]
        public void Test100NSTicksToTimespan02()
        {
            var stopwatch = Startwatch.FromTimeSpanTicks(1234567689);

            AssertTimespansClose(new TimeSpan(1234567689), stopwatch.Elapsed);
        }

        [TestMethod]
        public void TestTicksToMilliseconds01()
        {
            var stopwatch = Startwatch.FromTicks(Stopwatch.Frequency * 20);

            AssertMillisecondsClose(new TimeSpan(0, 0, 20).TotalMilliseconds, stopwatch.ElapsedMilliseconds);
        }
        [TestMethod]
        public void TestTicksToMilliseconds02()
        {
            var stopwatch = Startwatch.FromTicks(Stopwatch.Frequency * 25);

            AssertMillisecondsClose(new TimeSpan(0, 0, 25).TotalMilliseconds, stopwatch.ElapsedMilliseconds);
        }
        [TestMethod]
        public void TestTicksToMilliseconds03()
        {
            var stopwatch = Startwatch.FromTicks((long)(Stopwatch.Frequency * 5M + 50M * Stopwatch.Frequency / 1000M));

            AssertMillisecondsClose(new TimeSpan(0, 0, 0, 5, 50).TotalMilliseconds, stopwatch.ElapsedMilliseconds);
        }
        [TestMethod]
        public void TestTimespanToMilliseconds01()
        {
            var stopwatch = Startwatch.FromTimeSpan(new TimeSpan(0, 0, 20));

            AssertMillisecondsClose(new TimeSpan(0, 0, 20).TotalMilliseconds, stopwatch.ElapsedMilliseconds);
        }
        [TestMethod]
        public void TestTimespanToMilliseconds02()
        {
            var stopwatch = Startwatch.FromTimeSpan(new TimeSpan(0, 0, 35));

            AssertMillisecondsClose(new TimeSpan(0, 0, 35).TotalMilliseconds, stopwatch.ElapsedMilliseconds);
        }
        [TestMethod]
        public void TestTimespanToMilliseconds03()
        {
            var stopwatch = Startwatch.FromTimeSpan(new TimeSpan(0, 0, 0, 5, 150));

            AssertMillisecondsClose(new TimeSpan(0, 0, 0, 5, 150).TotalMilliseconds, stopwatch.ElapsedMilliseconds);
        }
        [TestMethod]
        public void TestMillisecondsToMilliseconds01()
        {
            var stopwatch = Startwatch.FromMilliseconds(20 * 1000);

            AssertMillisecondsClose(new TimeSpan(0, 0, 20).TotalMilliseconds, stopwatch.ElapsedMilliseconds);
        }
        [TestMethod]
        public void TestMillisecondsToMilliseconds02()
        {
            var stopwatch = Startwatch.FromMilliseconds(25 * 1000 + 200);

            AssertMillisecondsClose(new TimeSpan(0, 0, 0, 25, 200).TotalMilliseconds, stopwatch.ElapsedMilliseconds);
        }
        [TestMethod]
        public void Test100NSTicksToMilliseconds01()
        {
            var stopwatch = Startwatch.FromTimeSpanTicks(20000);

            AssertMillisecondsClose(new TimeSpan(20000).TotalMilliseconds, stopwatch.ElapsedMilliseconds);
        }
        [TestMethod]
        public void Test100NSTicksToMilliseconds02()
        {
            var stopwatch = Startwatch.FromTimeSpanTicks(1234567689);

            AssertMillisecondsClose(new TimeSpan(1234567689).TotalMilliseconds, stopwatch.ElapsedMilliseconds);
        }

    }
}
