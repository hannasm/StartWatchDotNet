using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressiveLogging;

namespace StartwatchDiagnostics.Tests
{
    [TestClass]
    public class PerformanceTrackerDefaultTimerTests : PerformanceEventTestBase
    {
        static readonly ILogToken _lt = LogManager.GetToken();

        static IEnumerable<int> UniqueNumberSequence(int initial) {
            int sum = initial;
            while (true) {
                var @int = sum;
                sum = sum << 1;
                yield return @int;
            }
        }
        static Lazy<IEnumerator<int>> _wastedTimeSequence = new Lazy<IEnumerator<int>>(()=>UniqueNumberSequence(8).GetEnumerator());

        /// <summary>
        /// Waste a little bit of time, which would be useful in identifying bugs in the stopwatch / timer code
        /// </summary>
        private void WasteTime() {
            _wastedTimeSequence.Value.MoveNext();
            System.Threading.Thread.Sleep(_wastedTimeSequence.Value.Current);
        }

        [TestMethod]
        public void Test001()
        {
            var log = GetLogger();
            Startwatch timer = new Startwatch();
            log.PerfEvent_Initialize();
            timer.Stop();
            WasteTime();

            Assert.IsFalse(log.PerfEvent_GetSetupTime().TimeData.IsActive, "SEtup timer should not be running");
            AssertStopwatch(timer, log.PerfEvent_GetSetupTime().TimeData);
        }

        [TestMethod]
        public void Test002()
        {
            var log = GetLogger();
            log.PerfEvent_Initialize();
            Startwatch timer = new Startwatch();
            WasteTime();
            Assert.IsTrue(log.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "First event timer should be running");
            log.PerfEvent_NewEventAudit(_lt);
            timer.Stop();
            WasteTime();

            Assert.IsFalse(log.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event timer should not be runing");
            AssertStopwatch(timer, log.PerfEvent_GetBeforeFirstEventTime().TimeData);
        }

        [TestMethod]
        public void Test003()
        {
            var log = GetLogger();
            log.PerfEvent_Initialize();
            Assert.IsFalse(log.PerfEvent_GetRunningTotal().TimeData.IsActive, "Running total timer should not have started yet");
            WasteTime();
            Startwatch timer = new Startwatch();
            log.PerfEvent_NewEventAudit(_lt);
            WasteTime();
            Assert.IsTrue(log.PerfEvent_GetRunningTotal().TimeData.IsActive, "Total timer should have started");
            log.PerfEvent_NextEventAudit(_lt);
            WasteTime();
            Assert.IsTrue(log.PerfEvent_GetRunningTotal().TimeData.IsActive, "Total timer should have started");
            log.PerfEvent_CompleteEverything();
            timer.Stop();

            WasteTime();

            Assert.IsFalse(log.PerfEvent_GetRunningTotal().TimeData.IsActive, "Running total timer should have finished");
            AssertStopwatch(timer, log.PerfEvent_GetRunningTotal().TimeData);
        }

        [TestMethod]
        public void Test004()
        {
            var log = GetLogger();
            log.PerfEvent_Initialize();
            Assert.IsFalse(log.PerfEvent_GetRunningTotal().TimeData.IsActive, "Running total timer should not have started yet");
            WasteTime();
            Startwatch timer = new Startwatch();
            log.PerfEvent_NewEventAudit(_lt);
            Assert.IsTrue(log.PerfEvent_GetRunningTotal().TimeData.IsActive, "Total timer should have started");
            WasteTime();
            Assert.IsTrue(log.PerfEvent_GetRunningTotal().TimeData.IsActive, "Total timer should have started");
            log.PerfEvent_CompleteEverything();
            timer.Stop();

            WasteTime();

            Assert.IsFalse(log.PerfEvent_GetRunningTotal().TimeData.IsActive, "Running total timer should have finished");
            AssertStopwatch(timer, log.PerfEvent_GetRunningTotal().TimeData);
        }
    }
}
