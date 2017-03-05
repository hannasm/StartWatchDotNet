using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressiveLogging.V1;
using ExpressiveLogging;
using ExpressiveAssertions;

namespace StartwatchDiagnostics.Tests
{
    [TestClass]
    public class WhenCompleteTests : PerformanceEventTestBase
    {
        static readonly ILogToken _lt = LogManager.GetToken();
        static readonly ILogToken beforeToken = LogManager.GetToken(_lt, "Before"),
                                  afterToken = LogManager.GetToken(_lt, "After"),
                                  parentToken = LogManager.GetToken(_lt, "Parent");
        static readonly Func<ILogStream, IUnstoppablePerformanceEvent>[] positiveTestCases;
        static readonly Func<ILogStream, IUnstoppablePerformanceEvent>[] nestingTestCases;
        static readonly Func<ILogStream, IUnstoppablePerformanceEvent>[] unnestingTestCases;

        static readonly Dictionary<Func<ILogStream, IUnstoppablePerformanceEvent>, ILogToken> _tokenToAssert = new Dictionary<Func<ILogStream, IUnstoppablePerformanceEvent>, ILogToken>();
        static readonly Dictionary<Func<ILogStream, IUnstoppablePerformanceEvent>, LoggingLevel> _levelToAssert = new Dictionary<Func<ILogStream, IUnstoppablePerformanceEvent>, LoggingLevel>();
        static Func<ILogStream, IUnstoppablePerformanceEvent> genTestCase(Func<ILogStream, ILogToken, IUnstoppablePerformanceEvent> func, ILogToken t, LoggingLevel level)
        {
            Func<ILogStream, IUnstoppablePerformanceEvent> result = (l) => func(l, t);
            _tokenToAssert[result] = t;
            _levelToAssert[result] = level;
            return result;
        }
        static WhenCompleteTests()
        {
            positiveTestCases = new Func<ILogStream, IUnstoppablePerformanceEvent>[] {
                genTestCase((log,token)=>log.PerfEvent_NewEventDebug(token), LogManager.GetToken(_lt, "EXAMPLE1"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_NewEventInfo(token), LogManager.GetToken(_lt, "EXAMPLE2"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_NewEventAudit(token), LogManager.GetToken(_lt, "EXAMPLE3"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_NextEventDebug(token), LogManager.GetToken(_lt, "EXAMPLE4"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_NextEventInfo(token), LogManager.GetToken(_lt, "EXAMPLE5"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_NextEventAudit(token), LogManager.GetToken(_lt, "EXAMPLE6"), LoggingLevel.DEBUG),
            };
            nestingTestCases = new Func<ILogStream, IUnstoppablePerformanceEvent>[] {
                genTestCase((log,token)=>log.PerfEvent_PushEventDebug(token), LogManager.GetToken(_lt, "EXAMPLE7"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_PushEventInfo(token), LogManager.GetToken(_lt, "EXAMPLE8"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_PushEventAudit(token), LogManager.GetToken(_lt, "EXAMPLE9"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_PushFirstEventDebug(token), LogManager.GetToken(_lt, "EXAMPLE10"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_PushFirstEventInfo(token), LogManager.GetToken(_lt, "EXAMPLE11"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_PushFirstEventAudit(token), LogManager.GetToken(_lt, "EXAMPLE12"), LoggingLevel.DEBUG),
            };
            unnestingTestCases = new Func<ILogStream, IUnstoppablePerformanceEvent>[] {
                genTestCase((log,token)=>log.PerfEvent_PopLastEventAudit(token), LogManager.GetToken(_lt, "EXAMPLE13"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_PopLastEventDebug(token), LogManager.GetToken(_lt, "EXAMPLE14"), LoggingLevel.DEBUG),
                genTestCase((log,token)=>log.PerfEvent_PopLastEventInfo(token), LogManager.GetToken(_lt, "EXAMPLE15"), LoggingLevel.DEBUG),
            };
        }

        [TestMethod]
        public void Test001()
        {
            const string MESSAGE = "EXECUTED";
            var log = GetLogger();

            var targetLevel = LoggingLevel.AUDIT;
            var assertLog = GetAssertLogger();

            var pe = log;
            foreach (var @case in positiveTestCases)
            {

                var result = @case(log);
                result.WhenComplete((e) => _levelToAssert[@case].LogMessage(log, _tokenToAssert[@case], m => m(MESSAGE)));

                var after = pe.PerfEvent_NextEventAudit(afterToken);

                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => _tokenToAssert[@case], t); _assert.AreEqual(() => _levelToAssert[@case], l); _assert.AreEqual(() => MESSAGE, m); });

                ((IPerformanceEvent)after).EventCompleted();

                //GetAssertLogger().AssertEmpty(after.LoggingLevel);
                //GetAssertLogger().AssertEmpty(targetLevel);
            }
        }

        [TestMethod]
        public void Test002()
        {
            const string MESSAGE = "EXECUTED";
            var log = GetLogger();

            var targetLevel = LoggingLevel.AUDIT;
            var assertLog = GetAssertLogger();

            var pe = log;
            foreach (var @case in nestingTestCases)
            {
                var parent = pe.PerfEvent_NewEventAudit(parentToken);
                var result = @case(log);
                result.WhenComplete((e) => _levelToAssert[@case].LogMessage(log, _tokenToAssert[@case], m => m(MESSAGE)));
                pe.PerfEvent_PopEvent();
                var after = pe.PerfEvent_NextEventAudit(afterToken);

                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => _tokenToAssert[@case], t); _assert.AreEqual(() => _levelToAssert[@case], l); _assert.AreEqual(() => MESSAGE, m); });

                ((IPerformanceEvent)after).EventCompleted();

                //GetAssertLogger().AssertEmpty(parent.LoggingLevel);
                //GetAssertLogger().AssertEmpty(after.LoggingLevel);
                //GetAssertLogger().AssertEmpty(targetLevel);
            }
        }

        [TestMethod]
        public void Test003()
        {
            const string MESSAGE = "EXECUTED";
            var log = GetLogger();

            var targetLevel = LoggingLevel.AUDIT;
            var assertLog = GetAssertLogger();

            var pe = log;
            foreach (var @case in unnestingTestCases)
            {
                var parent = pe.PerfEvent_NextEventAudit(parentToken);
                var before = pe.PerfEvent_PushEventAudit(beforeToken);
                var result = @case(log);
                result.WhenComplete((e) => _levelToAssert[@case].LogMessage(log, _tokenToAssert[@case], m => m(MESSAGE)));

                var after = pe.PerfEvent_NextEventAudit(afterToken);

                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => _tokenToAssert[@case], t); _assert.AreEqual(() => _levelToAssert[@case], l); _assert.AreEqual(() => MESSAGE, m); });

                ((IPerformanceEvent)after).EventCompleted();

                //GetAssertLogger().AssertEmpty(parent.LoggingLevel);
                //GetAssertLogger().AssertEmpty(before.LoggingLevel);
                //GetAssertLogger().AssertEmpty(after.LoggingLevel);
                //GetAssertLogger().AssertEmpty(targetLevel);
            }
        }
    }
}
