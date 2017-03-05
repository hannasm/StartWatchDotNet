using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StartwatchDiagnostics.Tests
{
    [TestClass]
    public class StartwatchTimestampFieldTests
    {
        public void AssertDatesSimilar(DateTime? expect, DateTime? actual)
        {
            if (!expect.HasValue) {
                string msg = "Expected null, actual had a value of ";
                if (actual != null) {
                    msg += actual.Value.ToString();
                }
                Assert.IsTrue(!actual.HasValue, msg);
            } else {
                Assert.IsTrue(actual.HasValue, "Expected value, but actual was null");
                if (expect.Value.AddMilliseconds(10) < actual.Value || expect.Value.AddMilliseconds(-10) > actual.Value) {
                    Assert.AreEqual(expect, actual, "With thresold of 10 millisecond");
                }
            }
        }

        [TestMethod]
        public void StartTimeAfterStopping()
        {
            var stopwatch = new Startwatch();
            var dateTime = DateTime.Now;
            stopwatch.Stop();
            AssertDatesSimilar(dateTime, stopwatch.StartTime);
        }
        [TestMethod]
        public void StartTimeWithoutStopping()
        {
            var stopwatch = new Startwatch();
            var dateTime = DateTime.Now;
            AssertDatesSimilar(dateTime, stopwatch.StartTime);
        }

        [TestMethod]
        public void StartTimeWithoutStarting()
        {
            var first = new Startwatch();
            var second = first.CreateSibling();
            AssertDatesSimilar(null, second.StartTime);

        }
        [TestMethod]
        public void EndTimeWithoutStopping()
        {
            var stopwatch = new Startwatch();
            AssertDatesSimilar(null, stopwatch.EndTime);
        }
        [TestMethod]
        public void EndTimeWithStopping()
        {
            var stopwatch = new Startwatch();
            Thread.Sleep(10);
            stopwatch.Stop();
            var dateTime = DateTime.Now;
            AssertDatesSimilar(dateTime, stopwatch.EndTime);
        }

        [TestMethod]
        public void IsActiveOnActiveStopwatch()
        {
            var watch = new Startwatch();
            Assert.IsTrue(watch.IsActive, "Stopwatch active property");
        }
        [TestMethod]
        public void IsActiveOnInactiveStopwatch()
        {
            var watch = new Startwatch();
            watch.Stop();
            
            Assert.IsFalse(watch.IsActive, "Stopwatch active property");
        }
    }
}
