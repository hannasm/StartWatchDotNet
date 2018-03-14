using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace StartwatchDiagnostics
{
    /// <summary>
    /// Implementation similar to System.Diagnostics.Stopwatch with different assumptions made for better performance
    ///   * this stopwatch cannot be restarted or re-used. You must instantiate a new instance each time you want to collect
    ///   * this stopwatch can share the start timestamp with another stopwatch, this is the Parent relationship
    ///   * this stopwatch can use the ending timestamp of another stopwatch as it's starting timestamp, this is the Sibling relationship
    ///   * this stopwatch can be constructed statically using either Ticks, Milliseconds, TimeStamps, or a traditional Stopwatch, in these cases the Stopwatch
    ///      will not ever be startable / stoppable, however will represent a time region in both system ticks as well as DateTime ticks (100-ns intervals)
    ///   * this stopwatch yields better performance when tracking performance of multiple consecutively executed code regions, while still offering similar performance in less structured settings
    /// </summary>
    public sealed class Startwatch : IReadonlyStartwatch
    {
        /// <summary>
        /// Represents an Startwatch as a parent, for use in constructing stopwatch hierarchies
        /// </summary>
        interface IParent
        {
        }
        /// <summary>
        /// Create a new Stopwatch object that is a child. The child stopwatch will share the same start timestamp as it's parent.
        /// </summary>
        public Startwatch CreateChild() { return new Startwatch(this, (IParent)null); }

        /// <summary>
        /// Represents an Startwatch as a sibling, for use in constructing stopwatch hierarchies
        /// </summary>
        interface ISibling
        {
        }
        /// <summary>
        /// Create a new Stopwatch object that is a sibling, the sibling will start when the stop event of this stopwatch
        /// is called.
        /// </summary>
        public Startwatch CreateSibling() { return new Startwatch(this, (ISibling)null); }
        /// <summary>
        /// Represents an Startwatch that is a sibling, but also shares the end event of the provided parent. This 
        /// is a somewhat less common event but can be useful when used correctly and in certain circumstances
        /// </summary>
        public Startwatch CreateLastSibling(Startwatch parent) { return new Startwatch(this, (ISibling)null, parent, (IParent)null); }


        /// <summary>
        /// internal constructor that allows creation of a child stopwatch from parent
        /// </summary>
        Startwatch(Startwatch parent, IParent fakeParentParameter)
        {
            _startTimestamp = parent._startTimestamp;
            _stopTimestamp = new RefTimestamp();
        }

        /// <summary>
        /// internal constructor that allows creation of a sibling stopwatch from older sibling
        /// </summary>
        Startwatch(Startwatch sibling, ISibling fakeParentParameter)
        {
            _startTimestamp = sibling._stopTimestamp;
            _stopTimestamp = new RefTimestamp();
        }

        /// <summary>
        /// Internal constructor to allow creation of a stopwatch that uses previous sibling 'end' timestamp as the 'start'
        /// for this watch, and uses the assigned parents 'end' timestamp as the 'end' for this watch.
        /// </summary>
        Startwatch(Startwatch sibling, ISibling fakeParentParameter, Startwatch parent, IParent parent1)
        {
            if (parent == null) { throw new ArgumentNullException("parent"); }
            _startTimestamp = sibling._stopTimestamp;
            _stopTimestamp = parent._stopTimestamp;
        }

        /// <summary>
        /// Drop in replacement for the Stopwatch.StartNew() method of System.Diagnostics.Stopwatch class.
        /// </summary>
        public static Startwatch StartNew() { return new Startwatch(); }

        /// <summary>
        /// Create a started stopwatch
        /// </summary>
        public Startwatch()
        {
            _stopTimestamp = new RefTimestamp();
            _startTimestamp = new RefTimestamp();
            Startwatch.QueryPerformanceCounter(out _startTimestamp.Value);
        }
        /// <summary>
        /// private constructor allows creation of Startwatch in started state, using system dependent ticks
        /// </summary>
        Startwatch(long start) {
            _startTimestamp = new RefTimestamp(start);
            _stopTimestamp = new RefTimestamp();
        }
        /// <summary>
        /// private constructor allows creation of Startwatch in stopped / completed state, using system dependent ticks
        /// </summary>
        Startwatch(long start, long end) {
            _startTimestamp = new RefTimestamp(start);
            _stopTimestamp = new RefTimestamp(end);
        }

        readonly RefTimestamp _startTimestamp;
        readonly RefTimestamp _stopTimestamp;

        /// <summary>
        /// private class which tracks timestamps by reference instead of by value
        /// </summary>
        class RefTimestamp {
            public RefTimestamp() {}
            public RefTimestamp(long val) {
                Value = val;
            }
            public long Value;
        }

        /// <summary>
        /// Stop time tracking on the Startwatch, allowing for representation of a time period.
        /// </summary>
        /// <returns>true if the watch was running before the call, false if it was already stopped</returns>
        public bool Stop()
        {
            if (_stopTimestamp.Value == 0)
            {
                Startwatch.QueryPerformanceCounter(out _stopTimestamp.Value);
                return true;
            }
            return false;
        }
                
        readonly static long TicksPerMillisecond;
        
        static Startwatch() {
            TicksPerMillisecond = Stopwatch.Frequency / 1000;
        }

        long RawTicks { get { 
            // logic inversion is going on here to get slightly faster performance,
            // once stop has been set, the assumption is that start must also have been set
            // the intuitive ordering would be, if it's not started return 0, if it's started but not stopped return ..., otherwise...
            // TODO: would be nice to enforce the assumption about starting / stopping more carefully with code contracts or something
            if (_stopTimestamp.Value != 0) {
                return _stopTimestamp.Value - _startTimestamp.Value;
            } else if (_startTimestamp.Value != 0) {
                long result;
                Startwatch.QueryPerformanceCounter(out result);
                return result - _startTimestamp.Value;
            } else {
                return 0;
            }
        }}
        /// <summary>
        /// Retrieve the duration tracked by the stopwatch in ticks
        /// </summary>
        public long ElapsedTicks { get { return RawTicks; } }
        /// <summary>
        /// Retrieve the duration tracked by the stopwatch in milliseconds
        /// </summary>
        public long ElapsedMilliseconds { get { return (long)(RawTicks / TicksPerMillisecond); } }
        /// <summary>
        /// Retrieve the duration tracked by the stopwatch as a TimeSpan object
        /// </summary>
        public TimeSpan Elapsed { get { return new TimeSpan((long)( (RawTicks * TimeSpan.TicksPerMillisecond) / TicksPerMillisecond)); } }
        /// <summary>
        /// DateTime for the moment that the stopwatch was started, or null if it hasn't been started yet
        /// </summary>
        /// <remarks>This value is accurate over shorter periods of time, but due to the way time is 
        /// tracked, will eventually become inaccurate.</remarks>
        public DateTime? StartTime { 
            get {
                if (_startTimestamp.Value != 0) {
                    return PerfCounterRawDateTime().AddSeconds(_startTimestamp.Value / Stopwatch.Frequency);
                } else {
                    return null;
                }
            }
        }
        /// <summary>
        /// DateTime for the moment that the stopwatch was stopped, or null if it hasn't been stopped yet.
        /// </summary>
        /// <remarks>This value is accurate over shorter periods of time, but due to the way time is 
        /// tracked, will eventually become inaccurate.</remarks>
        public DateTime? EndTime {
            get {
                if (_stopTimestamp.Value != 0)
                {
                    return PerfCounterRawDateTime().AddSeconds(_stopTimestamp.Value / Stopwatch.Frequency);
                } else { 
                    return null;
                }
            }
        }
        /// <summary>
        /// This gives an accurate estimate of DateTime from ticks over shorter intervals, it won't be accurate over really long intervals though
        /// </summary>
        DateTime PerfCounterRawDateTime() {
            long result;
            Startwatch.QueryPerformanceCounter(out result);
            return DateTime.Now.AddSeconds(-result / Stopwatch.Frequency);
        }
        /// <summary>
        /// This returns false if the Stopwatch hasn't been started, or the Stopwatch has been stopped
        /// It returns true if the stopwatch is currently measuring a time interval
        /// </summary>
        public bool IsActive { get { return _stopTimestamp.Value == 0 && _startTimestamp.Value != 0; } }

        /// <summary>
        /// Uses an IDisposable to support stopping of an Startwatch, and provides an alternative reference 
        /// </summary>
        public sealed class DisposableController : IDisposable
        {
            internal DisposableController(Startwatch controlled) { Controlled = controlled; }
            /// <summary>
            /// Provides direct access to the stopwatch being controlled
            /// </summary>
            public readonly Startwatch Controlled;
            /// <summary>
            /// Disposing will call Stop() on the Startwatch which is being controlled
            /// </summary>
            public void Dispose()
            {
                Stop();
            }
            /// <summary>
            /// This will call Stop() on the Startwatch which is being controlled
            /// </summary>
            public bool Stop() { return Controlled.Stop(); }
        }
        /// <summary>
        /// Retrieve an object which implements IDisposable, that can be disposed to stop the Startwatch
        /// </summary>
        public DisposableController CreateController() { return new DisposableController(this); }
        /// <summary>
        /// Create a new stopwatch from time span in system tick units
        /// </summary>
        public static Startwatch FromTicks(long ticks)
        {
            return new Startwatch(0, ticks);
        }
        /// <summary>
        /// Create new stopwatch from time span in millisecond units
        /// </summary>
        public static Startwatch FromMilliseconds(long ms)
        {
            return new Startwatch(0, (long)(ms * TicksPerMillisecond));
        }
        /// <summary>
        /// Create new stopwatch from timespan object
        /// </summary>
        public static Startwatch FromTimeSpan(TimeSpan span)
        {
            return FromTimeSpanTicks(span.Ticks);
        }
        /// <summary>
        /// Create new stopwatch from time span measured in device independent tick units (100-ns units)
        /// </summary>
        public static Startwatch FromTimeSpanTicks(long deviceIndependentTicks)
        {
            return new Startwatch(0, (long)(deviceIndependentTicks * TicksPerMillisecond / TimeSpan.TicksPerMillisecond));
        }
        /// <summary>
        /// Initialize new Startwatch from System.Diagnostics.Stopwatch,
        /// if the System.Diagnostics.Stopwatch is actively running then the Startwatch will be running and have the same initial duration
        /// if the System.Diagnostics.Stopwatch is stopped then the Startwatch will be stopped
        /// the Startwatch will not be synchronized to the System.Diagnostics.Stopwatch in any way after it is created
        /// </summary>
        public static Startwatch FromStopwatch(Stopwatch watch)
        {
            if (watch.IsRunning)
            {
                long result;
                Startwatch.QueryPerformanceCounter(out result);
                return new Startwatch(result - watch.ElapsedTicks);
            }
            else
            {
                return new Startwatch(0, watch.ElapsedTicks);
            }
        }

        static bool QueryPerformanceCounter(out long value) {
            value = Stopwatch.GetTimestamp();
            return true;
        }
    }
}
