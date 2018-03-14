using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressiveLogging;

namespace StartwatchDiagnostics.Tests
{
    [TestClass]
    public class TimingPerformanceComparisons : TestBase
    {
        private static readonly ILogToken _lt = LogManager.GetToken();
        static readonly ILogToken perfTrackerToken = LogManager.GetToken(_lt, "perfTracker");

        struct TestResults
        {
            public long start;
            public long stop;
            public long count;

            public decimal seconds { get { return (stop - start) / Stopwatch.Frequency; } }
            public decimal averageSeconds { get { return (stop - start) / Stopwatch.Frequency / count; } }
        }
        TestResults RunPerfTest(int durationSeconds, Action behavior)
        {
            for (int i = 0; i < 100; ++i) { behavior(); }

            var result = new TestResults();
            result.start = Stopwatch.GetTimestamp();
            while ((result.stop = Stopwatch.GetTimestamp()) - result.start < Stopwatch.Frequency * durationSeconds)
            {
                behavior();
                result.count++;
            }
            return result;
        }

        [TestMethod]
        public void OneWatchFor2Seconds03()
        {
            const int duration = 2;

            Action epAction = () =>
            {
                Startwatch watch = new Startwatch();
                watch.Stop();
            };
            Action msAction = () =>
            {
                Stopwatch watch = Stopwatch.StartNew();
                watch.Stop();
            };

            var ptLogger1 = CreateLogger();
            ptLogger1.PerfEvent_Initialize(); // i think bypassing this onetime cost is acceptable
            Action ptAction1 = () =>
            {
                var evt = ptLogger1.PerfEvent_NewEventInfo(perfTrackerToken);
                ((IPerformanceEvent)evt).EventCompleted();
            };
            var ptLogger2 = CreateLogger();
            ptLogger2.PerfEvent_Initialize(); // i think bypassing this onetime cost is acceptable
            Action ptAction2 = () =>
            {
                var evt = ptLogger2.PerfEvent_NewEventInfo(perfTrackerToken);
                ((IPerformanceEvent)evt).EventCompleted();
            };
            var ptLogger3 = CreateLogger();
            ptLogger3.PerfEvent_Initialize(); // i think bypassing this onetime cost is acceptable
            Action ptAction3 = () =>
            {
                var evt = ptLogger3.PerfEvent_NextEventInfo(perfTrackerToken);
            };
            
            // warmup
            RunPerfTest(duration, epAction);
            RunPerfTest(duration, msAction);
            RunPerfTest(duration, ptAction1);
            RunPerfTest(duration, ptAction2);
            RunPerfTest(duration, ptAction3);

            var ep = RunPerfTest(duration, epAction);
            var ms = RunPerfTest(duration, msAction);
            var pt1 = RunPerfTest(duration, ptAction1);
            var pt2 = RunPerfTest(duration, ptAction2);
            var pt3 = RunPerfTest(duration, ptAction3);

            var log = GetLogger();
            log.Info(_lt, m => m("Completed {0} Startwatch iterations in {1} seconds (average {2} seconds)", ep.count, ep.seconds, ep.averageSeconds));
            log.Info(_lt, m => m("Completed {0} system.diagnostics.stopwatch iterations in {1} seconds (average {2} seconds)", ms.count, ms.seconds, ms.averageSeconds));
            log.Info(_lt, m => m("Completed {0} performanceTracker1 iterations in {1} seconds (average {2} seconds)", pt1.count, pt1.seconds, pt1.averageSeconds));
            log.Info(_lt, m => m("Completed {0} performanceTracker2 iterations in {1} seconds (average {2} seconds)", pt2.count, pt2.seconds, pt2.averageSeconds));
            log.Info(_lt, m => m("Completed {0} performanceTracker3 iterations in {1} seconds (average {2} seconds)", pt3.count, pt3.seconds, pt3.averageSeconds));
        }


