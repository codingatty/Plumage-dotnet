using NUnit.Framework;
using System;

namespace Plumage.Tests
{
    [TestFixture]
    public class UnitTestOffline
    {
        private string TESTFILES_DIR = "D:\\Development\\VisualStudio\\Plumage-dotnet\\Plumage.Tests\\testfiles\\";

        [Test]
        public void Test_A001_test_initialize()
        // Simple test, just verify TSDRReq can be initialized correctly
        {
            TSDRReq t = new TSDRReq();
            Assert.False(t.XMLDataIsValid);
            Assert.False(t.CSVDataIsValid);
            Assert.False(t.TSDRData.TSDRMapIsValid);
        }

        [Test]
        public void Test_A002_step_by_step()
        {
            // Console.WriteLine(TESTFILES_DIR);
            TSDRReq t = new TSDRReq();
            Assert.False(t.XMLDataIsValid);
            Assert.False(t.CSVDataIsValid);
            Assert.False(t.TSDRData.TSDRMapIsValid);
            t.getXMLData(TESTFILES_DIR+"sn76044902.zip");
            Assert.True(t.XMLDataIsValid);
            Assert.False(t.CSVDataIsValid);
            Assert.False(t.TSDRData.TSDRMapIsValid);
            t.getCSVData();
            Assert.True(t.XMLDataIsValid);
            Assert.True(t.CSVDataIsValid);
            Assert.False(t.TSDRData.TSDRMapIsValid);
            t.getTSDRData();
            Assert.True(t.XMLDataIsValid);
            Assert.True(t.CSVDataIsValid);
            Assert.True(t.TSDRData.TSDRMapIsValid);
            t.resetXMLData();
            Assert.False(t.XMLDataIsValid);
            Assert.False(t.CSVDataIsValid);
            Assert.False(t.TSDRData.TSDRMapIsValid);
        }
    }
}
