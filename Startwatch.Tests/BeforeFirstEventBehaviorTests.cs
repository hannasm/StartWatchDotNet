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
    public class BeforeFirstEventBehaviorTests : PerformanceEventTestBase
    {
        static readonly ILogToken _lt = LogManager.GetToken();
        static readonly ILogToken firstToken = LogManager.GetToken(_lt, "First");

        [TestMethod]
        public void Test001_Audit()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_NewEventAudit(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }
        [TestMethod]
        public void Test001_Info()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_NewEventInfo(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }
        [TestMethod]
        public void Test001_Debug()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_NewEventDebug(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }

        [TestMethod]
        public void Test002_Audit()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_NextEventAudit(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }
        [TestMethod]
        public void Test002_Info()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_NextEventInfo(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }
        [TestMethod]
        public void Test002_Debug()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_NextEventDebug(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }

        [TestMethod]
        public void Test003_Audit()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_PushEventAudit(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }
        [TestMethod]
        public void Test003_Info()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_PushEventInfo(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }
        [TestMethod]
        public void Test003_Debug()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_PushEventDebug(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }

        [TestMethod]
        public void Test004_Audit()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_PushFirstEventAudit(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }
        [TestMethod]
        public void Test004_Info()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_PushFirstEventInfo(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }
        [TestMethod]
        public void Test004_Debug()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_PushFirstEventDebug(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }

        [TestMethod]
        public void Test005_Audit()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_PopLastEventAudit(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }
        [TestMethod]
        public void Test005_Info()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_PopLastEventInfo(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }
        [TestMethod]
        public void Test005_Debug()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_PopLastEventDebug(firstToken);

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }
        [TestMethod]
        public void Test006()
        {
            var log = GetLogger();
            var pe = log;

            pe.PerfEvent_Initialize();

            var first = pe.PerfEvent_PopEvent();

            Assert.IsFalse(pe.PerfEvent_GetBeforeFirstEventTime().TimeData.IsActive, "Before first event should have stopped");
        }
    }
}