        [TestMethod]
        public void OneParentChildFor2Seconds01()
        {
            const int duration = 2;

            Action epAct = () =>
            {
                Startwatch watch1 = new Startwatch();
                Startwatch watch2 = watch1.CreateChild();
                watch2.Stop();
                watch1.Stop();
            };
            Action msAct = () =>
            {
                Stopwatch watch1 = Stopwatch.StartNew();
                Stopwatch watch2 = Stopwatch.StartNew();
                watch2.Stop();
                watch1.Stop();
            };

            var ptLogger1 = CreateLogger();
            ptLogger1.PerfEvent_Initialize(); // i think bypassing this onetime cost is acceptable
            Action ptAction1 = () =>
            {
                var evt = ptLogger1.PerfEvent_NewEventInfo(perfTrackerToken);
                var evt2 = ptLogger1.PerfEvent_PushFirstEventAudit(perfTrackerToken);
                ptLogger1.PerfEvent_PopEvent();
            };
            var ptLogger2 = CreateLogger();
            ptLogger2.PerfEvent_Initialize(); // i think bypassing this onetime cost is acceptable
            Action ptAction2 = () =>
            {
                var evt = ptLogger2.PerfEvent_NewEventInfo(perfTrackerToken);
                var evt2 = ptLogger2.PerfEvent_PushFirstEventAudit(perfTrackerToken);
                ptLogger2.PerfEvent_PopEvent();
            };
            var ptLogger3 = CreateLogger();
            ptLogger3.PerfEvent_Initialize(); // i think bypassing this onetime cost is acceptable
            Action ptAction3 = () =>
            {
                var evt = ptLogger3.PerfEvent_NextEventInfo(perfTrackerToken);
                var evt2 = ptLogger3.PerfEvent_PushFirstEventAudit(perfTrackerToken);
                ptLogger3.PerfEvent_PopEvent();
            };
            
            // warmup
            RunPerfTest(duration, epAct);
            RunPerfTest(duration, msAct);
            RunPerfTest(duration, ptAction1);
            RunPerfTest(duration, ptAction2);
            RunPerfTest(duration, ptAction3);

            var ep = RunPerfTest(duration, epAct);
            var ms = RunPerfTest(duration, msAct);
            var pt1 = RunPerfTest(duration, ptAction1);
            var pt2 = RunPerfTest(duration, ptAction2);
            var pt3 = RunPerfTest(duration, ptAction3);

            var log = GetLogger();
            log.Info(_lt, m => m("Completed {0} Startwatch iterations in {1} seconds (average {2} seconds)", ep.count, ep.seconds, ep.averageSeconds));
            log.Info(_lt, m => m("Completed {0} system.diagnostics.stopwatch iterations in {1} seconds (average {2} seconds)", ms.count, ms.seconds, ms.averageSeconds));
            log.Info(_lt, m => m("Completed {0} performanceTracker1 iterations in {1} seconds (average {2} seconds)", pt1.count, pt1.seconds, pt1.averageSeconds));
            log.Info(_lt, m => m("Completed {0} performanceTracker2 iterations in {1} seconds (average {2} seconds)", pt2.count, pt2.seconds, pt2.averageSeconds));
            log.Info(_lt, m => m("Completed {0} performanceTracker3 iterations in {1} seconds (average {2} seconds)", pt3.count, pt3.seconds, pt3.averageSeconds));
        }

