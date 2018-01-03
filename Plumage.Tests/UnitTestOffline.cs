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
            Assert.That(t.XMLDataIsValid, Is.False);
            Assert.That(t.CSVDataIsValid, Is.False);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
        }

        [Test]
        public void Test_A002_step_by_step_and_reset()
        {
            // Console.WriteLine(TESTFILES_DIR);
            TSDRReq t = new TSDRReq();
            Assert.That(t.XMLDataIsValid, Is.False);
            Assert.That(t.CSVDataIsValid, Is.False);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
            t.getXMLData(TESTFILES_DIR+"sn76044902.zip");
            Assert.That(t.XMLDataIsValid, Is.True);
            Assert.That(t.CSVDataIsValid, Is.False);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
            t.getCSVData();
            Assert.That(t.XMLDataIsValid, Is.True);
            Assert.That(t.CSVDataIsValid, Is.True);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
            t.getTSDRData();
            Assert.That(t.XMLDataIsValid, Is.True);
            Assert.That(t.CSVDataIsValid, Is.True);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.True);
            t.resetXMLData();
            Assert.That(t.XMLDataIsValid, Is.False);
            Assert.That(t.CSVDataIsValid, Is.False);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
        }

        [Test]
        public void Test_A003_typical_use()
        {
            TSDRReq t = new TSDRReq();
            t.getTSDRInfo(TESTFILES_DIR + "sn76044902.zip");
            Assert.That(t.XMLDataIsValid, Is.True);
            Assert.That(t.XMLData.Length, Is.EqualTo(30354));
            /*
            Python code to port:
        self.assertEqual(t.XMLData[0:55], r'<?xml version="1.0" encoding="UTF-8" standalone="yes"?>')
        self.assertEqual(t.ImageThumb[6:10], "JFIF")
        self.assertEqual(t.ImageFull[0:4], "\x89PNG")
        self.assertTrue(t.CSVDataIsValid)
        self.assertEqual(len(t.CSVData.split("\n")), 291)
        tsdrdata=t.TSDRData
        self.assertTrue(tsdrdata.TSDRMapIsValid)
        self.assertEqual(tsdrdata.TSDRSingle["ApplicationNumber"], "76044902")
        self.assertEqual(tsdrdata.TSDRSingle["ApplicationDate"], "2000-05-09-04:00")
        self.assertEqual(tsdrdata.TSDRSingle["ApplicationDate"][0:10],
                         tsdrdata.TSDRSingle["ApplicationDateTruncated"])
        self.assertEqual(tsdrdata.TSDRSingle["RegistrationNumber"], "2824281")
        self.assertEqual(tsdrdata.TSDRSingle["RegistrationDate"], "2004-03-23-05:00")
        self.assertEqual(tsdrdata.TSDRSingle["RegistrationDate"][0:10],
                         tsdrdata.TSDRSingle["RegistrationDateTruncated"])
        self.assertEqual(tsdrdata.TSDRSingle["MarkVerbalElementText"], "PYTHON")
        self.assertEqual(tsdrdata.TSDRSingle["MarkCurrentStatusExternalDescriptionText"],
                         "A Sections 8 and 15 combined declaration has been accepted and acknowledged.")
        self.assertEqual(tsdrdata.TSDRSingle["MarkCurrentStatusDate"], "2010-09-08-04:00")
        self.assertEqual(tsdrdata.TSDRSingle["MarkCurrentStatusDate"][0:10],
                         tsdrdata.TSDRSingle["MarkCurrentStatusDateTruncated"])
        applicant_list = tsdrdata.TSDRMulti["ApplicantList"]
        applicant_info = applicant_list[0]    
        self.assertEqual(applicant_info["ApplicantName"], "PYTHON SOFTWARE FOUNDATION")
        assignment_list = tsdrdata.TSDRMulti["AssignmentList"]
        assignment_0 = assignment_list[0] # Zeroth (most recent) assignment
        self.assertEqual(assignment_0["AssignorEntityName"],
                         "CORPORATION FOR NATIONAL RESEARCH INITIATIVES, INC.")
        self.assertEqual(assignment_0["AssignmentDocumentURL"],
                         "http://assignments.uspto.gov/assignments/assignment-tm-2849-0875.pdf")
        ## Diagnostic info
        self.assertEqual(tsdrdata.TSDRSingle["DiagnosticInfoXSLTFormat"], "ST.66")
        self.assertEqual(tsdrdata.TSDRSingle["DiagnosticInfoXSLTURL"],
                         "https://github.com/codingatty/Plumage")
        self.assertEqual(tsdrdata.TSDRSingle["DiagnosticInfoXSLTLicense"],
                         "Apache License, version 2.0 (January 2004)")
        self.assertEqual(tsdrdata.TSDRSingle["DiagnosticInfoXSLTSPDXLicenseIdentifier"],
                         "Apache-2.0")
        self.assertEqual(tsdrdata.TSDRSingle["DiagnosticInfoXSLTLicenseURL"],
                         "http://www.apache.org/licenses/LICENSE-2.0")
        self.assertEqual(tsdrdata.TSDRSingle["DiagnosticInfoImplementationURL"],
                         "https://github.com/codingatty/Plumage-py")
        self.assertEqual(tsdrdata.TSDRSingle["DiagnosticInfoImplementationVersion"],
                         "V. 1.2.0")
        self.assertEqual(tsdrdata.TSDRSingle["DiagnosticInfoImplementationLicenseURL"],
                         "http://www.apache.org/licenses/LICENSE-2.0")
        self.assertEqual(tsdrdata.TSDRSingle["DiagnosticInfoImplementationLicense"],
                         "Apache License, version 2.0 (January 2004)")
        self.assertEqual(tsdrdata.TSDRSingle["DiagnosticInfoImplementationName"],
                         "Plumage-py")
        self.assertEqual(tsdrdata.TSDRSingle["DiagnosticInfoImplementationSPDXLicenseIdentifier"],
                         "Apache-2.0")
             * */
        }

    }
}
