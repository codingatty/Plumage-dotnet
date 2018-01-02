using NUnit.Framework;
using System;

namespace Plumage.Tests
{
    [TestFixture]
    public class UnitTestOffline
    {
        [Test]
        public void Test_A000_null()
        {
            TSDRReq t = new TSDRReq();
            Assert.False(t.XMLDataIsValid);
            Assert.False(t.CSVDataIsValid);
            Assert.False(t.TSDRData.TSDRMapIsValid);
            string cd = System.IO.Directory.GetCurrentDirectory();
            Console.WriteLine("In Test_A000_null");
            Console.WriteLine(cd);
            string a = TestContext.CurrentContext.TestDirectory;
            Console.WriteLine(a);
        }
    }
}
