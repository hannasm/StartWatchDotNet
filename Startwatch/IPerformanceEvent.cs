using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartwatchDiagnostics
{
    /// <summary>
    /// Represents a <see cref="IUnstoppablePerformanceEvent"/> which 
    /// can be completed by client code. In some situations the PerformanceEvents API
    /// intentionally forbids completing a performance event by client code
    /// either because it would cause unpredictable or confusing behavior.
    /// </summary>
    public interface IPerformanceEvent : IUnstoppablePerformanceEvent
    {
        void EventCompleted();
    }
}
