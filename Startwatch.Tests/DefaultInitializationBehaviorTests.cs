using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressiveLogging;
using ExpressiveAssertions;

namespace StartwatchDiagnostics.Tests
{
    [TestClass]
    public class PerfEvent_DefaultInitializationBehaviorTests : PerformanceEventTestBase
    {
        private static readonly ILogToken _lt = LogManager.GetToken();

        Func<ILogStream, IUnstoppablePerformanceEvent>[] positiveTestCases;
        Func<ILogStream, IUnstoppablePerformanceEvent>[] negativeTestCases;

        static readonly Dictionary<Func<ILogStream, IUnstoppablePerformanceEvent>, ILogToken> _tokenToAssert = new Dictionary<Func<ILogStream, IUnstoppablePerformanceEvent>, ILogToken>();
        static readonly Dictionary<Func<ILogStream, IUnstoppablePerformanceEvent>, LoggingLevel> _levelToAssert = new Dictionary<Func<ILogStream, IUnstoppablePerformanceEvent>, LoggingLevel>();
        static Func<ILogStream, IUnstoppablePerformanceEvent> genTestCase(Func<ILogStream, ILogToken, IUnstoppablePerformanceEvent> func, ILogToken t, LoggingLevel level)
        {
            Func<ILogStream, IUnstoppablePerformanceEvent> result = (l) => func(l, t);
            _tokenToAssert[result] = t;
            _levelToAssert[result] = level;
            return result;
        }
        public PerfEvent_DefaultInitializationBehaviorTests()
        {
            positiveTestCases = new Func<ILogStream, IUnstoppablePerformanceEvent>[] {
                genTestCase((log,token)=>log.PerfEvent_NewEventDebug(token), LogManager.GetToken(_lt, "EXAMPLE1"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_NewEventInfo(token), LogManager.GetToken(_lt, "EXAMPLE2"), LoggingLevel.INFO),
                genTestCase((log,token)=>log.PerfEvent_NewEventAudit(token), LogManager.GetToken(_lt, "EXAMPLE3"), LoggingLevel.AUDIT),
                genTestCase((log,token)=>log.PerfEvent_NextEventDebug(token), LogManager.GetToken(_lt, "EXAMPLE4"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_NextEventInfo(token), LogManager.GetToken(_lt, "EXAMPLE5"), LoggingLevel.INFO),
                genTestCase((log,token)=>log.PerfEvent_NextEventAudit(token), LogManager.GetToken(_lt, "EXAMPLE6"), LoggingLevel.AUDIT),
                genTestCase((log,token)=>log.PerfEvent_PushEventDebug(token), LogManager.GetToken(_lt, "EXAMPLE7"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_PushEventInfo(token), LogManager.GetToken(_lt, "EXAMPLE8"), LoggingLevel.INFO),
                genTestCase((log,token)=>log.PerfEvent_PushEventAudit(token), LogManager.GetToken(_lt, "EXAMPLE9"), LoggingLevel.AUDIT),
                genTestCase((log,token)=>log.PerfEvent_PushFirstEventDebug(token), LogManager.GetToken(_lt, "EXAMPLE10"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_PushFirstEventInfo(token), LogManager.GetToken(_lt, "EXAMPLE11"), LoggingLevel.INFO),
                genTestCase((log,token)=>log.PerfEvent_PushFirstEventAudit(token), LogManager.GetToken(_lt, "EXAMPLE12"), LoggingLevel.AUDIT),
                genTestCase((log,token)=>log.PerfEvent_PopLastEventAudit(token), LogManager.GetToken(_lt, "EXAMPLE13"), LoggingLevel.AUDIT),
                genTestCase((log,token)=>log.PerfEvent_PopLastEventDebug(token), LogManager.GetToken(_lt, "EXAMPLE14"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_PopLastEventInfo(token), LogManager.GetToken(_lt, "EXAMPLE15"), LoggingLevel.INFO),
            };
            negativeTestCases = new Func<ILogStream, IUnstoppablePerformanceEvent>[] {
                (log)=>log.PerfEvent_PopEvent(),
            };
        }

        [TestMethod]
        public void Test001()
        {
            const string MESSAGE = "EXECUTED";
            var log = GetLogger();

            log.PerfEvent_Initialize().PerfEvent_DefaultInitializationBehavior((e, l, t, level) => level.LogMessage(l, t, m => m(MESSAGE)));
            var targetLevel = LoggingLevel.AUDIT;
            var assertLog = GetAssertLogger();

            int caseNum = 0;
            foreach (var @case in positiveTestCases)
            {
                _assert.ContextSet("caseId", caseNum++.ToString());

                var result = @case(log);
                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => _tokenToAssert[@case], t); _assert.AreEqual(() => _levelToAssert[@case], l); _assert.AreEqual(() => MESSAGE, m); });

                //GetAssertLogger().AssertEmpty(targetLevel);
            }
        }

        [TestMethod]
        public void Test002()
        {
            const string MESSAGE = "AUDIT EXECUTED";
            var log = GetLogger();
            log.PerfEvent_Initialize().PerfEvent_DefaultInitializationBehavior((e, l, t, level) => level.LogMessage(l, t, m => m(MESSAGE)));
            var assertLog = GetAssertLogger();
            var targetLevel = LoggingLevel.AUDIT;


            var pe = log;
            var beforeToken = LogManager.GetToken(_lt, "Before");
            var afterToken = LogManager.GetToken(_lt, "After");
            int caseNum = 0;
            foreach (var @case in negativeTestCases)
            {
                _assert.ContextSet("caseId", caseNum++.ToString());

                pe.PerfEvent_NextEventAudit(beforeToken);
                var result = @case(log);
                pe.PerfEvent_NextEventAudit(afterToken);
                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => beforeToken, t); _assert.AreEqual(() => targetLevel, l); _assert.AreEqual(() => MESSAGE, m); });
                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => afterToken, t); _assert.AreEqual(() => targetLevel, l); _assert.AreEqual(() => MESSAGE, m); });

                //GetAssertLogger().AssertEmpty(result.LoggingLevel);
            }
        }        
    }
}
