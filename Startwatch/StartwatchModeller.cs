using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartwatchDiagnostics
{
    /// <summary>
    /// Provides the ability to create a mapping for a complex timing hierarchy
    /// up-front, and then execute the complete hierarchy sequentially through
    /// repeated calls to a single, simplified advancement function like Next()
    /// </summary>
    public sealed class StartwatchModeller
    {
        /// <summary>
        /// Create a new Stopwatch Modeller ready to map a timing hierarchy
        /// </summary>
        /// <returns></returns>
        public static StartwatchModeller Create()
        {
            return new StartwatchModeller();
        }

        StartwatchModeller() {
            SetupTimer = _current = new Startwatch();
            _parents = new Stack<Startwatch>();
            _tracking = new List<Startwatch>();
            TotalTimer = AddNext();
            BeforeStart = PushChild();
            SetupTimer.Stop();
        }

        /// <summary>
        /// Automatically created timer that tracks how much time was spent creating the modeler
        /// </summary>
        public readonly Startwatch SetupTimer;
        /// <summary>
        /// Automatically created timer that tracks how much time was spent during the entire 
        /// modelled performance hierarchy
        /// </summary>
        public readonly Startwatch TotalTimer;
        /// <summary>
        /// Automatically created timer that track the total amount of time spent before the advancement
        /// function was first executed.
        /// </summary>
        public readonly Startwatch BeforeStart;
        readonly Stack<Startwatch> _parents;
        readonly List<Startwatch> _tracking; 
        Startwatch _current;

        /// <summary>
        /// Create a new timer that is sequential to the previous one. 
        /// A call to next is required for each call of this function (to stop the timer).
        /// </summary>
        public Startwatch AddNext()
        {
            if (_current == null) { throw new InvalidModellerStateException(); }
            var result = _current = _current.CreateSibling();   
            _tracking.Add(result);
            return result;
        }
        /// <summary>
        /// Create a new timer which is the first child of the parent. 
        /// A call to next is required for each call to this function (to stop the timer).
        /// </summary>
        public Startwatch PushChild()
        {
            if (_current == null) { throw new InvalidModellerStateException(); }
            _parents.Push(_current);
            var result = _current = _current.CreateChild();
            _tracking[_tracking.Count-1] = result; // overwrite parent in _tracking
            return result;
        }
        /// <summary>
        /// Create a new timer which is the last child of a parent. 
        /// A call to next <b>is not</b> required for each call to this function.
        /// The timer created by this function will never be returned by the advancement function.
        /// </summary>
        public Startwatch PopNext()
        {
            if (_current == null) { throw new InvalidModellerStateException(); }
            var result = _current = _current.CreateLastSibling(_parents.Peek());
            // since it's the last sibling automatically pop the scope back to the parent
            Pop();
            return result;
        }
        /// <summary>
        /// Synonym for <see cref="PopNext"/> that has a more inuitive name...
        /// </summary>
        public Startwatch PopNextSibling()
        {
            return PopNext();
        }

        /// <summary>
        /// Goes up the hierarchy of timers to the parent without creating an explicit timer.
        /// A call to next <b>is not</b> required after calling this function because
        /// no timer is created.
        /// </summary>
        public StartwatchModeller Pop() {
            if (_current == null) { throw new InvalidModellerStateException(); }
            // don't allow user to go outside the 'total'
            if (_parents.Peek() == TotalTimer) { return this; }

            var result = _current = _parents.Pop();
            _tracking.Add(result);
            return this;
        }
        
        /// <summary>
        /// Create the advancement function which will generate the sequence of timers.
        /// Calling this function will automatically close the entire timer hierarchy.
        /// After calling this function, the modeller will become invalidated
        /// and no other functionality will be available.
        /// </summary>
        /// <para>
        /// The first call to the advancement function will start the timer modelling
        /// hierarchy, and each subsequent call will stop the previously returned timer,
        /// and return the next one. The advancement function will eventually
        /// reach the end of the modelled hierarchy, at which point it will
        /// return the <see cref="TotalTimer" /> on each call. It is up
        /// to the caller to stop the TotalTimer at the appropriate point,
        /// which may either be immediately upon seeing it or
        /// at some final point later in the timer hierarchy.
        /// </para>
        public Func<Startwatch> CreateAdvancementFunction()
        {
            if (_current == null) { throw new InvalidModellerStateException(); }
            while (_parents.Count > 1) { Pop(); }
            _current = null;

            var enum1 = _tracking.GetEnumerator();
            if (_tracking[0] != BeforeStart) {
                throw new InvalidOperationException("Modeller tracking state corrupted");
            }
            enum1.MoveNext();

            var enumerator = AdvancementEnumerable(enum1).GetEnumerator();
            return ()=>{ 
                if (enumerator.MoveNext()) { return enumerator.Current; }
                enumerator.Dispose();
                return TotalTimer;
            };
        }
        /// <summary>
        /// This implements all the confusing logic about stopping the timers at the right times
        /// </summary>
        IEnumerable<Startwatch> AdvancementEnumerable(IEnumerator<Startwatch> enumerator)
        {
            Startwatch prev = BeforeStart;

            while (enumerator.MoveNext())
            {
                // don't stop the timer if it's stop event is linked to the next one
                prev.Stop();
                yield return prev = enumerator.Current;
            }

            if (prev != null) { prev.Stop(); }
            enumerator.Dispose();
        }

        /// <summary>
        /// Exception which is thrown by the modeller after the advancement function has been created.
        /// </summary>
        public class InvalidModellerStateException : Exception
        {
            internal InvalidModellerStateException() :
                base("After generating an advancement function, the modeller may not be used for this action")
            {}
        }
    }
}
