using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartwatchDiagnostics
{
    /// <summary>
    /// Represents an event with a duration, meaning it has both a start, and an end.
    /// The event may be configured to perform various actions, 
    /// especially writing message to the logging infrastructure.
    /// </summary>
    public interface IUnstoppablePerformanceEvent
    {
        /// <summary>
        /// Timing data for the event
        /// </summary>
        IReadonlyStartwatch TimeData { get; }

        /// <summary>
        /// Registers an action to take place when the event completes.
        /// </summary>
        IUnstoppablePerformanceEvent WhenComplete(Action<IUnstoppablePerformanceEvent> behv);
    }
}