        [TestMethod]
        public void OneParentChildSiblingSiblingFor2Seconds01()
        {
            const int duration = 2;

            Action epAct = () =>
            {
                Startwatch watch1 = new Startwatch();
                Startwatch watch2 = watch1.CreateChild();
                Startwatch watch3 = watch2.CreateSibling();
                Startwatch watch4 = watch3.CreateSibling();
                watch2.Stop();
                watch3.Stop();
                watch4.Stop();
                watch1.Stop();
            };
            Action msAct = () =>
            {
                Stopwatch watch1 = Stopwatch.StartNew();
                Stopwatch watch2 = Stopwatch.StartNew();
                watch2.Stop();
                Stopwatch watch3 = Stopwatch.StartNew();
                watch3.Stop();
                Stopwatch watch4 = Stopwatch.StartNew();
                watch4.Stop();
                watch1.Stop();
            };

            var ptLogger1 = CreateLogger();
            ptLogger1.PerfEvent_Initialize(); // i think bypassing this onetime cost is acceptable
            Action ptAction1 = () =>
            {
                var evt = ptLogger1.PerfEvent_NewEventInfo(perfTrackerToken);
                var evt2 = ptLogger1.PerfEvent_PushFirstEventAudit(perfTrackerToken);
                var evt3 = ptLogger1.PerfEvent_NextEventAudit(perfTrackerToken);
                var evt4 = ptLogger1.PerfEvent_NextEventAudit(perfTrackerToken);
                ptLogger1.PerfEvent_PopEvent();
            };
            var ptLogger2 = CreateLogger();
            ptLogger2.PerfEvent_Initialize(); // i think bypassing this onetime cost is acceptable
            Action ptAction2 = () =>
            {
                var evt = ptLogger2.PerfEvent_NewEventInfo(perfTrackerToken);
                var evt2 = ptLogger2.PerfEvent_PushFirstEventAudit(perfTrackerToken);
                var evt3 = ptLogger2.PerfEvent_NextEventAudit(perfTrackerToken);
                var evt4 = ptLogger2.PerfEvent_NextEventAudit(perfTrackerToken);
                ptLogger2.PerfEvent_PopEvent();
            };
            var ptLogger3 = CreateLogger();
            ptLogger3.PerfEvent_Initialize(); // i think bypassing this onetime cost is acceptable
            Action ptAction3 = () =>
            {
                var evt = ptLogger3.PerfEvent_NextEventInfo(perfTrackerToken);
                var evt2 = ptLogger3.PerfEvent_PushFirstEventAudit(perfTrackerToken);
                var evt3 = ptLogger3.PerfEvent_NextEventAudit(perfTrackerToken);
                var evt4 = ptLogger3.PerfEvent_NextEventAudit(perfTrackerToken);
                ptLogger3.PerfEvent_PopEvent();
            };
            
            // warmup
            RunPerfTest(duration, epAct);
            RunPerfTest(duration, msAct);
            RunPerfTest(duration, ptAction1);
            RunPerfTest(duration, ptAction2);
            RunPerfTest(duration, ptAction3);

            var ep = RunPerfTest(duration, epAct);
            var ms = RunPerfTest(duration, msAct);
            var pt1 = RunPerfTest(duration, ptAction1);
            var pt2 = RunPerfTest(duration, ptAction2);
            var pt3 = RunPerfTest(duration, ptAction3);

            var log = GetLogger();
            log.Info(_lt, m => m("Completed {0} Startwatch iterations in {1} seconds (average {2} seconds)", ep.count, ep.seconds, ep.averageSeconds));
            log.Info(_lt, m => m("Completed {0} system.diagnostics.stopwatch iterations in {1} seconds (average {2} seconds)", ms.count, ms.seconds, ms.averageSeconds));
            log.Info(_lt, m => m("Completed {0} performanceTracker1 iterations in {1} seconds (average {2} seconds)", pt1.count, pt1.seconds, pt1.averageSeconds));
            log.Info(_lt, m => m("Completed {0} performanceTracker2 iterations in {1} seconds (average {2} seconds)", pt2.count, pt2.seconds, pt2.averageSeconds));
            log.Info(_lt, m => m("Completed {0} performanceTracker3 iterations in {1} seconds (average {2} seconds)", pt3.count, pt3.seconds, pt3.averageSeconds));
        }

