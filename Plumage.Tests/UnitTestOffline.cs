using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Plumage.Tests
{
    [TestFixture]
    [Category("Offline")]
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
        // Group H: test add'l fields as added
        // Group I: test timing

        // Group O (in UnitTestOnline): Online tests that actually hit the PTO TSDR system

        // Group A
        // Basic exercises
        [Test]
        public void Test_A001_test_initialize()
        // Simple test, just verify TSDRReq can be initialized correctly
        {
            TSDRReq t = new TSDRReq();
            Assert.IsFalse(t.XMLDataIsValid);
            Assert.IsFalse(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
        }

        [Test]
        public void Test_A002_step_by_step_and_reset()
        {
            TSDRReq t = new TSDRReq();
            Assert.IsFalse(t.XMLDataIsValid);
            Assert.IsFalse(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
            t.getXMLData(TESTFILES_DIR+"sn76044902.zip");
            Assert.IsTrue(t.XMLDataIsValid);
            Assert.IsFalse(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
            t.getCSVData();
            Assert.IsTrue(t.XMLDataIsValid);
            Assert.IsTrue(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
            t.getTSDRData();
            Assert.IsTrue(t.XMLDataIsValid);
            Assert.IsTrue(t.CSVDataIsValid);
            Assert.IsTrue(t.TSDRData.TSDRMapIsValid);
            t.resetXMLData();
            Assert.IsFalse(t.XMLDataIsValid);
            Assert.IsFalse(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
        }

        [Test]
        public void Test_A003_typical_use()
        {
            TSDRReq t = new TSDRReq();
            t.getTSDRInfo(TESTFILES_DIR + "sn76044902.zip");
            Assert.IsTrue(t.XMLDataIsValid);
            Assert.That(t.XMLData.Length, Is.EqualTo(30354));
            Assert.That(t.XMLData.Substring(0, 55), 
                Is.EqualTo("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"));
            string JFIF_tag = System.Text.Encoding.UTF8.GetString(t.ImageThumb, 6, 4);
            Assert.That(JFIF_tag, Is.EqualTo("JFIF"));
            // PNG tag is "\x89PNG"; let's do this in two steps
            string PNG_tag = System.Text.Encoding.UTF8.GetString(t.ImageFull, 1, 3);
            Assert.That(t.ImageFull[0], Is.EqualTo(0X89));
            Assert.That(PNG_tag, Is.EqualTo("PNG"));
            // note: this fails:
            // string PNG_tag_fails = System.Text.Encoding.UTF8.GetString(t.ImageFull, 0, 4);
            // Assert.That(PNG_tag_fails, Is.EqualTo("\x89PNG"));
            Assert.IsTrue(t.CSVDataIsValid);
            TSDRMap tsdrdata = t.TSDRData;
            Assert.IsTrue(tsdrdata.TSDRMapIsValid);
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
            Assert.That(tsdrdata.TSDRSingle["MetaInfoXSLTName"], Is.EqualTo("Plumage"));
            Assert.That(tsdrdata.TSDRSingle["MetaInfoXSLTVersion"], Does.Match(@"^\d+\.\d+\.\d+(-(\w+))*$"));
            // @"^\d+\.\d+\.\d+(-(\w+))*$"  :
            // matches release number in the form "1.2.3", with an optional dashed suffix like "-prelease"
            Assert.That(tsdrdata.TSDRSingle["MetaInfoExecXSLTFormat"], Is.EqualTo("ST.66"));
            Assert.That(tsdrdata.TSDRSingle["MetaInfoXSLTURL"], Is.EqualTo("https://github.com/codingatty/Plumage"));
            Assert.That(tsdrdata.TSDRSingle["MetaInfoXSLTLicense"], Is.EqualTo("Apache License, version 2.0 (January 2004)"));
            Assert.That(tsdrdata.TSDRSingle["MetaInfoXSLTSPDXLicenseIdentifier"], Is.EqualTo("Apache-2.0"));
            Assert.That(tsdrdata.TSDRSingle["MetaInfoXSLTLicenseURL"], Is.EqualTo("http://www.apache.org/licenses/LICENSE-2.0"));
            Assert.That(tsdrdata.TSDRSingle["MetaInfoLibraryName"], Is.EqualTo("Plumage-dotnet"));
            Assert.That(tsdrdata.TSDRSingle["MetaInfoLibraryVersion"], Does.Match(@"^\d+\.\d+\.\d+(-(\w+))*$"));
            // @"^\d+\.\d+\.\d+(-(\w+))*$"  :
            // matches release number in the form "1.2.3", with an optional dashed suffix like "-prelease"
            Assert.That(tsdrdata.TSDRSingle["MetaInfoLibraryURL"], Is.EqualTo("https://github.com/codingatty/Plumage-dotnet"));
            Assert.That(tsdrdata.TSDRSingle["MetaInfoLibraryLicense"], Is.EqualTo("Apache License, version 2.0 (January 2004)"));
            Assert.That(tsdrdata.TSDRSingle["MetaInfoXSLTSPDXLicenseIdentifier"], Is.EqualTo("Apache-2.0"));
            Assert.That(tsdrdata.TSDRSingle["MetaInfoLibraryLicenseURL"], Is.EqualTo("http://www.apache.org/licenses/LICENSE-2.0"));

            // Execution-time fields
            string timestamp_as_text;
            System.DateTime timestamp_as_datetime;
            bool conversion_check;

            string simple_timestamp = tsdrdata.TSDRSingle["MetaInfoExecExecutionDateTime"];
            // verify no error parsing timestamp as valid date-time: 
            conversion_check = DateTime.TryParseExact(simple_timestamp, "yyyy-MM-dd HH:mm:ss", 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp_as_datetime);
            Assert.IsTrue(conversion_check);
            // verify looks the same after round-trip conversion
            timestamp_as_text = timestamp_as_datetime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            Assert.AreEqual(simple_timestamp, timestamp_as_text);
            
            //foreach (string key in tsdrdata.TSDRSingle.Keys)
            //{
            //    Console.WriteLine(key);
            //    Console.WriteLine(tsdrdata.TSDRSingle[key]);
            //}

            string start_datetime_text = tsdrdata.TSDRSingle["MetaInfoExecTSDRStartTimestamp"];
            string complete_datetime_text = tsdrdata.TSDRSingle["MetaInfoExecTSDRCompleteTimestamp"];

            var timestamps_to_test = new List<string> { start_datetime_text, complete_datetime_text };
            foreach (string timestamp in timestamps_to_test)
            {
                // verify no error parsing timestamp as valid date-time: 
                conversion_check = DateTime.TryParseExact(timestamp, "yyyy-MM-dd HH:mm:ss.ffffff",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp_as_datetime);
                Assert.IsTrue(conversion_check);
                // verify looks the same after round-trip conversion
                timestamp_as_text = timestamp_as_datetime.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture);
                Assert.AreEqual(timestamp, timestamp_as_text);
            }
        }

        [Test]
        public void Test_A004_check_releaseindependent_metainfo()
        {
            Dictionary<string, string> metainfo = Plumage.TSDRReq.GetMetainfo();

            // XSLT metainfo (Plumage)
            Assert.That(metainfo["MetaInfoXSLTName"], Is.EqualTo("Plumage"));
            Assert.That(metainfo["MetaInfoXSLTAuthor"], Is.EqualTo("Terry Carroll"));
            Assert.That(metainfo["MetaInfoXSLTURL"], Is.EqualTo("https://github.com/codingatty/Plumage"));
            Assert.That(metainfo["MetaInfoXSLTLicense"], Is.EqualTo("Apache License, version 2.0 (January 2004)"));
            Assert.That(metainfo["MetaInfoXSLTSPDXLicenseIdentifier"], Is.EqualTo("Apache-2.0"));
            Assert.That(metainfo["MetaInfoXSLTLicenseURL"], Is.EqualTo("http://www.apache.org/licenses/LICENSE-2.0"));

            // Library metainfo (Plumage-dotnet)
            Assert.That(metainfo["MetaInfoLibraryName"], Is.EqualTo("Plumage-dotnet"));
            Assert.That(metainfo["MetaInfoLibraryAuthor"], Is.EqualTo("Terry Carroll"));
            Assert.That(metainfo["MetaInfoLibraryURL"], Is.EqualTo("https://github.com/codingatty/Plumage-dotnet"));
            Assert.That(metainfo["MetaInfoLibraryLicense"], Is.EqualTo("Apache License, version 2.0 (January 2004)"));
            Assert.That(metainfo["MetaInfoLibrarySPDXLicenseIdentifier"], Is.EqualTo("Apache-2.0"));
            Assert.That(metainfo["MetaInfoLibraryLicenseURL"], Is.EqualTo("http://www.apache.org/licenses/LICENSE-2.0"));
            // not much worth checking here; verify that it at least is non-zero-length
            Assert.That(metainfo["MetaInfoLibraryLicenseURL"].Length, Is.GreaterThan(0));
        }

        [Test]
        //Test release-dependent metainfo data (changes, or may change, from release-to-release)
        public void Test_A005_check_releasedependent_metainfo()
        {
            Dictionary<string, string> metainfo = Plumage.TSDRReq.GetMetainfo();

            // XSLT fields (Plumage-XSL)
            Assert.AreEqual(metainfo["MetaInfoXSLTVersion"], "1.4.0-pre");
            Assert.AreEqual(metainfo["MetaInfoXSLTDate"], "2020-12-15");
            Assert.AreEqual(metainfo["MetaInfoXSLTCopyright"], "Copyright 2014-2020 Terry Carroll");

            // Library (Plumage-dotnet)
            Assert.AreEqual(metainfo["MetaInfoLibraryVersion"], "1.4.0-pre");
            Assert.AreEqual(metainfo["MetaInfoLibraryDate"], "2021-01-25");
            Assert.AreEqual(metainfo["MetaInfoLibraryCopyright"], "Copyright 2014-2021 Terry Carroll");
        }

        [Test]
        // Test metainfo consistency
        public void Test_A006_check_metainfo_consistency()
        {

            // 1. All keys from GetMetainfo() are also in run-time TSDR data (both ST.66 and ST.96)
            // 2. All values from GetMetainfo() match those from run-time TSDR data (both ST.66 and ST.96)

            string testfile;

            Dictionary<string, string> metainfo = Plumage.TSDRReq.GetMetainfo();

            TSDRReq t66 = new TSDRReq();
            testfile = TESTFILES_DIR + "sn76044902-ST66.xml";
            t66.getTSDRInfo(testfile);

            TSDRReq t96 = new TSDRReq();
            testfile = TESTFILES_DIR + "sn76044902-ST96.xml";
            t96.getTSDRInfo(testfile);

            CollectionAssert.IsSubsetOf(metainfo.Keys, t66.TSDRData.TSDRSingle.Keys);
            CollectionAssert.IsSubsetOf(metainfo.Keys, t96.TSDRData.TSDRSingle.Keys);

            // Consistent values for ST.96
            foreach (string K in metainfo.Keys)
            {
                Assert.AreEqual(metainfo[K], t66.TSDRData.TSDRSingle[K]);
            }

            // Consistent values for ST.96
            foreach (string K in metainfo.Keys)
            {
                Assert.AreEqual(metainfo[K], t96.TSDRData.TSDRSingle[K]);
            }


        }

        [Test]
        //Test API key format
        public void Test_A007_check_API_key()
        {

        }

        [Test]
        // Test API key set/reset
        public void Test_A008_check_API_key_setting()
        {

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
            t.getXMLData(TESTFILES_DIR + "sn76044902-ST66.xml");
            Assert.That(t.XMLDataIsValid, Is.True);
            Assert.That(t.CSVDataIsValid, Is.False);
            Assert.That(t.TSDRData.TSDRMapIsValid, Is.False);
        }

        [Test]
        public void Test_B002_step_by_step_thru_xml_zipped()
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
            // (No "Diagnostic..." entries to filter out; 
            // but ignoring newer keys added by post-2016 enhancements)
            foreach (var key in t_old.TSDRData.TSDRMulti.Keys)
            {
                Assert.That(t_new.TSDRData.TSDRMulti[key], Is.EqualTo(t_old.TSDRData.TSDRMulti[key]));
            }
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
            t = interior_test_with_XSLT_override(altXSL, success_expected: true);
            int normal_CSV_length = t.CSVData.Length;

            // Now the variations, injecting blank lines. They should be ignored and get the same result

            // Blank lines at the end
            new_guts = XSL_text_tag + XSL_appno + XSL_pubdate + XSL_two_blanklines;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: true);
            Assert.That(t.CSVData.Length, Is.EqualTo(normal_CSV_length));

            // Blank lines at the beginning
            new_guts = XSL_text_tag + XSL_two_blanklines + XSL_appno + XSL_pubdate ;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: true);
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
        [Test]
        public void Test_G003_CSV_malformed()
        /*
        Test common malforms of CSVs get caught
        */
        {
            string XSL_skeleton = System.IO.File.ReadAllText(TESTFILES_DIR + "xsl_exception_test_skeleton.txt");
            string XSLGUTS = "XSLGUTS";
            string XSL_appno = "ApplicationNumber,\"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            string XSL_pubdate = "PublicationDate,\"<xsl:value-of select=\"tm:PublicationDetails/tm:Publication/tm:PublicationDate\"/>\"<xsl:text/>";
            string XSL_appno_bad;
            // string XSL_two_blanklines = "   \n     \n";
            string altXSL, new_guts;
            TSDRReq t;

            // First, a good one
            XSL_appno = "ApplicationNumber,\"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            new_guts = XSL_appno + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: true);

            // No good: missing comma (space instead)
            XSL_appno_bad = "ApplicationNumber \"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.That(t.ErrorCode, Is.EqualTo("CSV-InvalidKeyValuePair"));

            // No good: missing quotes around application number
            XSL_appno_bad = "ApplicationNumber,<xsl:value-of select=\"tm:ApplicationNumber\"/><xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.That(t.ErrorCode, Is.EqualTo("CSV-InvalidValue"));

            // No good: missing close-quote
            XSL_appno_bad = "ApplicationNumber,\"<xsl:value-of select=\"tm:ApplicationNumber\"/><xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.That(t.ErrorCode, Is.EqualTo("CSV-InvalidValue"));

            // No good: missing open-quote
            XSL_appno_bad = "ApplicationNumber,<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.That(t.ErrorCode, Is.EqualTo("CSV-InvalidValue"));

            // No good: space between key and field after comma
            XSL_appno_bad = "ApplicationNumber, \"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.That(t.ErrorCode, Is.EqualTo("CSV-InvalidValue"));

            // No good: space in key name
            XSL_appno_bad = "Application Number,\"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.That(t.ErrorCode, Is.EqualTo("CSV-InvalidKey"));

            // No good: disallowed character '-' in key name
            XSL_appno_bad = "Application-Number,\"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.That(t.ErrorCode, Is.EqualTo("CSV-InvalidKey"));

            // No good: leading blank  in key name
            XSL_appno_bad = " ApplicationNumber,\"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.That(t.ErrorCode, Is.EqualTo("CSV-InvalidKey"));

            // No good: trailing blank in line
            XSL_appno_bad = "ApplicationNumber,\"<xsl:value-of select=\"tm:ApplicationNumber\"/>\" <xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.That(t.ErrorCode, Is.EqualTo("CSV-InvalidValue"));
        }

        // Group H
        // test add'l fields as added

        [Test]
        public void Test_H001_verify_class_fields_exist()
        /*
        Make sure the three new dicts added to support trademark classifications:
          InternationalClassDescriptionList
          DomesticClassDescriptionList
          FirstUseDatesList
        are present for both ST.66 and ST.96 formats.
        */
        {
            TSDRReq t66 = new TSDRReq();
            t66.getTSDRInfo(TESTFILES_DIR + "sn76044902-ST66.xml");
            TSDRReq t96 = new TSDRReq();
            t96.getTSDRInfo(TESTFILES_DIR + "sn76044902-ST96.xml");
            Assert.That(t66.TSDRData.TSDRMulti.ContainsKey("InternationalClassDescriptionList"), Is.True);
            Assert.That(t66.TSDRData.TSDRMulti.ContainsKey("DomesticClassDescriptionList"), Is.True);
            Assert.That(t66.TSDRData.TSDRMulti.ContainsKey("FirstUseDatesList"), Is.True);
            Assert.That(t96.TSDRData.TSDRMulti.ContainsKey("InternationalClassDescriptionList"), Is.True);
            Assert.That(t96.TSDRData.TSDRMulti.ContainsKey("DomesticClassDescriptionList"), Is.True);
            Assert.That(t96.TSDRData.TSDRMulti.ContainsKey("FirstUseDatesList"), Is.True);
        }


        [Test]
        public void Test_H002_verify_intl_class_consistency()
        /*
        Make sure that all international classes are reported consistently and correctly
          InternationalClassDescriptionList / InternationalClassNumber (both formats)
          DomesticClassDescriptionList / PrimaryClassNumber (both formats)
          DomesticClassDescriptionList / NiceClassNumber (ST.96 only)
          FirstUseDatesList / PrimaryClassNumber (both formats)

        For the test cases, each of these should have the same set of class IDs: ["009", "042"], although maybe more than once
        */
        {
            Dictionary<string,List<Dictionary<string, string>>> tsdrmulti;
            List<Dictionary<string, string>> ICD_List;   // int'l class descriptions
            List<Dictionary<string, string>> DCD_List;   // domestic class descriptions
            List<Dictionary<string, string>> FUD_List;   // first-use dates

            HashSet<string> control_set = new HashSet<string> { "009", "042" };

            // gather ST.66 class info
            TSDRReq t66 = new TSDRReq();
            t66.getTSDRInfo(TESTFILES_DIR + "sn76044902-ST66.xml");
            tsdrmulti = t66.TSDRData.TSDRMulti;
            ICD_List = tsdrmulti["InternationalClassDescriptionList"];
            HashSet<string> ST66_IC_nos = (from s in ICD_List select s["InternationalClassNumber"]).ToHashSet();
            DCD_List = tsdrmulti["DomesticClassDescriptionList"];
            HashSet<string> ST66_DC_nos = (from s in DCD_List select s["PrimaryClassNumber"]).ToHashSet();
            FUD_List = tsdrmulti["FirstUseDatesList"];
            HashSet<string> ST66_FUD_PrimaryClass_nos = (from s in FUD_List select s["PrimaryClassNumber"]).ToHashSet();


            // gather ST.96 class info
            TSDRReq t96 = new TSDRReq();
            t96.getTSDRInfo(TESTFILES_DIR + "sn76044902-ST96.xml");
            tsdrmulti = t96.TSDRData.TSDRMulti;
            ICD_List = tsdrmulti["InternationalClassDescriptionList"];
            HashSet<string> ST96_IC_nos = (from s in ICD_List select s["InternationalClassNumber"]).ToHashSet();
            DCD_List = tsdrmulti["DomesticClassDescriptionList"];
            HashSet<string> ST96_DC_nos = (from s in DCD_List select s["PrimaryClassNumber"]).ToHashSet();
            // following is ST.96 only:
            HashSet<string> ST96_DC_NiceClass_nos = (from s in DCD_List select s["NiceClassNumber"]).ToHashSet();
            FUD_List = tsdrmulti["FirstUseDatesList"];
            HashSet<string> ST96_FUD_PrimaryClass_nos = (from s in FUD_List select s["PrimaryClassNumber"]).ToHashSet();
           
            // Confirm all of these match the control set
            Assert.That(ST66_IC_nos.SetEquals(control_set), Is.True);
            Assert.That(ST66_DC_nos.SetEquals(control_set), Is.True);
            Assert.That(ST66_FUD_PrimaryClass_nos.SetEquals(control_set), Is.True);
            Assert.That(ST96_IC_nos.SetEquals(control_set), Is.True);
            Assert.That(ST96_DC_nos.SetEquals(control_set), Is.True);
            Assert.That(ST96_DC_NiceClass_nos.SetEquals(control_set), Is.True);   // ST66_DC_nos.96 only
            Assert.That(ST96_FUD_PrimaryClass_nos.SetEquals(control_set), Is.True);
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
