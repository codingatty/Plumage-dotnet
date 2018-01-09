using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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
            // Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoImplementationVersion"], Is.EqualTo("1.2.0"));
            Assert.That(tsdrdata.TSDRSingle["DiagnosticInfoImplementationVersion"], Does.Match(@"^\d+\.\d+\.\d+(-(\w+))*$"));
            // @"^\d+\.\d+\.\d+(-(\w+))*$"  :
            // matches release number in the form "1.2.3", with an optional dashed suffix like "-prelease"
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
        
        // Group E
        // Test parameter validations
        [Test]
        public void Test_E001_no_such_file()
        {
            TSDRReq t = new TSDRReq();
            Assert.Throws<System.IO.FileNotFoundException>(
              delegate { t.getTSDRInfo(TESTFILES_DIR + "filedoesnotexist.zip"); });
            ;
        }

        [Test]
        public void Test_E002_getTSDRInfo_parameter_validation()
        {
            TSDRReq t = new TSDRReq();
            Assert.Throws<ArgumentException>(
              delegate { t.getTSDRInfo("123456789", "s"); }
              );    //      > 8-digit serial no.
            Assert.Throws<ArgumentException>(
              delegate { t.getTSDRInfo("1234567", "s"); }
              );    //      < 8-digit serial no.
            Assert.Throws<ArgumentException>(
                delegate { t.getTSDRInfo("1234567Z", "s"); }
                );    //    non-numeric serial no.
            Assert.Throws<ArgumentException>(
                delegate { t.getTSDRInfo("12345678", "r"); }
                );    //    > 7-digit reg. no
            Assert.Throws<ArgumentException>(
                delegate { t.getTSDRInfo("123456", "r"); }
                );    //    < 7-digit reg. no
            Assert.Throws<ArgumentException>(
                delegate { t.getTSDRInfo("123456Z", "r"); }
                );    //    non-numeric reg. no.
            Assert.Throws<ArgumentException>
                (delegate { t.getTSDRInfo("123456", "X"); }
                );    //    incorrect type (not "s"/"r")
        }

        // Group F
        // XML/XSL variations
        [Test]
        public void Test_F001_flag_ST961D3()
        //  Plumage recognizes ST.96 1_D3 format XML, but no longer supports it
        {
            TSDRReq t = new TSDRReq();
            t.getTSDRInfo(TESTFILES_DIR + "rn2178784-ST-961_D3.xml"); // ST.96 1_D3 format XML file
            Assert.That(t.XMLDataIsValid, Is.True);
            Assert.That(t.CSVDataIsValid, Is.False);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
            Assert.That(t.ErrorCode, Is.EqualTo("CSV-UnsupportedXML"));
        }

        [Test]
        public void Test_F002_process_ST961D3()
            /*
            test using an alternate XSL file.

            In this case, rn2178784-ST-961_D3.xml is a file formatted under 
            the old ST.96 format, no longer used by the PTO; ST96-V1.0.1.xsl 
            is an XSLT file that used to support that format
            */
        {
            TSDRReq t = new TSDRReq();
            string ST961D3xslt = System.IO.File.ReadAllText(TESTFILES_DIR + "ST96-V1.0.1.xsl");
            t.setXSLT(ST961D3xslt);
            t.getTSDRInfo(TESTFILES_DIR + "rn2178784-ST-961_D3.xml");
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.True);
        }

        [Test]
        public void Test_F003_confirm_ST96_support_201605()
        {
            /*
            In May 2016, the USPTO switched from ST96 V1_D3 to ST96 2.2.1.
            This test is to ensure that Plumage provides identical result
            under both the the old and new formats.  
            */

            // old
            TSDRReq t_old = new TSDRReq();
            string ST961D3xslt = System.IO.File.ReadAllText(TESTFILES_DIR + "ST96-V1.0.1.xsl");
            t_old.setXSLT(ST961D3xslt);
            t_old.getTSDRInfo(TESTFILES_DIR + "rn2178784-ST-961_D3.xml");
            var t_old_keys = from k in t_old.TSDRData.TSDRSingle.Keys where !k.StartsWith("Diag") select k;
            
            // new
            TSDRReq t_new = new TSDRReq();
            t_new.getTSDRInfo(TESTFILES_DIR + "rn2178784-ST-962.2.1.xml");
            // List<string> t_old_keys = new List<string>(t_old.TSDRData.TSDRSingle.Keys);
            var t_new_keys = from k in t_old.TSDRData.TSDRSingle.Keys where !k.StartsWith("DiagnosticInfo") select k;

            // verify same keys in both
            Assert.That(t_new_keys, Is.EqualTo(t_old_keys));

            // and same values, too
            foreach (var key in t_new_keys) {
                Assert.That(t_new.TSDRData.TSDRSingle[key], Is.EqualTo(t_old.TSDRData.TSDRSingle[key]));
            }

            // Confirm the TSDRMultis match, too
            // (No "Diagnostic..." entries to filter out)
            Assert.That(t_new.TSDRData.TSDRMulti, Is.EqualTo(t_old.TSDRData.TSDRMulti));

        }

        [Test]
        public void Test_F004_process_with_alternate_XSL()
        /*
        Process using alternate XSL; this simple example pulls out
        nothing but the application no. and publication date
        */
        {
            string altXSL = System.IO.File.ReadAllText(TESTFILES_DIR + "appno+pubdate.xsl");
            TSDRReq t = new TSDRReq();
            t.setXSLT(altXSL);
            t.getTSDRInfo(TESTFILES_DIR + "sn76044902.zip");
            Assert.That(t.XMLDataIsValid, Is.True);
            Assert.That(t.CSVDataIsValid, Is.True);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.True);
        }

        [Test]
        public void Test_F005_process_with_alternate_XSL_inline()
        /*
        Process alternate XSL, placed inline
        Pull out nothing but application no. and publication date.
        Other than placing the XSL inline, this is identical to 
        Test_F004_process_with_alternate_XSL

        Note: using inline is not recommended; XSL processor is picky and inline
        XSL is hard to debug. The recommended approach is to use an external file
        and develop/debug the XSL separately, using an external XSL processor such
        as MSXSL or equivalent.
        */
        {

            string altXSL = @"<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" xmlns:tm=""http://www.wipo.int/standards/XMLSchema/trademarks"" xmlns:pto=""urn:us:gov:doc:uspto:trademark:status"">
<xsl:output method=""text"" encoding=""utf-8""/>
<xsl:template match=""tm:Transaction"">
<xsl:apply-templates select="".//tm:TradeMark""/>
</xsl:template>
<xsl:template match=""tm:TradeMark"">
<xsl:text/>ApplicationNumber,""<xsl:value-of select=""tm:ApplicationNumber""/>""<xsl:text/>
PublicationDate,""<xsl:value-of select=""tm:PublicationDetails/tm:Publication/tm:PublicationDate""/>""<xsl:text/>
</xsl:template>
</xsl:stylesheet>";
            TSDRReq t = new TSDRReq();
            t.setXSLT(altXSL);
            t.getTSDRInfo(TESTFILES_DIR + "sn76044902.zip");
            Assert.That(t.XMLDataIsValid, Is.True);
            Assert.That(t.CSVDataIsValid, Is.True);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.True);
        }

        // Group G
        // XSL/CSV validations

        private TSDRReq interior_test_with_XSLT_override(string xsl_text, Boolean success_expected)
        /*
        interior test used for each Group G test
        */
        {
            TSDRReq t = new TSDRReq();
            t.setXSLT(xsl_text);
            t.getXMLData(TESTFILES_DIR + "sn76044902.zip");
            t.getCSVData();
            if (success_expected)
            {
                Assert.That(t.CSVDataIsValid, Is.True);
                t.getTSDRData();
                Assert.That(t.TSDRData.TSDRMapIsValid, Is.True);
            }
            else
            {
                Assert.That(t.CSVDataIsValid, Is.False);
            }
            return (t);
        }

        [Test]
        public void Test_G001_XSLT_with_blanks()
        /*
        Process alternate XSL, slightly malformed to generate empty lines;
        make sure they're ignored (new feature in Plumage 1.2)
        */
        {
            string XSL_skeleton = System.IO.File.ReadAllText(TESTFILES_DIR + "xsl_exception_test_skeleton.txt");
            string XSLGUTS = "XSLGUTS";
            string XSL_text_tag = "<xsl:text/>";
            string XSL_appno = "ApplicationNumber,\"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            string XSL_pubdate = "PublicationDate,\"<xsl:value-of select=\"tm:PublicationDetails/tm:Publication/tm:PublicationDate\"/>\"<xsl:text/>";
            string XSL_two_blanklines = "   \n     \n";
            // string XSL_one_blankline = "   \n";
            string altXSL, new_guts;
            TSDRReq t;

            // First, try a vanilla working version
            new_guts = XSL_text_tag + XSL_appno + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            //Console.WriteLine(XSL_skeleton);
            //Console.WriteLine(altXSL);
            t = interior_test_with_XSLT_override(altXSL, success_expected: true);
            int normal_CSV_length = t.CSVData.Length;

            // Now the variations, injecting blank lines. They should be ignored and get the same result

            // Blank lines at the end
            new_guts = XSL_text_tag + XSL_appno + XSL_pubdate + XSL_two_blanklines;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            // Console.WriteLine(altXSL);
            t = interior_test_with_XSLT_override(altXSL, success_expected: true);
            Assert.That(t.CSVData.Length, Is.EqualTo(normal_CSV_length));

            // Blank lines at the beginning
            new_guts = XSL_text_tag + XSL_two_blanklines + XSL_appno + XSL_pubdate ;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            //Console.WriteLine(altXSL);
            t = interior_test_with_XSLT_override(altXSL, success_expected: true);
            //Console.WriteLine(t.ErrorCode, t.ErrorMessage);
            Assert.That(t.CSVData.Length, Is.EqualTo(normal_CSV_length));

            // Blank lines in the middle
            new_guts = XSL_text_tag  + XSL_appno + XSL_two_blanklines + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: true);
            Assert.That(t.CSVData.Length, Is.EqualTo(normal_CSV_length));
        }

        [Test]
        public void Test_G002_CSV_too_short()
        /*
        Sanity check requires at least two non-blank lines (at least two fields) in CSV
        */
        {
            string XSL_skeleton = System.IO.File.ReadAllText(TESTFILES_DIR + "xsl_exception_test_skeleton.txt");
            string XSLGUTS = "XSLGUTS";
            string XSL_appno = "ApplicationNumber,\"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            string XSL_pubdate = "PublicationDate,\"<xsl:value-of select=\"tm:PublicationDetails/tm:Publication/tm:PublicationDate\"/>\"<xsl:text/>";
            string XSL_two_blanklines = "   \n     \n";
            string altXSL, new_guts;
            TSDRReq t;

            // First, try a vanilla working version: 2 fields, appno and publication date
            new_guts = XSL_appno + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: true);

            // Now, application no. only, publication date only; each should fail

            // application no. only
            new_guts = XSL_appno;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.That(t.ErrorCode, Is.EqualTo("CSV-ShortCSV"));

            // publication date only
            new_guts = XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.That(t.ErrorCode, Is.EqualTo("CSV-ShortCSV"));

            // should also fail if there is more than two lines, but only one non-blank
            new_guts = XSL_appno + XSL_two_blanklines;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.That(t.ErrorCode, Is.EqualTo("CSV-ShortCSV"));
        }


        // Group X
        // placeholder in which to develop tests
        [Test]
        public void Test_X001_placeholder()
        {
            TSDRReq t = new TSDRReq();
            t.getTSDRInfo(TESTFILES_DIR + "sn76044902.zip");
            TSDRMap tsdrdata = t.TSDRData;
            // Asserts go here
        }

    }
}