        [TestMethod]
        public void OneParentChildChildParentSiblingChildSiblingParentSiblingChildSiblingParentFor2Seconds01()
        {
            const int duration = 2;

            Action epAct = () =>
            {
                Startwatch watch1 = new Startwatch();
                Startwatch watch2 = watch1.CreateChild();
                var watch2Child1 = watch2.CreateChild();
                Startwatch watch3 = watch2.CreateSibling();
                var watch3Child1 = watch3.CreateChild();
                var watch3Child2 = watch3Child1.CreateSibling();
                Startwatch watch4 = watch3.CreateSibling();
                var watch4Child1 = watch4.CreateChild();
                var watch4Child2 = watch4Child1.CreateSibling();
                // do stuff
                watch2Child1.Stop();
                // do more stuff??
                watch2.Stop();
                // watch3 starts
                // do stuff ...
                watch3Child1.Stop();
                // do stuff ...
                watch3Child2.Stop();
                // do stuff ...
                watch3.Stop();
                // watch4 starts
                // do stuff ...
                watch4Child1.Stop();
                // do stuff ...
                watch4Child2.Stop();
                // do stuff ...
                watch4.Stop();
                // do stuff ...
                watch1.Stop();
            };
            Action msAct = () =>
            {
                Stopwatch watch1 = Stopwatch.StartNew();
                Stopwatch watch2 = Stopwatch.StartNew();
                var watch2Child1 = Stopwatch.StartNew();
                // do stuff ...
                watch2Child1.Stop();
                // do more stuff??
                watch2.Stop();
                Stopwatch watch3 = Stopwatch.StartNew();
                var watch3Child1 = Stopwatch.StartNew();
                // do stuff ...
                watch3Child1.Stop();
                var watch3Child2 = Stopwatch.StartNew();
                // do stuff ...
                watch3Child2.Stop();
                // do more stuff??
                watch3.Stop();
                Stopwatch watch4 = Stopwatch.StartNew();
                var watch4Child1 = Stopwatch.StartNew();
                // do stuff ...
                watch4Child1.Stop();
                var watch4Child2 = Stopwatch.StartNew();
                // do stuff ...
                watch4Child2.Stop();
                // do more stuff??
                watch4.Stop();
                // do more stuff??
                watch1.Stop();
            };

            var ptLogger1 = CreateLogger();
            ptLogger1.PerfEvent_Initialize(); // i think bypassing this onetime cost is acceptable
            Action ptAction1 = () =>
            {
                var evt = ptLogger1.PerfEvent_NewEventInfo(perfTrackerToken);
                var evt2 = ptLogger1.PerfEvent_PushFirstEventAudit(perfTrackerToken);
                var evt3 = ptLogger1.PerfEvent_PushFirstEventAudit(perfTrackerToken);
                var evt4 = ptLogger1.PerfEvent_NextEventAudit(perfTrackerToken);
                ptLogger1.PerfEvent_PopEvent();
                ptLogger1.PerfEvent_NextEventAudit(perfTrackerToken);
                ptLogger1.PerfEvent_PushFirstEventDebug(perfTrackerToken);
                ptLogger1.PerfEvent_NextEventAudit(perfTrackerToken);
                ptLogger1.PerfEvent_PopEvent();
                ptLogger1.PerfEvent_PushEventAudit(perfTrackerToken);
                ptLogger1.PerfEvent_NextEventAudit(perfTrackerToken);
                ptLogger1.PerfEvent_PopEvent();
                ptLogger1.PerfEvent_PopEvent();
            };
            
            // warmup
            RunPerfTest(duration, epAct);
            RunPerfTest(duration, msAct);
            RunPerfTest(duration, ptAction1);

            var ep = RunPerfTest(duration, epAct);
            var ms = RunPerfTest(duration, msAct);
            var pt1 = RunPerfTest(duration, ptAction1);

            var log = GetLogger();
            log.Info(_lt, m => m("Completed {0} Startwatch iterations in {1} seconds (average {2} seconds)", ep.count, ep.seconds, ep.averageSeconds));
            log.Info(_lt, m => m("Completed {0} system.diagnostics.stopwatch iterations in {1} seconds (average {2} seconds)", ms.count, ms.seconds, ms.averageSeconds));
            log.Info(_lt, m => m("Completed {0} performanceTracker1 iterations in {1} seconds (average {2} seconds)", pt1.count, pt1.seconds, pt1.averageSeconds));
        }

    }
}
