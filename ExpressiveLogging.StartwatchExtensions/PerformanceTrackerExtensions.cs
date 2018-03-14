using ExpressiveLogging;
using ExpressiveLogging.Counters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using StartwatchDiagnostics;

namespace ExpressiveLogging
{
    public static class DiagnosticPerformanceTrackerEventLoggingExtensions
    {
        static readonly ILogToken _lt = LogManager.GetToken();
        class InternalData
        {
            public InternalData(ILogStream log)
            {
                DefaultCompletionBehaviors = new List<Action<IUnstoppablePerformanceEvent, ILogStream, ILogToken, LoggingLevel>>();
                DefaultInitializationBehaviors = new List<Action<IUnstoppablePerformanceEvent, ILogStream, ILogToken, LoggingLevel>>();
                Tracker = new PerformanceEventTracker();
            }
            public readonly PerformanceEventTracker Tracker;

            public readonly List<Action<IUnstoppablePerformanceEvent, ILogStream, ILogToken, LoggingLevel>> DefaultInitializationBehaviors;
            public readonly List<Action<IUnstoppablePerformanceEvent, ILogStream, ILogToken, LoggingLevel>> DefaultCompletionBehaviors;
        }
        readonly static ConditionalWeakTable<ILogStream, InternalData> _data = new ConditionalWeakTable<ILogStream, InternalData>();
        static InternalData GetData(ILogStream log)
        {
            InternalData result = null;
            if (_data.TryGetValue(log, out result))
            {
                return result;
            }
            _data.Add(log, result = new InternalData(log));
            return result;
        }

        /// <summary>
        /// Returns the current size of the performance tracker stack, primarily this is
        /// intended for testing that the stack size is being maintained properly.
        /// </summary>
        public static int PerfEvent_GetStackSize(this ILogStream log)
        {
            return GetData(log).Tracker.GetStackSize();
        }

        /// <summary>
        /// Returns an event representing the time spent setting up the performance tracker
        /// </summary>
        public static IUnstoppablePerformanceEvent PerfEvent_GetSetupTime(this ILogStream log)
        {
            return GetData(log).Tracker.SetupEvent;
        }
        /// <summary>
        /// Returns the total time that spent by the performance wrapper
        /// </summary>
        public static IUnstoppablePerformanceEvent PerfEvent_GetRunningTotal(this ILogStream log)
        {
            return GetData(log).Tracker.TotalEvent;
        }
        /// <summary>
        /// Time spent between initialization and the first event being created.
        /// </summary>
        public static IUnstoppablePerformanceEvent PerfEvent_GetBeforeFirstEventTime(this ILogStream log)
        {
            return GetData(log).Tracker.BeforeFirstEvent;
        }

        /// <summary>
        /// Ensure that performance tracker is initialized now. If this method is not called,
        /// additional performance costs will be incurred on first use.
        /// </summary>
        public static ILogStream PerfEvent_Initialize(this ILogStream log)
        {
            var data = GetData(log);
            // TODO: make sure compiler doesn't optimize this away?
            return log;
        }
        
        /// <summary>
        /// Define an action to take at the beginning of every performance event
        /// </summary>
        public static ILogStream PerfEvent_DefaultInitializationBehavior(this ILogStream log, Action<IUnstoppablePerformanceEvent, ILogStream, ILogToken, LoggingLevel> action)
        {
            GetData(log).DefaultInitializationBehaviors.Add(action);
            return log;
        }

        
        /// <summary>
        /// Define a generic action to perform upon completion of every created PerformanceEvent.
        /// </summary>
        public static ILogStream PerfEvent_DefaultCompletionBehavior(this ILogStream log, Action<IUnstoppablePerformanceEvent, ILogStream, ILogToken, LoggingLevel> action)
        {
            GetData(log).DefaultCompletionBehaviors.Add(action);
            return log;
        }

