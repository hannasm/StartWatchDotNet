using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StartwatchDiagnostics.Tests
{
    [TestClass]
    public class StartwatchExceptionsTests : TestBase
    {
        [TestMethod]
        public void TestLastSiblingParentNotNull()
        {
            try {
                var stopwatch = new Startwatch();
                var child = stopwatch.CreateChild();
                var lastSibling = child.CreateLastSibling(null);
                Assert.Fail("Expected an exception to be thrown before reaching here");
            }
            catch (ArgumentNullException eError) {
                Assert.AreEqual("parent", eError.ParamName, "parameter name of exception");
            }
        }
    }
}
