using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartwatchDiagnostics
{
    /// <summary>
    /// Exposes the data associated with an Startwatch without exposing any methods or properties that might change the state of it
    /// </summary>
    public interface IReadonlyStartwatch
    {
        /// <summary>
        /// Retrieve the duration tracked by the stopwatch in ticks
        /// </summary>
        long ElapsedTicks { get; }
        /// <summary>
        /// Retrieve the duration tracked by the stopwatch in milliseconds
        /// </summary>
        long ElapsedMilliseconds { get; }
        /// <summary>
        /// Retrieve the duration tracked by the stopwatch as a TimeSpan object
        /// </summary>
        TimeSpan Elapsed { get; }
        /// <summary>
        /// DateTime for the moment that the stopwatch was started, or null if it hasn't been started yet
        /// </summary>
        /// <remarks>This value is accurate over shorter periods of time, but due to the way time is 
        /// tracked, will eventually become inaccurate.</remarks>
        DateTime? StartTime { get; }
        /// <summary>
        /// DateTime for the moment that the stopwatch was stopped, or null if it hasn't been stopped yet.
        /// </summary>
        /// <remarks>This value is accurate over shorter periods of time, but due to the way time is 
        /// tracked, will eventually become inaccurate.</remarks>
        DateTime? EndTime { get; }

        /// <summary>
        /// This returns false if the Stopwatch hasn't been started, or the Stopwatch has been stopped
        /// It returns true if the stopwatch is currently measuring a time interval
        /// </summary>
        bool IsActive { get; }
    }
}
