using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressiveLogging.V1;
using ExpressiveLogging;

namespace StartwatchDiagnostics.Tests
{
    [TestClass]
    public class TestPerformanceEvent_PopComplete : PerformanceEventTestBase
    {
        ILogToken _lt = LogManager.GetToken();

        [TestMethod]
        public void Test001()
        {
            var log = GetLogger();

            var total = log.PerfEvent_GetRunningTotal();
            var target = log.PerfEvent_NewEventAudit(_lt);

            log.PerfEvent_PushEventAudit(_lt);
            log.PerfEvent_NextEventAudit(_lt);

            log.PerfEvent_PopComplete(target);

            Assert.IsFalse(target.TimeData.IsActive, "Target timer should have been stopped during PopComplete");
            Assert.IsTrue(total.TimeData.IsActive, "Total timer should not have been stopped");
        }

        [TestMethod]
        public void Test002()
        {
            var log = GetLogger();

            var total = log.PerfEvent_GetRunningTotal();
            var target = log.PerfEvent_NewEventAudit(_lt);

            var child1 = log.PerfEvent_PushEventAudit(_lt);
            var child2 = log.PerfEvent_NextEventAudit(_lt);

            Assert.IsFalse(child1.TimeData.IsActive, "Child1 should have been stopped");
            Assert.IsTrue(child2.TimeData.IsActive, "Child2 should not have been stopped");

            var child2child1 = log.PerfEvent_PushEventAudit(_lt);
            var child2child2 = log.PerfEvent_NextEventAudit(_lt);

            Assert.IsTrue(child2.TimeData.IsActive, "Child2 should not have been stopped");
            Assert.IsTrue(child2child2.TimeData.IsActive, "Child2 Child2 should not have been stopped");
            Assert.IsFalse(child2child1.TimeData.IsActive, "Child2 Child1 should have been stopped");

            log.PerfEvent_PopComplete(target);

            Assert.IsFalse(target.TimeData.IsActive, "Target timer should have been stopped during PopComplete");
            Assert.IsTrue(total.TimeData.IsActive, "Total timer should not have been stopped");
            Assert.IsFalse(child2.TimeData.IsActive, "Child2 should have been stopped");
            Assert.IsFalse(child2child2.TimeData.IsActive, "Child2 Child2 should have been stopped");
        }


        [TestMethod]
        public void Test003()
        {
            var log = GetLogger();

            var total = log.PerfEvent_GetRunningTotal();
            var parent1 = log.PerfEvent_PushEventAudit(_lt);
            var parent2 = log.PerfEvent_PushEventAudit(_lt);

            var target = log.PerfEvent_NewEventAudit(_lt);

            Assert.IsFalse(parent2.TimeData.IsActive, "parent2 should have been stopped on new event call");
            Assert.IsTrue(parent1.TimeData.IsActive, "parent1 should not have been stopped");

            var child1 = log.PerfEvent_PushEventAudit(_lt);
            var child2 = log.PerfEvent_NextEventAudit(_lt);

            Assert.IsFalse(child1.TimeData.IsActive, "child1 should have been stopped");
            Assert.IsTrue(child2.TimeData.IsActive, "chidl2 should not have been stopped");
            Assert.IsTrue(target.TimeData.IsActive, "target should not have been stopped");

            log.PerfEvent_PopComplete(target);
            
            Assert.IsFalse(parent2.TimeData.IsActive, "parent2 should have been stopped on new event call");
            Assert.IsTrue(parent1.TimeData.IsActive, "parent1 should not have been stopped");
            Assert.IsFalse(target.TimeData.IsActive, "Target timer should have been stopped during PopComplete");
            Assert.IsTrue(total.TimeData.IsActive, "Total timer should not have been stopped");
            Assert.IsFalse(child2.TimeData.IsActive, "Chidl2 should have been stopped");
        }

        [TestMethod]
        public void Test004()
        {
            var log = GetLogger();
            var total = log.PerfEvent_GetRunningTotal();
            var parent1 = log.PerfEvent_PushEventAudit(_lt);
            var parent2 = log.PerfEvent_PushEventAudit(_lt);

            var target = log.PerfEvent_NewEventAudit(_lt);

            Assert.IsFalse(parent2.TimeData.IsActive, "parent2 should have been stopped on new event call");
            Assert.IsTrue(parent1.TimeData.IsActive, "parent1 should not have been stopped");
            Assert.IsTrue(total.TimeData.IsActive, "total should not have been stopped");

            var child1 = log.PerfEvent_PushEventAudit(_lt);
            var child2 = log.PerfEvent_NextEventAudit(_lt);

            Assert.IsFalse(child1.TimeData.IsActive, "Child1 should have been stopped");
            Assert.IsTrue(child2.TimeData.IsActive, "Child2 should not have been stopped");
            Assert.IsTrue(parent1.TimeData.IsActive, "parent1 should not have been stopped");
            Assert.IsTrue(total.TimeData.IsActive, "total should not have been stopped");

            var child2child1 = log.PerfEvent_PushEventAudit(_lt);
            var child2child2 = log.PerfEvent_NextEventAudit(_lt);

            Assert.IsTrue(child2.TimeData.IsActive, "Child2 should not have been stopped");
            Assert.IsTrue(child2child2.TimeData.IsActive, "Child2 Child2 should not have been stopped");
            Assert.IsFalse(child2child1.TimeData.IsActive, "Child2 Child1 should have been stopped");
            Assert.IsTrue(parent1.TimeData.IsActive, "parent1 should not have been stopped");
            Assert.IsTrue(total.TimeData.IsActive, "total should not have been stopped");

            log.PerfEvent_PopComplete(target);

            Assert.IsFalse(parent2.TimeData.IsActive, "parent2 should have been stopped on new event call");
            Assert.IsTrue(parent1.TimeData.IsActive, "parent1 should not have been stopped");
            Assert.IsFalse(target.TimeData.IsActive, "Target timer should have been stopped during PopComplete");
            Assert.IsTrue(total.TimeData.IsActive, "Total timer should not have been stopped");
            Assert.IsFalse(child2.TimeData.IsActive, "Chidl2 should have been stopped");
        }
    }
}