        /// <summary>
        /// Executes for each performance event and executes some of the 'configuration' associated with events.
        /// </summary>
        private static IUnstoppablePerformanceEvent CreateEvent(ILogStream log, ILogToken token, LoggingLevel level, Func<InternalData, IUnstoppablePerformanceEvent> eventFunc)
        {
            var data = GetData(log);
            var @event = eventFunc(data);

            for (int i = 0, n = data.DefaultCompletionBehaviors.Count; i < n; i++)
            {
                var li = i;
                @event.WhenComplete(e=>data.DefaultCompletionBehaviors[li](e,log,token,level));
            }            

            var initBehv = data.DefaultInitializationBehaviors;
            var len = initBehv.Count;
            for (int i = 0; i < len; i++)
            {
                initBehv[i](@event, log, token, level);
            }

            return @event;
        }

        public static void IncrementCounterBy(this ILogStream log, ILogToken token, INamedCounterToken ct, IUnstoppablePerformanceEvent value)
        {
            log.IncrementCounterBy(token, ct, value.TimeData.ElapsedTicks);
        }
        public static void IncrementCounterBy(this ILogStream log, IRawCounterToken ct, IUnstoppablePerformanceEvent value)
        {
            log.IncrementCounterBy(ct, value.TimeData.ElapsedTicks);
        }

        /// <see cref="PerformanceEventTracker.NextEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_NextEventDebug(this ILogStream log, ILogToken token) { return CreateEvent(log,token,LoggingLevel.DEBUG, d => d.Tracker.NextEvent()); }
        /// <see cref="PerformanceEventTracker.NewEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_NewEventDebug(this ILogStream log, ILogToken token) { return CreateEvent(log,token, LoggingLevel.DEBUG, d => d.Tracker.NewEvent()); }
        /// <see cref="PerformanceEventTracker.AddCustomEvent" />
        public static IUnstoppablePerformanceEvent PerfEvent_AddCustomEventDebug(this ILogStream log, ILogToken token, Startwatch watch) { return CreateEvent(log,token, LoggingLevel.DEBUG, d => d.Tracker.AddCustomEvent(watch)); }
        /// <see cref="PerformanceEventTracker.PushCustomEvent" />
        public static IUnstoppablePerformanceEvent PerfEvent_PushCustomEventDebug(this ILogStream log, ILogToken token, Startwatch watch) { return CreateEvent(log,token, LoggingLevel.DEBUG, d => d.Tracker.PushCustomEvent(watch)); }
        /// <see cref="PerformanceEventTracker.PushFirstEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_PushFirstEventDebug(this ILogStream log, ILogToken token) { return CreateEvent(log,token, LoggingLevel.DEBUG, d => d.Tracker.PushFirstEvent()); }
        /// <see cref="PerformanceEventTracker.PushEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_PushEventDebug(this ILogStream log, ILogToken token) { return CreateEvent(log,token, LoggingLevel.DEBUG, d => d.Tracker.PushEvent()); }
        /// <see cref="PerformanceEventTracker.PopLastEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_PopLastEventDebug(this ILogStream log, ILogToken token) { return CreateEvent(log,token, LoggingLevel.DEBUG, d => d.Tracker.PopLastEvent()); }

