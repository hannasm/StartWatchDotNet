using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartwatchDiagnostics
{
    /// <summary>
    /// Provides a configurable interface for creating performance events in a consistent hierarchy 
    /// and then writing them to the underlying logging infrastructure.
    /// </summary>
    public sealed class PerformanceEventTracker
    {
        readonly Stack<PerformanceEvent> _hierarchy;
        PerformanceEvent _current;
        readonly PerformanceEvent _setupEvent, _beforeFirstEvent, _realTotalEvent;
        
        /// <summary>
        /// Time spent during creationof the PerformanceEventTracker
        /// </summary>
        public IUnstoppablePerformanceEvent SetupEvent { get { return _setupEvent; } }
        /// <summary>
        /// Time spent between creating performance event tracker and starting the first event 
        /// </summary>
        public IUnstoppablePerformanceEvent BeforeFirstEvent { get { return _beforeFirstEvent; } }
        /// <summary>
        /// Timing data inclusive of setup time for performance event tracker and the time
        /// waiting for the first performance event. This measurement is generally 
        /// less useful than TotalEvent.
        /// </summary>
        public IUnstoppablePerformanceEvent SetupInclusiveTotalEvent { get { return _realTotalEvent; } }

        /// <summary>
        /// The total amount of time between the first event and the completion of the last event.
        /// </summary>
        public IUnstoppablePerformanceEvent TotalEvent
        {
            get {
                return new PerformanceEvent(
                    // watch starts when 'before first event' stops, and stops when 'real total event' stops
                    _beforeFirstEvent._watch.CreateLastSibling(_realTotalEvent._watch));
            }
        }

        public PerformanceEventTracker()
        {
            var setupWatch = new Startwatch();
            _setupEvent = new PerformanceEvent(setupWatch);

            var realTotalWatch = setupWatch.CreateSibling();
            _realTotalEvent = new PerformanceEvent(realTotalWatch);

            var beforeFirstWatch = realTotalWatch.CreateChild();
            _beforeFirstEvent = new PerformanceEvent(beforeFirstWatch);

            _current = _beforeFirstEvent;
            _hierarchy = new Stack<PerformanceEvent>();
            _hierarchy.Push(_realTotalEvent);
            _setupEvent.EventCompleted();
        }

        /// <summary>
        /// Create a scoped event that starts at the same time as the previous event completed.
        /// If the previous event was stopped prior to calling NextEvent(), the time for the event
        /// will still include all time since the prior event stopping.
        /// If the previous event was not stopped prior to calling NextEvent(), the previous event
        /// will be stopped automatically.
        /// </summary>
        public IUnstoppablePerformanceEvent NextEvent()
        {
            _current.EventCompleted();
            _current = new PerformanceEvent(_current._watch.CreateSibling());
            return _current;
        }
        /// <summary>
        /// Create a scoped event that starts at the time of the call to NewEvent(), this can
        /// differ from the behavior of NextEvent() in subtle ways.
        /// If the previous event was not stopped prior to calling NewEvent(), the previous event
        /// will be stopped automatically.
        /// </summary>
        public IUnstoppablePerformanceEvent NewEvent()
        {
            _current.EventCompleted();
            return _current = new PerformanceEvent(new Startwatch());
        }

        /// <summary>
        /// Creates a scoped event that is defined by the provided stopwatch. The event will be managed
        /// in the same way as other performance events, but some behavior is going to be 
        /// dependent on the state of the provided stopwatch. 
        /// If the previous event was not stopped prior to calling NewEvent(), the previous event
        /// will be stopped automatically.
        /// </summary>
        public IUnstoppablePerformanceEvent AddCustomEvent(Startwatch watch)
        {
            _current.EventCompleted();
            return _current = new PerformanceEvent(watch);
        }
        /// <summary>
        /// Create new scoped event that starts at the same time as the current one, but will have an unlinked
        /// stopping time, and can in turn create other siblings for further tracking of time information.
        /// </summary>
        /// <remarks>This can have better performance and more accurate measurements than PushEvent() when circumstances permit it.</remarks>
        public IUnstoppablePerformanceEvent PushFirstEvent()
        {
            if (_current == _beforeFirstEvent) { NextEvent(); }

            var newWatch = _current._watch.CreateChild();
            _hierarchy.Push(_current);
            return _current = new PerformanceEvent(newWatch);
        }
        /// <summary>
        /// Create new scoped event pushing the current one onto the stack. Start / stop times will not
        /// be linked but the other timers on the stack will continue measuring time.
        /// </summary>
        public IUnstoppablePerformanceEvent PushEvent()
        {
            if (_current == _beforeFirstEvent) { NextEvent(); }

            _hierarchy.Push(_current);
            return _current = new PerformanceEvent(new Startwatch());
        }
        /// <summary>
        /// Create new scoped event pushing the current one onto the stack. Start / stop times will not
        /// be linked but the other timers on the stack will continue measuring time. The pushed event
        /// will be defined by the provided stopwatch instead of one created by the performance tracker.
        /// The event will be managed in the same way as other events, but some behavior will be dependent
        /// on the state of the provided stopwatch.
        /// </summary>
        public IUnstoppablePerformanceEvent PushCustomEvent(Startwatch watch)
        {
            if (_current == _beforeFirstEvent) { NextEvent(); }

            _hierarchy.Push(_current);
            return _current = new PerformanceEvent(watch);
        }

        /// <summary>
        /// Create new scoped event, where the stop time of the event will be linked to the stop time of the one on the top of the stack.
        /// The current scoped event timer will be stopped.
        /// The new 'current' scoped event will be the one which was on the top of the stack.
        /// The return value of the function is the new scoped event that was created.
        /// </summary>
        /// <remarks>One would generally use this for the last event in the scope of some series of events being tracked.</remarks>
        public IUnstoppablePerformanceEvent PopLastEvent()
        {
            _current.EventCompleted();
            PerformanceEvent next;
            if (_hierarchy.Peek() == _realTotalEvent) { next = _current; }
            else {next = _hierarchy.Pop(); }

            var result = new PerformanceEvent(_current._watch.CreateLastSibling(next._watch));
            _current = next;

            // link the completion behaviors between the two so they coincide
            _current.WhenComplete(evt => result.EventCompleted());
            result.WhenComplete(evt => _current.EventCompleted());

            return result;
        }

        /// <summary>
        /// Changes the current event to the one currently on the top of the stack. The previous current event will be stopped.
        /// </summary>
        public IUnstoppablePerformanceEvent PopEvent()
        {
            _current.EventCompleted();
            if (_hierarchy.Peek() == _realTotalEvent) { return _current; }
            return _current = _hierarchy.Pop();
        }


        /// <summary>
        /// Pops all events from the hierarchy up to and including the specified event. 
        /// </summary>
        public IUnstoppablePerformanceEvent PopToEvent(IUnstoppablePerformanceEvent @event)
        {
            return PopToEvent(@event, 0);
        }

        /// <summary>
        /// Pops all events from the hierarchy up to and including the specified event. 
        /// And will then pop <paramref name="plus"/> more
        /// </summary>
        public IUnstoppablePerformanceEvent PopToEvent(IUnstoppablePerformanceEvent @event, int plus)
        {
            if (_hierarchy.Count == 1)
            {
                _current.EventCompleted();
                return _current;
            }
            bool ready = false;
            do
            {
                if (object.ReferenceEquals(_current, @event))
                {
                    ready = true;
                }
                if (ready)
                {
                    if (plus <= 0) { break; }
                    plus -= 1;
                }

                _current.EventCompleted();
                if (_hierarchy.Count <= 1) { break; }
                _current = _hierarchy.Pop();
            } while (true);
            return _current;
        }

        /// <summary>
        /// Pops all events from the hierarchy up to and including the specified event, and sets
        /// the current element to it's parent. The specified event will be completed in this process
        /// but the parent will not.
        /// </summary>
        public IUnstoppablePerformanceEvent PopComplete(IUnstoppablePerformanceEvent @event)
        {
            return PopToEvent(@event, 1);
        }

        /// <summary>
        /// Completes the current event
        /// </summary>
        public IUnstoppablePerformanceEvent CompleteEvent()
        {
            _current.EventCompleted();
            return _current;
        }

        /// <summary>
        /// Completes the total timer on the performance tracker, as well as all other active events  currently tracked,
        /// and pops all events from the monitoring hierarchy
        /// </summary>
        /// <returns><see cref="TotalEvent" /></returns>
        public IUnstoppablePerformanceEvent Complete()
        {
            _current.EventCompleted();
            while (_hierarchy.Count > 1) { 
                _current = _hierarchy.Pop();

                // calling event completed won't always be necesarry because some events may be linked
                // but it's safe to call this multiple times, so we will go ahead and do it
                _current.EventCompleted(); 
            }

            _realTotalEvent.EventCompleted();
            return TotalEvent;
        }

        public int GetStackSize()
        {
            return _hierarchy.Count;
        }
    }
}
