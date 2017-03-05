using ExpressiveLogging.V1;
using ExpressiveLogging.V1.Counters;
using System;
using System.Collections.Generic;
using StartwatchDiagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressiveLogging
{
    public static class StartwatchLoggingExtensions
    {
        public static void IncrementCounterBy(this ILogStream log, ILogToken lt, INamedCounterToken ct, Startwatch value)
        {
            log.IncrementCounterBy(lt, ct, value.ElapsedTicks);
        }
        public static void IncrementCounterBy(this ILogStream log, IRawCounterToken ct, Startwatch value)
        {
            log.IncrementCounterBy(ct, value.ElapsedTicks);
        }
    }
}
