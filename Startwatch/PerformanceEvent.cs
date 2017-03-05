using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StartwatchDiagnostics
{
    sealed class PerformanceEvent : IPerformanceEvent
    {
        internal PerformanceEvent(Startwatch watch)
        {
            _watch = watch;
            _behaviors = new List<Action<IUnstoppablePerformanceEvent>>();
        }
        int _behaviorIndex = 0;
        readonly List<Action<IUnstoppablePerformanceEvent>> _behaviors;
        readonly internal Startwatch _watch;
        
        public IReadonlyStartwatch TimeData { get { return _watch; } }
        
        public IUnstoppablePerformanceEvent WhenComplete(Action<IUnstoppablePerformanceEvent> behv) { _behaviors.Add(behv); return this; }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EventCompleted()
        {
            _watch.Stop();
            int len = _behaviors.Count;
            while (true)
            {
                var index =  _behaviorIndex++;
                if (len <= index) { break; }
                _behaviors[index](this);
            }
        }
    }
}
