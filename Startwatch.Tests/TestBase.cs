using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressiveLogging.SystemDiagnosticsLogging;
using ExpressiveLogging;
using ExpressiveAssertions;

namespace StartwatchDiagnostics.Tests
{
    public abstract class TestBase
    {
        public virtual ILogStream CreateLogger() {
            return new DebugLogStream();
        }

        ILogStream _defaultLogger;
        public virtual ILogStream GetLogger()
        {
            if (_defaultLogger == null) {
                _defaultLogger = CreateLogger();
            }
            return _defaultLogger;
        }

        protected virtual bool DoPreJit()
        {
            return true;
        }

        public IAssertionTool _assert = ExpressiveAssertions.Tooling.ShortAssertionRendererTool.Create(
            ExpressiveAssertions.MSTest.MSTestAssertionTool.Create()
        );

        [TestInitialize]
        public virtual void Setup()
        {
            if (DoPreJit())
            {
                // pre jit everything...?
                foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()))
                {
                    foreach (var method in type.GetMethods(BindingFlags.DeclaredOnly |
                                        BindingFlags.NonPublic |
                                        BindingFlags.Public | BindingFlags.Instance |
                                        BindingFlags.Static))
                    {
                        try
                        {
                            System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(method.MethodHandle);
                        }
                        catch { }
                    }
                }
            }

            _measurementError = Stopwatch.Frequency / 1000; // ticks per millisecond
            _measurementError *= 2; // 5 millisecond threshold...?
        }
        long _measurementError;
        public long MEASUREMENT_ERROR { get { return _measurementError; } }

        [TestCleanup]
        public virtual void Cleanup()
        {
            GetLogger().Dispose();
        }

        public void AssertStopwatch(Startwatch expected, IReadonlyStartwatch actual)
        {
            AssertStopwatch(expected.ElapsedTicks, actual);
        }
        public void AssertStopwatch(Startwatch watch1, IReadonlyStartwatch actual, string message, params object[] fmt)
        {
            AssertStopwatch(watch1.ElapsedTicks, actual, message, fmt);
        }
        public void AssertStopwatch(Stopwatch expected, IReadonlyStartwatch actual)
        {
            AssertStopwatch(expected.ElapsedTicks, actual);
        }
        public void AssertStopwatch(Stopwatch watch1, IReadonlyStartwatch actual, string message, params object[] fmt)
        {
            AssertStopwatch(watch1.ElapsedTicks, actual, message, fmt);
        }
        public void AssertStopwatch(long expectedTicks, IReadonlyStartwatch actual)
        {
            this.AssertStopwatch(expectedTicks, actual, null);
        }
        public void AssertStopwatch(long expectedTicks, IReadonlyStartwatch actual, string message, params object[] fmt)
        {
            if (expectedTicks + MEASUREMENT_ERROR < actual.ElapsedTicks || expectedTicks - MEASUREMENT_ERROR > actual.ElapsedTicks)
            {
                var msg = string.Format("Expected: {2}ms Actual: {3}ms Within {0} ticks ({1} ms). ", MEASUREMENT_ERROR, (MEASUREMENT_ERROR + Stopwatch.Frequency/1000) / (Stopwatch.Frequency/1000),
                    expectedTicks * 1000 / Stopwatch.Frequency, actual.ElapsedTicks * 1000 / Stopwatch.Frequency);
                if (message != null && fmt != null)
                {
                    msg += string.Format(message, fmt);
                }
                Assert.AreEqual(expectedTicks, actual.ElapsedTicks, msg);
            }
        }

    }
}
