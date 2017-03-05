using ExpressiveLogging.V1;
using ExpressiveLogging.V1.AssertableLogging;
using ExpressiveLogging.V1.CompositeLogging;
using ExpressiveLogging.V1.SystemDiagnosticsLogging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartwatchDiagnostics.Tests
{
    public abstract class PerformanceEventTestBase : TestBase
    {
        public PerformanceEventTestBase()
        {
            _assertLogger = new Lazy<AssertableLogStream>(() => new AssertableLogStream(() => { throw new AssertFailedException("while checking logstream"); }));
            _logger = new Lazy<ILogStream>(()=>
                CompositeLogStream.Create(
                    GetAssertLogger(),
                    new DebugLogStream()
                )
            );
        }

        protected override bool DoPreJit()
        {
            return false;
        }

        public override ILogStream CreateLogger()
        {
            return CompositeLogStream.Create(
                new DebugLogStream()
            );
        }

        readonly Lazy<AssertableLogStream> _assertLogger; 
        public AssertableLogStream GetAssertLogger() { return _assertLogger.Value; }
        readonly Lazy<ILogStream> _logger;
        public override ILogStream GetLogger()
        {
            return _logger.Value;
        }
    }
}
