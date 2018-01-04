using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Plumage.Tests
{
    [TestFixture]
    public class UnitTestOffline
    {
        private string TESTFILES_DIR = "D:\\Development\\VisualStudio\\Plumage-dotnet\\Plumage.Tests\\testfiles\\";
        private string LINE_SEPARATOR = Environment.NewLine;

        // Group A: Basic exercises
        // Group B: XML fetch only
        // Group C: CSV creation
        // Group D: All the way through TSDR map
        // Group E: Parameter validations
        // Group F: XML/XSL variations
        // Group G: CSV/XSL validations

        // Group A
        // Basic exercises
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
            Assert.That(t.XMLData.Substring(0, 55), 
                Is.EqualTo("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"));
            string JFIF_tag = System.Text.Encoding.UTF8.GetString(t.ImageThumb, 6, 4);
            // Console.WriteLine(JFIF_tag);
            Assert.That(JFIF_tag, Is.EqualTo("JFIF"));
            // PNG tag is "\x89PNG"; let's do this in two steps
            string PNG_tag = System.Text.Encoding.UTF8.GetString(t.ImageFull, 1, 3);
            Assert.That(t.ImageFull[0], Is.EqualTo(0X89));
            Assert.That(PNG_tag, Is.EqualTo("PNG"));
            // note: this fails:
            // string PNG_tag_fails = System.Text.Encoding.UTF8.GetString(t.ImageFull, 0, 4);
            // Assert.That(PNG_tag_fails, Is.EqualTo("\x89PNG"));
            Assert.That(t.CSVDataIsValid, Is.True);
            Assert.That(t.CSVData.Split(new string[] {LINE_SEPARATOR},
                StringSplitOptions.None).Length, Is.EqualTo(291));
            TSDRMap tsdrdata = t.TSDRData;
            Assert.That(tsdrdata.TSDRMapIsValid, Is.True);
            Assert.That(tsdrdata.TSDRSingle["ApplicationNumber"], Is.EqualTo("76044902"));
            Assert.That(tsdrdata.TSDRSingle["ApplicationDate"], Is.EqualTo("2000-05-09-04:00"));
            Assert.That(tsdrdata.TSDRSingle["ApplicationDate"].Substring(0,10), 
                Is.EqualTo(tsdrdata.TSDRSingle["ApplicationDateTruncated"]));
            Assert.That(tsdrdata.TSDRSingle["RegistrationNumber"], Is.EqualTo("2824281"));
            Assert.That(tsdrdata.TSDRSingle["RegistrationDate"], Is.EqualTo("2004-03-23-05:00"));
            Assert.That(tsdrdata.TSDRSingle["RegistrationDate"].Substring(0, 10),
                Is.EqualTo(tsdrdata.TSDRSingle["RegistrationDateTruncated"]));
            Assert.That(tsdrdata.TSDRSingle["MarkVerbalElementText"], Is.EqualTo("PYTHON"));
            Assert.That(tsdrdata.TSDRSingle["MarkCurrentStatusExternalDescriptionText"], 
                Is.EqualTo("A Sections 8 and 15 combined declaration has been accepted and acknowledged."));
            Assert.That(tsdrdata.TSDRSingle["MarkCurrentStatusDate"], Is.EqualTo("2010-09-08-04:00"));
            Assert.That(tsdrdata.TSDRSingle["MarkCurrentStatusDate"].Substring(0, 10),
                Is.EqualTo(tsdrdata.TSDRSingle["MarkCurrentStatusDateTruncated"]));
            List<Dictionary<string, string>> applicant_list = tsdrdata.TSDRMulti["ApplicantList"];
            Dictionary<string, string> applicant_info = applicant_list[0];
            Assert.That(applicant_info["ApplicantName"], Is.EqualTo("PYTHON SOFTWARE FOUNDATION"));
            List<Dictionary<string, string>> assignment_list = tsdrdata.TSDRMulti["AssignmentList"];
            Dictionary<string, string> assignment_0 = assignment_list[0]; ; // # Zeroth (most recent) assignment
            Assert.That(assignment_0["AssignorEntityName"], Is.EqualTo("CORPORATION FOR NATIONAL RESEARCH INITIATIVES, INC."));
            Assert.That(assignment_0["AssignmentDocumentURL"], Is.EqualTo("http://assignments.uspto.gov/assignments/assignment-tm-2849-0875.pdf"));
            // Diagnostic info

            Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoXSLTFormat"], Is.EqualTo("ST.66"));
            Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoXSLTURL"], Is.EqualTo("https://github.com/codingatty/Plumage"));
            Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoXSLTLicense"], Is.EqualTo("Apache License, version 2.0 (January 2004)"));
            Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoXSLTSPDXLicenseIdentifier"], Is.EqualTo("Apache-2.0"));
            Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoXSLTLicenseURL"], Is.EqualTo("http://www.apache.org/licenses/LICENSE-2.0"));
            Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoImplementationURL"], Is.EqualTo("https://github.com/codingatty/Plumage-dotnet"));
            Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoImplementationVersion"], Is.EqualTo("1.2.0"));
            Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoImplementationLicenseURL"], Is.EqualTo("http://www.apache.org/licenses/LICENSE-2.0"));
            Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoImplementationLicense"], Is.EqualTo("Apache License, version 2.0 (January 2004)"));
            Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoImplementationName"], Is.EqualTo("Plumage-dotnet"));
            Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoImplementationSPDXLicenseIdentifier"], Is.EqualTo("Apache-2.0"));
        }

        // Group B
        // Test XML fetch only
        [Test]
        public void Test_B001_step_by_step_thru_xml()
        {
            TSDRReq t = new TSDRReq();
            Assert.That(t.XMLDataIsValid, Is.False);
            Assert.That(t.CSVDataIsValid, Is.False);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
            t.getXMLData(TESTFILES_DIR + "sn76044902.zip");
            Assert.That(t.XMLDataIsValid, Is.True);
            Assert.That(t.CSVDataIsValid, Is.False);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
        }

        // Group C
        // Test through CSV creation
        [Test]
        public void Test_C001_step_by_step_thru_csv()
        {
            // Console.WriteLine(TESTFILES_DIR);
            TSDRReq t = new TSDRReq();
            Assert.That(t.XMLDataIsValid, Is.False);
            Assert.That(t.CSVDataIsValid, Is.False);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
            t.getXMLData(TESTFILES_DIR + "sn76044902.zip");
            Assert.That(t.XMLDataIsValid, Is.True);
            Assert.That(t.CSVDataIsValid, Is.False);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
            t.getCSVData();
            Assert.That(t.XMLDataIsValid, Is.True);
            Assert.That(t.CSVDataIsValid, Is.True);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
        }

        // Group D
        // Test all the way through TSDR map
        [Test]
        public void Test_D001_step_by_step_thru_map()
        {
            // Console.WriteLine(TESTFILES_DIR);
            TSDRReq t = new TSDRReq();
            Assert.That(t.XMLDataIsValid, Is.False);
            Assert.That(t.CSVDataIsValid, Is.False);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
            t.getXMLData(TESTFILES_DIR + "sn76044902.zip");
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
        }

    }
}
