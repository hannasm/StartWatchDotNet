using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using ExpressiveLogging;
using ExpressiveAssertions;
using StartwatchDiagnostics;

namespace StartwatchDiagnostics.Tests
{
    [TestClass]
    public class PerfEvent_DefaultCompletionBehaviorTests : PerformanceEventTestBase
    {
        static readonly ILogToken _lt = LogManager.GetToken();
        static readonly Func<ILogStream, IUnstoppablePerformanceEvent>[] positiveTestCases;
        static readonly Func<ILogStream, IUnstoppablePerformanceEvent>[] nestingTestCases;
        static readonly Func<ILogStream, IUnstoppablePerformanceEvent>[] negativeTestCases;
        static readonly Func<ILogStream, IUnstoppablePerformanceEvent>[] unnestingTestCases;

        static readonly ILogToken beforeToken = LogManager.GetToken(_lt, "Before"),
                                  afterToken = LogManager.GetToken(_lt, "After"),
                                  parentToken = LogManager.GetToken(_lt, "Parent");

        static readonly Dictionary<Func<ILogStream, IUnstoppablePerformanceEvent>, ILogToken> _tokenToAssert = new Dictionary<Func<ILogStream, IUnstoppablePerformanceEvent>, ILogToken>();
        static readonly Dictionary<Func<ILogStream, IUnstoppablePerformanceEvent>, LoggingLevel> _levelToAssert = new Dictionary<Func<ILogStream, IUnstoppablePerformanceEvent>, LoggingLevel>();
        static Func<ILogStream, IUnstoppablePerformanceEvent> genTestCase(Func<ILogStream, ILogToken, IUnstoppablePerformanceEvent> func, ILogToken t, LoggingLevel level)
        {
            Func<ILogStream,IUnstoppablePerformanceEvent> result = (l) => func(l, t);
            _tokenToAssert[result] = t;
            _levelToAssert[result] = level;
            return result;
        }
        static PerfEvent_DefaultCompletionBehaviorTests()
        {
            positiveTestCases = new Func<ILogStream,IUnstoppablePerformanceEvent>[] {
                genTestCase((log,tok)=>log.PerfEvent_NewEventDebug(tok), LogManager.GetToken(_lt, "EXAMPLE1"), LoggingLevel.DEBUG),
                genTestCase((log,tok)=>log.PerfEvent_NewEventInfo(tok), LogManager.GetToken(_lt, "EXAMPLE2"), LoggingLevel.INFO),
                genTestCase((log,tok)=>log.PerfEvent_NewEventAudit(tok), LogManager.GetToken(_lt, "EXAMPLE3"),LoggingLevel.AUDIT),
                genTestCase((log,tok)=>log.PerfEvent_NextEventDebug(tok), LogManager.GetToken(_lt, "EXAMPLE4"), LoggingLevel.DEBUG),
                genTestCase((log,tok)=>log.PerfEvent_NextEventInfo(tok), LogManager.GetToken(_lt, "EXAMPLE5"), LoggingLevel.INFO),
                genTestCase((log,tok)=>log.PerfEvent_NextEventAudit(tok), LogManager.GetToken(_lt, "EXAMPLE6"), LoggingLevel.AUDIT),
            };
            negativeTestCases = new Func<ILogStream, IUnstoppablePerformanceEvent>[] {
                genTestCase((log,tok)=>log.PerfEvent_PopEvent(),LogManager.GetToken(_lt, "EXAMPLE16"), LoggingLevel.ALERT)
            };
            nestingTestCases = new Func<ILogStream, IUnstoppablePerformanceEvent>[] {
                genTestCase((log,tok)=>log.PerfEvent_PushEventDebug(tok), LogManager.GetToken(_lt, "EXAMPLE7"), LoggingLevel.DEBUG),
                genTestCase((log,tok)=>log.PerfEvent_PushEventInfo(tok), LogManager.GetToken(_lt, "EXAMPLE8"), LoggingLevel.INFO),
                genTestCase((log,tok)=>log.PerfEvent_PushEventAudit(tok), LogManager.GetToken(_lt, "EXAMPLE9"), LoggingLevel.AUDIT),
                genTestCase((log,tok)=>log.PerfEvent_PushFirstEventDebug(tok), LogManager.GetToken(_lt, "EXAMPLE10"), LoggingLevel.DEBUG),
                genTestCase((log,tok)=>log.PerfEvent_PushFirstEventInfo(tok), LogManager.GetToken(_lt, "EXAMPLE11"), LoggingLevel.INFO),
                genTestCase((log,tok)=>log.PerfEvent_PushFirstEventAudit(tok), LogManager.GetToken(_lt, "EXAMPLE12"), LoggingLevel.AUDIT),
            };
            unnestingTestCases = new Func<ILogStream, IUnstoppablePerformanceEvent>[] {
                genTestCase((log,tok)=>log.PerfEvent_PopLastEventAudit(tok), LogManager.GetToken(_lt, "EXAMPLE13"), LoggingLevel.AUDIT),
                genTestCase((log,tok)=>log.PerfEvent_PopLastEventDebug(tok), LogManager.GetToken(_lt, "EXAMPLE14"), LoggingLevel.DEBUG),
                genTestCase((log,tok)=>log.PerfEvent_PopLastEventInfo(tok), LogManager.GetToken(_lt, "EXAMPLE15"), LoggingLevel.INFO),
            };
        }

