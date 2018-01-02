using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class UnitTestOffline
    {
        [TestMethod]
        public void Test_A000_null()
        {
            Plumage.TSDRReq t = new Plumage.TSDRReq();
            Assert.IsFalse(t.XMLDataIsValid);
            Assert.IsFalse(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
            string cd = System.IO.Directory.GetCurrentDirectory();
            // Console.WriteLine("In Test_A000_null");
            // Console.WriteLine(cd);
            // CD: D:\Development\VisualStudio\Plumage-dotnet\UnitTests\bin\Debug
            // var z = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
