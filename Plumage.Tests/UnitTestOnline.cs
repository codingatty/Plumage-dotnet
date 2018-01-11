using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plumage.Tests
{
    [TestFixture]
    public class UnitTestOnline
    {

        private void validate_sample(TSDRReq t)
        {
            /*
            Test to confirm content for Python trademark; whether as
            app no. 76/044,902 or ser. no. 2824281

            Note this is analogous to UnitTestOffline.Test_A003_typical_use, but limited to 
            stable data
            */
            Assert.That(t.XMLDataIsValid, Is.True);
            Assert.That(t.XMLData.Substring(0, 55),
                Is.EqualTo("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"));
            Assert.That(t.CSVDataIsValid, Is.True);
            TSDRMap tsdrdata = t.TSDRData;
            Assert.That(tsdrdata.TSDRMapIsValid, Is.True);
            Assert.That(tsdrdata.TSDRSingle["ApplicationNumber"], Is.EqualTo("76044902"));
            Assert.That(tsdrdata.TSDRSingle["ApplicationDate"], Is.EqualTo("2000-05-09-04:00"));
            Assert.That(tsdrdata.TSDRSingle["ApplicationDate"].Substring(0, 10),
                Is.EqualTo(tsdrdata.TSDRSingle["ApplicationDateTruncated"]));
            Assert.That(tsdrdata.TSDRSingle["RegistrationNumber"], Is.EqualTo("2824281"));
            if (tsdrdata.TSDRSingle["DiagnosticInfoXSLTFormat"] == "ST.96")
            {
                // #ST.96 does not include time portion in reg date
                Assert.That(tsdrdata.TSDRSingle["RegistrationDate"], Is.EqualTo("2004-03-23"));
            }
            else
            {
                Assert.That(tsdrdata.TSDRSingle["RegistrationDate"], Is.EqualTo("2004-03-23-05:00"));
            }
            Assert.That(tsdrdata.TSDRSingle["RegistrationDate"].Substring(0, 10),
                Is.EqualTo(tsdrdata.TSDRSingle["RegistrationDateTruncated"]));
            Assert.That(tsdrdata.TSDRSingle["MarkVerbalElementText"], Is.EqualTo("PYTHON"));
            List<Dictionary<string, string>> applicant_list = tsdrdata.TSDRMulti["ApplicantList"];
            Dictionary<string, string> applicant_info = applicant_list[0];
            Assert.That(applicant_info["ApplicantName"], Is.EqualTo("PYTHON SOFTWARE FOUNDATION"));
            if (t.PTOFormat != "ST66")  //non-zipped ST.66 format does not include assignments
            {
                List<Dictionary<string, string>> assignment_list = tsdrdata.TSDRMulti["AssignmentList"];
                Dictionary<string, string> assignment_0 = assignment_list[0]; ; // # Zeroth (most recent) assignment
                Assert.That(assignment_0["AssignorEntityName"], Is.EqualTo("CORPORATION FOR NATIONAL RESEARCH INITIATIVES, INC."));
                Assert.That(assignment_0["AssignmentDocumentURL"], Is.EqualTo("http://assignments.uspto.gov/assignments/assignment-tm-2849-0875.pdf"));
                Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoXSLTURL"], Is.EqualTo("https://github.com/codingatty/Plumage"));
                Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoXSLTLicense"], Is.EqualTo("Apache License, version 2.0 (January 2004)"));
                Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoXSLTSPDXLicenseIdentifier"], Is.EqualTo("Apache-2.0"));
                Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoXSLTLicenseURL"], Is.EqualTo("http://www.apache.org/licenses/LICENSE-2.0"));
                Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoImplementationURL"], Is.EqualTo("https://github.com/codingatty/Plumage-dotnet"));
                Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoImplementationVersion"], Does.Match(@"^\d+\.\d+\.\d+(-(\w+))*$"));
                // @"^\d+\.\d+\.\d+(-(\w+))*$"  :
                // matches release number in the form "1.2.3", with an optional dashed suffix like "-prelease"
                Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoImplementationLicenseURL"], Is.EqualTo("http://www.apache.org/licenses/LICENSE-2.0"));
                Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoImplementationLicense"], Is.EqualTo("Apache License, version 2.0 (January 2004)"));
                Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoImplementationName"], Is.EqualTo("Plumage-dotnet"));
                Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoImplementationSPDXLicenseIdentifier"], Is.EqualTo("Apache-2.0"));
            }
        }

        [Test]
        public void Test_O001_zipfile_by_serialno()
        // fetch by application ser. no. 76/044,902
        {
            TSDRReq t = new TSDRReq();
            t.getTSDRInfo("76044902", "s");
            validate_sample(t);
        }

        [Test]
        public void Test_O002_zipfile_by_regno()
        // fetch by reg. no. 2,824,281
        {
            TSDRReq t = new TSDRReq();
            t.getTSDRInfo("2824281", "r");
            validate_sample(t);
        }

        [Test]
        public void Test_O003_ST66xmlfile_by_serialno()
        // fetch by application ser. no. 76/044,902, ST66 format
        {
            TSDRReq t = new TSDRReq();
            t.setPTOFormat("ST66");
            t.getTSDRInfo("76044902", "s");
            validate_sample(t);
        }

        [Test]
        public void Test_O004_ST96xmlfile_by_serialno()
        // fetch by application ser. no. 76/044,902, ST96 format
        {
            TSDRReq t = new TSDRReq();
            t.setPTOFormat("ST96");
            t.getTSDRInfo("76044902", "s");
            validate_sample(t);
        }

        [Test]
        public void Test_O005_step_by_step()
        {
            TSDRReq t = new TSDRReq();
            Assert.That(t.XMLDataIsValid, Is.False);
            Assert.That(t.CSVDataIsValid, Is.False);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
            t.getXMLData("76044902", "s");
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

        [Test]
        public void Test_O099_no_such_app()
        // Test no-such-application returns no data, and a Fetch-404 error cod
        {
            TSDRReq t = new TSDRReq();
            t.getTSDRInfo("99999999", "s");
            Assert.That(t.XMLDataIsValid, Is.False);
            Assert.That(t.CSVDataIsValid, Is.False);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
            Assert.That(t.ErrorCode, Is.EqualTo("Fetch-404"));
        }
    }
}