        [TestMethod]
        public void Test001()
        {
            const string MESSAGE = "EXECUTED";
            var log = GetLogger();

            log.PerfEvent_Initialize().PerfEvent_DefaultCompletionBehavior((e, l, t, level) => level.LogMessage(l, t, m => m(MESSAGE)));
            var targetLevel = LoggingLevel.AUDIT;
            var assertLog = GetAssertLogger();
            var pe = log;
            foreach (var @case in positiveTestCases) {
                var before = pe.PerfEvent_NextEventAudit(beforeToken);
                var result = @case(log);
                var after = pe.PerfEvent_NextEventAudit(afterToken);

                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(()=> beforeToken, t); _assert.AreEqual(() => targetLevel, l); _assert.AreEqual(() => MESSAGE, m); });
                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => _tokenToAssert[@case], t); _assert.AreEqual(() => _levelToAssert[@case], l); _assert.AreEqual(() => MESSAGE, m); });

                ((IPerformanceEvent)after).EventCompleted(); // artificially complete this one so it doesnt' leak into the next test
                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => afterToken, t); _assert.AreEqual(() => targetLevel, l); _assert.AreEqual(() => MESSAGE, m); });

                // TODO: assertableLogger doesn't support this yet...
                //GetAssertLogger().AssertEmpty(before.LoggingLevel);
                //GetAssertLogger().AssertEmpty(after.LoggingLevel);
                //GetAssertLogger().AssertEmpty(targetLevel);
            }
        }

        [TestMethod]
        public void Test002()
        {
            const string MESSAGE = "EXECUTED";
            var log = GetLogger();

            log.PerfEvent_Initialize().PerfEvent_DefaultCompletionBehavior((e, l, t, level) => level.LogMessage(l, t, m => m(MESSAGE)));
            var targetLevel = LoggingLevel.AUDIT;
            var assertLog = GetAssertLogger();

            var pe = log;
            foreach (var @case in nestingTestCases)
            {
                var before = pe.PerfEvent_NextEventAudit(beforeToken);
                var result = @case(log);
                var after = pe.PerfEvent_NextEventAudit(afterToken);

                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => _tokenToAssert[@case], t); _assert.AreEqual(() => _levelToAssert[@case], l); _assert.AreEqual(() => MESSAGE, m); });
                ((IPerformanceEvent)after).EventCompleted();
                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => afterToken, t); _assert.AreEqual(() => targetLevel, l); _assert.AreEqual(() => MESSAGE, m); });

                pe.PerfEvent_PopEvent();
                ((IPerformanceEvent)before).EventCompleted();
                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => beforeToken, t); _assert.AreEqual(() => targetLevel, l); _assert.AreEqual(() => MESSAGE, m); });

                //GetAssertLogger().AssertEmpty(before.LoggingLevel);
                //GetAssertLogger().AssertEmpty(after.LoggingLevel);
                //GetAssertLogger().AssertEmpty(targetLevel);
            }
        }

        [TestMethod]
        public void Test003()
        {
            const string MESSAGE = "AUDIT EXECUTED";
            var log = GetLogger();
            log.PerfEvent_Initialize().PerfEvent_DefaultCompletionBehavior((e, l, t, level) => level.LogMessage(l, t, m => m(MESSAGE)));
            var assertLog = GetAssertLogger();
            var targetLevel = LoggingLevel.AUDIT;

            var pe = log;
            var beforeToken = LogManager.GetToken(_lt, "Before");
            var afterToken = LogManager.GetToken(_lt, "After");
            foreach (var @case in negativeTestCases)
            {
                var before = pe.PerfEvent_NextEventAudit(beforeToken);
                var result = @case(log);
                var after = pe.PerfEvent_NextEventAudit(afterToken);
                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => beforeToken, t); _assert.AreEqual(() => targetLevel, l); _assert.AreEqual(() => MESSAGE, m); });

                ((IPerformanceEvent)after).EventCompleted();
                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => afterToken, t); _assert.AreEqual(() => targetLevel, l); _assert.AreEqual(() => MESSAGE, m); });

                //GetAssertLogger().AssertEmpty(before.LoggingLevel);
                //GetAssertLogger().AssertEmpty(after.LoggingLevel);
                //GetAssertLogger().AssertEmpty(targetLevel);
            }
        }

        [TestMethod]
        public void Test004()
        {
            const string MESSAGE = "EXECUTED";
            var log = GetLogger();

            log.PerfEvent_Initialize().PerfEvent_DefaultCompletionBehavior((e, l, t, level) => level.LogMessage(l, t, m => m(MESSAGE)));
            var targetLevel = LoggingLevel.AUDIT;
            var assertLog = GetAssertLogger();

            var pe = log;
            foreach (var @case in unnestingTestCases)
            {
                var parent = pe.PerfEvent_NextEventAudit(parentToken);
                var before = pe.PerfEvent_PushEventAudit(beforeToken);
                var result = @case(log);
                var after = pe.PerfEvent_NextEventAudit(afterToken);

                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => beforeToken, t); _assert.AreEqual(() => targetLevel, l); _assert.AreEqual(() => MESSAGE, m); });
                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => parentToken, t); _assert.AreEqual(() => targetLevel, l); _assert.AreEqual(() => MESSAGE, m); });
                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => _tokenToAssert[@case], t); _assert.AreEqual(() => _levelToAssert[@case], l); _assert.AreEqual(() => MESSAGE, m); });

                ((IPerformanceEvent)after).EventCompleted();
                assertLog.AssertMessage((l, t, e, u, m, f) => { _assert.AreEqual(() => afterToken, t); _assert.AreEqual(() => MESSAGE, m); });

                //GetAssertLogger().AssertEmpty(parent.LoggingLevel);
                //GetAssertLogger().AssertEmpty(before.LoggingLevel);
                //GetAssertLogger().AssertEmpty(after.LoggingLevel);
                //GetAssertLogger().AssertEmpty(targetLevel);
            }
        }        
    }
}
