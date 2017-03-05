using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressiveLogging.V1;
using ExpressiveLogging;

namespace StartwatchDiagnostics.Tests
{
    [TestClass]
    public class TestPerformanceEvent_PopTo : PerformanceEventTestBase
    {
        ILogToken _lt = LogManager.GetToken();

        [TestMethod]
        public void Test001()
        {
            var log = GetLogger();
            
            var total = log.PerfEvent_NewEventAudit(_lt);

            log.PerfEvent_PushEventAudit(_lt);
            log.PerfEvent_NextEventAudit(_lt);

            log.PerfEvent_PopToEvent(total);

            Assert.IsTrue(total.TimeData.IsActive, "Total timer should not have been stopped during PopTo");

            log.PerfEvent_CompleteEvent();

            Assert.IsFalse(total.TimeData.IsActive, "Total timer should have been stopped");
        }

        [TestMethod]
        public void Test002()
        {
            var log = GetLogger();

            var total = log.PerfEvent_NewEventAudit(_lt);

            log.PerfEvent_PushEventAudit(_lt);
            log.PerfEvent_NextEventAudit(_lt);
            log.PerfEvent_PushEventAudit(_lt);
            log.PerfEvent_NextEventAudit(_lt);

            log.PerfEvent_PopToEvent(total);

            Assert.IsTrue(total.TimeData.IsActive, "Total timer should not have been stopped during PopTo");

            log.PerfEvent_CompleteEvent();

            Assert.IsFalse(total.TimeData.IsActive, "Total timer should have been stopped");
        }


        [TestMethod]
        public void Test003()
        {
            var log = GetLogger();

            var parent1 = log.PerfEvent_PushEventAudit(_lt);
            var parent2 = log.PerfEvent_PushEventAudit(_lt);

            var total = log.PerfEvent_NewEventAudit(_lt);

            Assert.IsFalse(parent2.TimeData.IsActive, "parent2 should have been stopped on new event call");
            Assert.IsTrue(parent1.TimeData.IsActive, "parent1 should not have been stopped");

            log.PerfEvent_PushEventAudit(_lt);
            log.PerfEvent_NextEventAudit(_lt);

            log.PerfEvent_PopToEvent(total);

            Assert.IsTrue(total.TimeData.IsActive, "Total timer should have been stopped");

            log.PerfEvent_CompleteEvent();

            Assert.IsFalse(total.TimeData.IsActive, "Total timer should have been stopped");

            Assert.IsTrue(parent1.TimeData.IsActive, "parent1 should not have been stopped");
        }

        [TestMethod]
        public void Test004()
        {
            var log = GetLogger();
            var parent1 = log.PerfEvent_PushEventAudit(_lt);
            var parent2 = log.PerfEvent_PushEventAudit(_lt);

            var total = log.PerfEvent_NewEventAudit(_lt);

            Assert.IsFalse(parent2.TimeData.IsActive, "parent2 should have been stopped on new event call");
            Assert.IsTrue(parent1.TimeData.IsActive, "parent1 should not have been stopped");

            log.PerfEvent_PushEventAudit(_lt);
            log.PerfEvent_NextEventAudit(_lt);
            log.PerfEvent_PushEventAudit(_lt);
            log.PerfEvent_NextEventAudit(_lt);

            log.PerfEvent_PopToEvent(total);

            Assert.IsTrue(total.TimeData.IsActive, "Total timer should have been stopped");

            log.PerfEvent_CompleteEvent();

            Assert.IsFalse(total.TimeData.IsActive, "Total timer should have been stopped");
            Assert.IsTrue(parent1.TimeData.IsActive, "parent1 should not have been stopped");
        }
    }
}
