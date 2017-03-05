using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StartwatchDiagnostics.Tests
{
    /// <summary>
    /// Summary description for ReadmeTests
    /// </summary>
    [TestClass]
    public class ReadmeTests
    {
        public ReadmeTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void test001()
        {
            var executionTime = Startwatch.StartNew();

            /* execute interesting code here */
            System.Threading.Thread.Sleep(new System.Random().Next(10, 20));

            executionTime.Stop();

            Console.WriteLine("Total Ticks {0}", executionTime.ElapsedTicks);
            Console.WriteLine("Total Milliseconds {0}", executionTime.ElapsedMilliseconds);
            Console.WriteLine("Total Timespan {0}", (TimeSpan)executionTime.Elapsed);
            Console.WriteLine("Start Time {0}", (DateTime?)executionTime.StartTime);
            Console.WriteLine("End Time {0}", (DateTime?)executionTime.EndTime);
        }
        [TestMethod]
        public void Test002()
        {
            var tracker = new PerformanceEventTracker();

            var setup = tracker.NextEvent().WhenComplete(e => Console.WriteLine("Setup completed in {0}", e.TimeData.ElapsedMilliseconds));

            /* execute interesting code here */
            System.Threading.Thread.Sleep(new System.Random().Next(10, 20));

            var totalTimer = tracker.NextEvent().WhenComplete(e => Console.WriteLine("Loop completed in {0}", e.TimeData.ElapsedMilliseconds));
            var loopInitTimer = tracker.PushFirstEvent().WhenComplete(e => Console.WriteLine("Loop init in {0}", e.TimeData.ElapsedMilliseconds));
            for (int i = 0; i < 100; i++)
            {
                var loopTimer = tracker.NextEvent().WhenComplete(e => Console.WriteLine("Loop timer in {0}", e.TimeData.ElapsedMilliseconds));

                /* execute interesting code here */
                System.Threading.Thread.Sleep(new System.Random().Next(10, 20));


            }
            tracker.PopLastEvent().WhenComplete(e => Console.WriteLine("Loop teardown in {0}", e.TimeData.ElapsedMilliseconds));
            var teardown = tracker.NextEvent().WhenComplete(e => Console.WriteLine("Teardown in {0}", e.TimeData.ElapsedMilliseconds));

            /* execute interesting code here */
            System.Threading.Thread.Sleep(new System.Random().Next(10, 20));

            tracker.Complete().WhenComplete(e => Console.WriteLine("Total duration {0}", e.TimeData.ElapsedMilliseconds));
        }
    }
}