        /// <see cref="PerformanceEventTracker.NextEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_NextEventInfo(this ILogStream log, ILogToken token) { return CreateEvent(log,token, LoggingLevel.INFO, d => d.Tracker.NextEvent()); }
        /// <see cref="PerformanceEventTracker.NewEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_NewEventInfo(this ILogStream log, ILogToken token) { return CreateEvent(log,token, LoggingLevel.INFO, d => d.Tracker.NewEvent()); }
        /// <see cref="PerformanceEventTracker.AddCustomEvent" />
        public static IUnstoppablePerformanceEvent PerfEvent_AddCustomEventInfo(this ILogStream log, ILogToken token, Startwatch watch) { return CreateEvent(log,token, LoggingLevel.INFO, d => d.Tracker.AddCustomEvent(watch)); }
        /// <see cref="PerformanceEventTracker.PushCustomEvent" />
        public static IUnstoppablePerformanceEvent PerfEvent_PushCustomEventInfo(this ILogStream log, ILogToken token, Startwatch watch) { return CreateEvent(log,token, LoggingLevel.INFO, d => d.Tracker.PushCustomEvent(watch)); }
        /// <see cref="PerformanceEventTracker.PushFirstEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_PushFirstEventInfo(this ILogStream log, ILogToken token) { return CreateEvent(log,token, LoggingLevel.INFO, d => d.Tracker.PushFirstEvent()); }
        /// <see cref="PerformanceEventTracker.PushEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_PushEventInfo(this ILogStream log, ILogToken token) { return CreateEvent(log,token, LoggingLevel.INFO, d => d.Tracker.PushEvent()); }
        /// <see cref="PerformanceEventTracker.PopLastEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_PopLastEventInfo(this ILogStream log, ILogToken token) { return CreateEvent(log,token, LoggingLevel.INFO, d => d.Tracker.PopLastEvent()); }

        /// <see cref="PerformanceEventTracker.NextEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_NextEventAudit(this ILogStream log, ILogToken token) { return CreateEvent(log,token, LoggingLevel.AUDIT, d => d.Tracker.NextEvent()); }
        /// <see cref="PerformanceEventTracker.NewEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_NewEventAudit(this ILogStream log, ILogToken token) { return CreateEvent(log,token, LoggingLevel.AUDIT, d => d.Tracker.NewEvent()); }
        /// <see cref="PerformanceEventTracker.AddCustomEvent" />
        public static IUnstoppablePerformanceEvent PerfEvent_AddCustomEventAudit(this ILogStream log, ILogToken token, Startwatch watch) { return CreateEvent(log,token, LoggingLevel.AUDIT, d => d.Tracker.AddCustomEvent(watch)); }
        /// <see cref="PerformanceEventTracker.PushCustomEvent" />
        public static IUnstoppablePerformanceEvent PerfEvent_PushCustomEventAudit(this ILogStream log, ILogToken token, Startwatch watch) { return CreateEvent(log,token, LoggingLevel.AUDIT, d => d.Tracker.PushCustomEvent(watch)); }
        /// <see cref="PerformanceEventTracker.PushFirstEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_PushFirstEventAudit(this ILogStream log, ILogToken token) { return CreateEvent(log,token, LoggingLevel.AUDIT, d => d.Tracker.PushFirstEvent()); }
        /// <see cref="PerformanceEventTracker.PushEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_PushEventAudit(this ILogStream log, ILogToken token) { return CreateEvent(log,token, LoggingLevel.AUDIT, d => d.Tracker.PushEvent()); }
        /// <see cref="PerformanceEventTracker.PopLastEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_PopLastEventAudit(this ILogStream log, ILogToken token) { return CreateEvent(log,token, LoggingLevel.AUDIT, d => d.Tracker.PopLastEvent()); }

        /// <see cref="PerformanceEventTracker.PopEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_PopEvent(this ILogStream log) { return GetData(log).Tracker.PopEvent(); }
        /// <see cref="PerformanceEventTracker.PopToEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_PopToEvent(this ILogStream log, IUnstoppablePerformanceEvent evt) { return GetData(log).Tracker.PopToEvent(evt); }
        /// <see cref="PerformanceEventTracker.PopComplete"/>
        public static IUnstoppablePerformanceEvent PerfEvent_PopComplete(this ILogStream log, IUnstoppablePerformanceEvent evt) { return GetData(log).Tracker.PopComplete(evt); }


        /// <see cref="PerformanceEventTracker.CompleteEvent"/>
        public static IUnstoppablePerformanceEvent PerfEvent_CompleteEvent(this ILogStream log) { return GetData(log).Tracker.CompleteEvent(); }

        /// <see cref="PerformanceEventTracker.Complete"/>
        public static IUnstoppablePerformanceEvent PerfEvent_CompleteEverything(this ILogStream log) { return GetData(log).Tracker.Complete(); }
    }
}
