﻿using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace Plumage.Tests
{
    [TestFixture]
    [Category("Offline")]
    public class UnitTestOffline
    {
        private string TESTFILES_DIR = "testfiles";
        private string LINE_SEPARATOR = Environment.NewLine;
        private string TEST_CONFIG_FILENAME = "test-config.json";
        private static DateTime? INITIAL_PRIOR_TSDR_CALL_TIME = TSDRReq._prior_TSDR_call_time;

        [SetUp]
        public void Init()
        {
            TSDRReq._SetPriorTSDRCallTime(INITIAL_PRIOR_TSDR_CALL_TIME);
            TSDRReq.ResetIntervalTime();
        }


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
            t.getXMLData(Path.Combine(TESTFILES_DIR, "sn76044902.zip"));
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
            t.getTSDRInfo(Path.Combine(TESTFILES_DIR, "sn76044902.zip"));
            Assert.IsTrue(t.XMLDataIsValid);
            Assert.AreEqual(t.XMLData.Length, 30354);
            Assert.AreEqual(t.XMLData.Substring(0, 55), "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"); 
            string JFIF_tag = System.Text.Encoding.UTF8.GetString(t.ImageThumb, 6, 4);
            Assert.AreEqual(JFIF_tag, "JFIF");
            // PNG tag is "\x89PNG"; let's do this in two steps
            string PNG_tag = System.Text.Encoding.UTF8.GetString(t.ImageFull, 1, 3);
            Assert.AreEqual(t.ImageFull[0], 0X89);
            Assert.AreEqual(PNG_tag, "PNG");
            Assert.IsTrue(t.CSVDataIsValid);
            TSDRMap tsdrdata = t.TSDRData;
            Assert.IsTrue(tsdrdata.TSDRMapIsValid);
            Assert.AreEqual(tsdrdata.TSDRSingle["ApplicationNumber"], "76044902");
            Assert.AreEqual(tsdrdata.TSDRSingle["ApplicationDate"], "2000-05-09-04:00");
            Assert.AreEqual(tsdrdata.TSDRSingle["ApplicationDate"].Substring(0,10), 
                tsdrdata.TSDRSingle["ApplicationDateTruncated"]);
            Assert.AreEqual(tsdrdata.TSDRSingle["RegistrationNumber"], "2824281");
            Assert.AreEqual(tsdrdata.TSDRSingle["RegistrationDate"], "2004-03-23-05:00");
            Assert.AreEqual(tsdrdata.TSDRSingle["RegistrationDate"].Substring(0, 10),
                tsdrdata.TSDRSingle["RegistrationDateTruncated"]);
            Assert.AreEqual(tsdrdata.TSDRSingle["MarkVerbalElementText"], "PYTHON");
            Assert.AreEqual(tsdrdata.TSDRSingle["MarkCurrentStatusExternalDescriptionText"], 
                "A Sections 8 and 15 combined declaration has been accepted and acknowledged.");
            Assert.AreEqual(tsdrdata.TSDRSingle["MarkCurrentStatusDate"], "2010-09-08-04:00");
            Assert.AreEqual(tsdrdata.TSDRSingle["MarkCurrentStatusDate"].Substring(0, 10),
                tsdrdata.TSDRSingle["MarkCurrentStatusDateTruncated"]);
            List<Dictionary<string, string>> applicant_list = tsdrdata.TSDRMulti["ApplicantList"];
            Dictionary<string, string> applicant_info = applicant_list[0];
            Assert.AreEqual(applicant_info["ApplicantName"], "PYTHON SOFTWARE FOUNDATION");
            List<Dictionary<string, string>> assignment_list = tsdrdata.TSDRMulti["AssignmentList"];
            Dictionary<string, string> assignment_0 = assignment_list[0]; ; // # Zeroth (most recent) assignment
            Assert.AreEqual(assignment_0["AssignorEntityName"], "CORPORATION FOR NATIONAL RESEARCH INITIATIVES, INC.");
            Assert.AreEqual(assignment_0["AssignmentDocumentURL"], "http://assignments.uspto.gov/assignments/assignment-tm-2849-0875.pdf");
            
            // Diagnostic info
            Assert.AreEqual(tsdrdata.TSDRSingle["MetaInfoXSLTName"], "Plumage");
            Assert.That(tsdrdata.TSDRSingle["MetaInfoXSLTVersion"], Does.Match(@"^\d+\.\d+\.\d+(-(\w+))*$"));
            // @"^\d+\.\d+\.\d+(-(\w+))*$"  :
            // matches release number in the form "1.2.3", with an optional dashed suffix like "-prelease"
            Assert.AreEqual(tsdrdata.TSDRSingle["MetaInfoExecXSLTFormat"], "ST.66");
            Assert.AreEqual(tsdrdata.TSDRSingle["MetaInfoXSLTURL"], "https://github.com/codingatty/Plumage");
            Assert.AreEqual(tsdrdata.TSDRSingle["MetaInfoXSLTLicense"], "Apache License, version 2.0 (January 2004)");
            Assert.AreEqual(tsdrdata.TSDRSingle["MetaInfoXSLTSPDXLicenseIdentifier"], "Apache-2.0");
            Assert.AreEqual(tsdrdata.TSDRSingle["MetaInfoXSLTLicenseURL"], "http://www.apache.org/licenses/LICENSE-2.0");
            Assert.AreEqual(tsdrdata.TSDRSingle["MetaInfoLibraryName"], "Plumage-dotnet");
            Assert.That(tsdrdata.TSDRSingle["MetaInfoLibraryVersion"], Does.Match(@"^\d+\.\d+\.\d+(-(\w+))*$"));
            // @"^\d+\.\d+\.\d+(-(\w+))*$"  :
            // matches release number in the form "1.2.3", with an optional dashed suffix like "-prelease"
            Assert.AreEqual(tsdrdata.TSDRSingle["MetaInfoLibraryURL"], "https://github.com/codingatty/Plumage-dotnet");
            Assert.AreEqual(tsdrdata.TSDRSingle["MetaInfoLibraryLicense"], "Apache License, version 2.0 (January 2004)");
            Assert.AreEqual(tsdrdata.TSDRSingle["MetaInfoXSLTSPDXLicenseIdentifier"], "Apache-2.0");
            Assert.AreEqual(tsdrdata.TSDRSingle["MetaInfoLibraryLicenseURL"], "http://www.apache.org/licenses/LICENSE-2.0");

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
            Assert.AreEqual(metainfo["MetaInfoXSLTName"], "Plumage");
            Assert.AreEqual(metainfo["MetaInfoXSLTAuthor"], "Terry Carroll");
            Assert.AreEqual(metainfo["MetaInfoXSLTURL"], "https://github.com/codingatty/Plumage");
            Assert.AreEqual(metainfo["MetaInfoXSLTLicense"], "Apache License, version 2.0 (January 2004)");
            Assert.AreEqual(metainfo["MetaInfoXSLTSPDXLicenseIdentifier"], "Apache-2.0");
            Assert.AreEqual(metainfo["MetaInfoXSLTLicenseURL"], "http://www.apache.org/licenses/LICENSE-2.0");

            // Library metainfo (Plumage-dotnet)
            Assert.AreEqual(metainfo["MetaInfoLibraryName"], "Plumage-dotnet");
            Assert.AreEqual(metainfo["MetaInfoLibraryAuthor"], "Terry Carroll");
            Assert.AreEqual(metainfo["MetaInfoLibraryURL"], "https://github.com/codingatty/Plumage-dotnet");
            Assert.AreEqual(metainfo["MetaInfoLibraryLicense"], "Apache License, version 2.0 (January 2004)");
            Assert.AreEqual(metainfo["MetaInfoLibrarySPDXLicenseIdentifier"], "Apache-2.0");
            Assert.AreEqual(metainfo["MetaInfoLibraryLicenseURL"], "http://www.apache.org/licenses/LICENSE-2.0");
            // not much worth checking here; verify that it at least is non-zero-length
            Assert.That(metainfo["MetaInfoExecEnvironment"].Length, Is.GreaterThan(0));
        }

        [Test]
        //Test release-dependent metainfo data (changes, or may change, from release-to-release)
        public void Test_A005_check_releasedependent_metainfo()
        {
            Dictionary<string, string> metainfo = Plumage.TSDRReq.GetMetainfo();

            // XSLT fields (Plumage-XSL)
            Assert.AreEqual(metainfo["MetaInfoXSLTVersion"], "1.4.0");
            Assert.AreEqual(metainfo["MetaInfoXSLTDate"], "2021-02-02");
            Assert.AreEqual(metainfo["MetaInfoXSLTCopyright"], "Copyright 2014-2021 Terry Carroll");

            // Library (Plumage-dotnet)
            Assert.AreEqual(metainfo["MetaInfoLibraryVersion"], "1.4.0");
            Assert.AreEqual(metainfo["MetaInfoLibraryDate"], "2021-02-02");
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
            testfile = Path.Combine(TESTFILES_DIR, "sn76044902-ST66.xml");
            t66.getTSDRInfo(testfile);
            // reset already-called flag after each call to avoid delays in test
            TSDRReq._SetPriorTSDRCallTime(INITIAL_PRIOR_TSDR_CALL_TIME);

            TSDRReq t96 = new TSDRReq();
            testfile = Path.Combine(TESTFILES_DIR, "sn76044902-ST96.xml");
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
        // Read in the API key file, verify it looks good and has a valid expiration date; warn if within 30 days of expiration
        {
            DateTime exp_date;
            bool conversion_check;
            string config_file_path = Path.Combine(TESTFILES_DIR, TEST_CONFIG_FILENAME);
            string test_config_info_JSON = File.ReadAllText(config_file_path);
            Dictionary<string, string> config_info = JsonConvert.DeserializeObject<Dictionary<string, string>>(test_config_info_JSON);
            string comment = config_info["Comment"];
            string apikey = config_info["TSDRAPIKey"];
            string exp_date_string = config_info["TSDRAPIKeyExpirationDate"];
            Assert.AreEqual(apikey.Length, 32);
            Assert.IsTrue(apikey.All(Char.IsLetterOrDigit));
            Assert.AreEqual(exp_date_string.Length, 10);
            conversion_check = DateTime.TryParseExact(exp_date_string, "yyyy-MM-dd",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out exp_date);
            Assert.IsTrue(conversion_check);
            Assert.AreEqual(exp_date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), exp_date_string);
            DateTime today = DateTime.Today;
            Assert.That(exp_date, Is.GreaterThan(today)); // API key not yet expired
            int days_remaining = (exp_date - today).Days;
            Assert.That(days_remaining, Is.GreaterThan(0));
            if (days_remaining < 30)
            {
                Assert.Warn($"*** Only {days_remaining} days left on API key; expires {exp_date_string}. ***");
            }
        }

        [Test]
        // Test API key set/reset
        public void Test_A008_check_API_key_setting()
        // Test that API key gets set/reset (using dummy key)
        {
            TSDRReq t = new TSDRReq();
            Assert.IsNull(t.APIKey);
            string dummy_key = "ABCDEFGHIJK01234pqrstuvwxyz56789"; // 32 chars, upper&lower case, digits
            t.setAPIKey(dummy_key);
            Assert.AreEqual(t.APIKey, dummy_key);
            t.resetAPIKey();
            Assert.IsNull(t.APIKey);
        }
        // Group B
        // Test XML fetch only
        [Test]
        public void Test_B001_step_by_step_thru_xml()
        {
            TSDRReq t = new TSDRReq();
            Assert.IsFalse(t.XMLDataIsValid);
            Assert.IsFalse(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
            t.getXMLData(Path.Combine(TESTFILES_DIR, "sn76044902-ST66.xml"));
            Assert.IsTrue(t.XMLDataIsValid);
            Assert.IsFalse(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
        }

        [Test]
        public void Test_B002_step_by_step_thru_xml_zipped()
        {
            TSDRReq t = new TSDRReq();
            Assert.IsFalse(t.XMLDataIsValid);
            Assert.IsFalse(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
            t.getXMLData(Path.Combine(TESTFILES_DIR, "sn76044902.zip"));
            Assert.IsTrue(t.XMLDataIsValid);
            Assert.IsFalse(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
        }

        // Group C
        // Test through CSV creation
        [Test]
        public void Test_C001_step_by_step_thru_csv()
        {
            TSDRReq t = new TSDRReq();
            Assert.IsFalse(t.XMLDataIsValid);
            Assert.IsFalse(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
            t.getXMLData(Path.Combine(TESTFILES_DIR, "sn76044902.zip"));
            Assert.IsTrue(t.XMLDataIsValid);
            Assert.IsFalse(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
            t.getCSVData();
            Assert.IsTrue(t.XMLDataIsValid);
            Assert.IsTrue(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
        }

        // Group D
        // Test all the way through TSDR map
        [Test]
        public void Test_D001_step_by_step_thru_map()
        {
            TSDRReq t = new TSDRReq();
            Assert.IsFalse(t.XMLDataIsValid);
            Assert.IsFalse(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
            t.getXMLData(Path.Combine(TESTFILES_DIR, "sn76044902.zip"));
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
        }
        
        // Group E
        // Test parameter validations
        [Test]
        public void Test_E001_no_such_file()
        {
            TSDRReq t = new TSDRReq();
            Assert.Throws<System.IO.FileNotFoundException>(
              delegate { t.getTSDRInfo(Path.Combine(TESTFILES_DIR, "filedoesnotexist.zip")); });
            ;
        }

        [Test]
        public void Test_E002_getTSDRInfo_parameter_validation()
        {
            TSDRReq t = new TSDRReq();
            Assert.Throws<ArgumentException>(
              delegate { t.getTSDRInfo("123456789", "s"); }
              );    //      > 8-digit serial no. 

            // reset already-called flag after each call to avoid delays in test
            TSDRReq._SetPriorTSDRCallTime(INITIAL_PRIOR_TSDR_CALL_TIME);

            Assert.Throws<ArgumentException>(
              delegate { t.getTSDRInfo("1234567", "s"); }
              );    //      < 8-digit serial no.
            TSDRReq._SetPriorTSDRCallTime(INITIAL_PRIOR_TSDR_CALL_TIME);

            Assert.Throws<ArgumentException>(
                delegate { t.getTSDRInfo("1234567Z", "s"); }
                );    //    non-numeric serial no.
            TSDRReq._SetPriorTSDRCallTime(INITIAL_PRIOR_TSDR_CALL_TIME);

            Assert.Throws<ArgumentException>(
                delegate { t.getTSDRInfo("12345678", "r"); }
                );    //    > 7-digit reg. no
            TSDRReq._SetPriorTSDRCallTime(INITIAL_PRIOR_TSDR_CALL_TIME);

            Assert.Throws<ArgumentException>(
                delegate { t.getTSDRInfo("123456", "r"); }
                );    //    < 7-digit reg. no
            TSDRReq._SetPriorTSDRCallTime(INITIAL_PRIOR_TSDR_CALL_TIME);

            Assert.Throws<ArgumentException>(
                delegate { t.getTSDRInfo("123456Z", "r"); }
                );    //    non-numeric reg. no.
            TSDRReq._SetPriorTSDRCallTime(INITIAL_PRIOR_TSDR_CALL_TIME);

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
            t.getTSDRInfo(Path.Combine(TESTFILES_DIR, "rn2178784-ST-961_D3.xml")); // ST.96 1_D3 format XML file
            Assert.IsTrue(t.XMLDataIsValid);
            Assert.IsFalse(t.CSVDataIsValid);
            Assert.IsFalse(t.TSDRData.TSDRMapIsValid);
            Assert.AreEqual(t.ErrorCode, "CSV-UnsupportedXML");
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
            string ST961D3xslt = System.IO.File.ReadAllText(Path.Combine(TESTFILES_DIR, "ST96-V1.0.1.xsl"));
            t.setXSLT(ST961D3xslt);
            t.getTSDRInfo(Path.Combine(TESTFILES_DIR, "rn2178784-ST-961_D3.xml"));
            Assert.IsTrue(t.TSDRData.TSDRMapIsValid);
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
            string ST961D3xslt = System.IO.File.ReadAllText(Path.Combine(TESTFILES_DIR, "ST96-V1.0.1.xsl"));
            t_old.setXSLT(ST961D3xslt);
            t_old.getTSDRInfo(Path.Combine(TESTFILES_DIR, "rn2178784-ST-961_D3.xml"));
            var t_old_keys = from k in t_old.TSDRData.TSDRSingle.Keys where !k.StartsWith("Diag") select k;
            // reset already-called flag after each call to avoid delays in test
            TSDRReq._SetPriorTSDRCallTime(INITIAL_PRIOR_TSDR_CALL_TIME);

            // new
            TSDRReq t_new = new TSDRReq();
            t_new.getTSDRInfo(Path.Combine(TESTFILES_DIR, "rn2178784-ST-962.2.1.xml"));
            // List<string> t_old_keys = new List<string>(t_old.TSDRData.TSDRSingle.Keys);
            var t_new_keys = from k in t_old.TSDRData.TSDRSingle.Keys where !k.StartsWith("DiagnosticInfo") select k;

            // verify same keys in both
            Assert.AreEqual(t_new_keys, t_old_keys);

            // and same values, too
            foreach (var key in t_new_keys) {
                Assert.AreEqual(t_new.TSDRData.TSDRSingle[key], t_old.TSDRData.TSDRSingle[key]);
            }

            // Confirm the TSDRMultis match, too
            // (No "Diagnostic..." entries to filter out; 
            // but ignoring newer keys added by post-2016 enhancements)
            foreach (var key in t_old.TSDRData.TSDRMulti.Keys)
            {
                Assert.AreEqual(t_new.TSDRData.TSDRMulti[key], t_old.TSDRData.TSDRMulti[key]);
            }
        }

        [Test]
        public void Test_F004_process_with_alternate_XSL()
        /*
        Process using alternate XSL; this simple example pulls out
        nothing but the application no. and publication date
        */
        {
            string altXSL = System.IO.File.ReadAllText(Path.Combine(TESTFILES_DIR, "appno+pubdate.xsl"));
            TSDRReq t = new TSDRReq();
            t.setXSLT(altXSL);
            t.getTSDRInfo(Path.Combine(TESTFILES_DIR, "sn76044902.zip"));
            Assert.IsTrue(t.XMLDataIsValid);
            Assert.IsTrue(t.CSVDataIsValid);
            Assert.IsTrue(t.TSDRData.TSDRMapIsValid);
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
            t.getTSDRInfo(Path.Combine(TESTFILES_DIR, "sn76044902.zip"));
            Assert.IsTrue(t.XMLDataIsValid);
            Assert.IsTrue(t.CSVDataIsValid);
            Assert.IsTrue(t.TSDRData.TSDRMapIsValid);
        }

        // Group G
        // XSL/CSV validations

        private TSDRReq interior_test_with_XSLT_override(string xsl_text, Boolean success_expected)
        /*
        interior test used for each Group G test
        */
        {
            TSDRReq t = new TSDRReq();
            // reset already-called flag after each call to avoid delays in test
            TSDRReq._SetPriorTSDRCallTime(INITIAL_PRIOR_TSDR_CALL_TIME);
            t.setXSLT(xsl_text);
            t.getXMLData(Path.Combine(TESTFILES_DIR, "sn76044902.zip"));
            t.getCSVData();
            if (success_expected)
            {
                Assert.IsTrue(t.CSVDataIsValid);
                t.getTSDRData();
                Assert.IsTrue(t.TSDRData.TSDRMapIsValid);
            }
            else
            {
                Assert.IsFalse(t.CSVDataIsValid);
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
            string XSL_skeleton = System.IO.File.ReadAllText(Path.Combine(TESTFILES_DIR, "xsl_exception_test_skeleton.txt"));
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
            Assert.AreEqual(t.CSVData.Length, normal_CSV_length);

            // Blank lines at the beginning
            new_guts = XSL_text_tag + XSL_two_blanklines + XSL_appno + XSL_pubdate ;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: true);
            Assert.AreEqual(t.CSVData.Length, normal_CSV_length);

            // Blank lines in the middle
            new_guts = XSL_text_tag  + XSL_appno + XSL_two_blanklines + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: true);
            Assert.AreEqual(t.CSVData.Length, normal_CSV_length);
        }

        [Test]
        public void Test_G002_CSV_too_short()
        /*
        Sanity check requires at least two non-blank lines (at least two fields) in CSV
        */
        {
            string XSL_skeleton = System.IO.File.ReadAllText(Path.Combine(TESTFILES_DIR, "xsl_exception_test_skeleton.txt"));
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
            Assert.AreEqual(t.ErrorCode, "CSV-ShortCSV");

            // publication date only
            new_guts = XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.AreEqual(t.ErrorCode, "CSV-ShortCSV");

            // should also fail if there is more than two lines, but only one non-blank
            new_guts = XSL_appno + XSL_two_blanklines;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.AreEqual(t.ErrorCode, "CSV-ShortCSV");
        }
        [Test]
        public void Test_G003_CSV_malformed()
        /*
        Test common malforms of CSVs get caught
        */
        {
            string XSL_skeleton = System.IO.File.ReadAllText(Path.Combine(TESTFILES_DIR, "xsl_exception_test_skeleton.txt"));
            string XSLGUTS = "XSLGUTS";
            string XSL_appno = "ApplicationNumber,\"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            string XSL_pubdate = "PublicationDate,\"<xsl:value-of select=\"tm:PublicationDetails/tm:Publication/tm:PublicationDate\"/>\"<xsl:text/>";
            string XSL_appno_bad;
            // string XSL_two_blanklines = "   \n     \n";
            string altXSL, new_guts;
            TSDRReq t;

            // First, a good one
            new_guts = XSL_appno + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: true);


            // No good: missing comma (space instead)
            XSL_appno_bad = "ApplicationNumber \"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.AreEqual(t.ErrorCode, "CSV-InvalidKeyValuePair");

            // No good: missing quotes around application number
            XSL_appno_bad = "ApplicationNumber,<xsl:value-of select=\"tm:ApplicationNumber\"/><xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.AreEqual(t.ErrorCode, "CSV-InvalidValue");

            // No good: missing close-quote
            XSL_appno_bad = "ApplicationNumber,\"<xsl:value-of select=\"tm:ApplicationNumber\"/><xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.AreEqual(t.ErrorCode, "CSV-InvalidValue");

            // No good: missing open-quote
            XSL_appno_bad = "ApplicationNumber,<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.AreEqual(t.ErrorCode, "CSV-InvalidValue");

            // No good: space between key and field after comma
            XSL_appno_bad = "ApplicationNumber, \"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.AreEqual(t.ErrorCode, "CSV-InvalidValue");

            // No good: space in key name
            XSL_appno_bad = "Application Number,\"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.AreEqual(t.ErrorCode, "CSV-InvalidKey");

            // No good: disallowed character '-' in key name
            XSL_appno_bad = "Application-Number,\"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.AreEqual(t.ErrorCode, "CSV-InvalidKey");

            // No good: leading blank  in key name
            XSL_appno_bad = " ApplicationNumber,\"<xsl:value-of select=\"tm:ApplicationNumber\"/>\"<xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.AreEqual(t.ErrorCode, "CSV-InvalidKey");

            // No good: trailing blank in line
            XSL_appno_bad = "ApplicationNumber,\"<xsl:value-of select=\"tm:ApplicationNumber\"/>\" <xsl:text/>\n";
            new_guts = XSL_appno_bad + XSL_pubdate;
            altXSL = XSL_skeleton.Replace(XSLGUTS, new_guts);
            t = interior_test_with_XSLT_override(altXSL, success_expected: false);
            Assert.AreEqual(t.ErrorCode, "CSV-InvalidValue");
        }

        // Group H
        // test add'l fields as added

        [Test]
        public void Test_H001_verify_class_fields_exist()
        /*
        Make sure the three new dicts added to support trademark classifications:
          InternationalClassDescriptionList
          DomesticClassDescriptionList
          FirstUseDateList
        are present for both ST.66 and ST.96 formats.
        */
        {
            TSDRReq t66 = new TSDRReq();
            t66.getTSDRInfo(Path.Combine(TESTFILES_DIR, "sn76044902-ST66.xml"));
            // reset already-called flag after each call to avoid delays in test
            TSDRReq._SetPriorTSDRCallTime(INITIAL_PRIOR_TSDR_CALL_TIME);

            TSDRReq t96 = new TSDRReq();
            t96.getTSDRInfo(Path.Combine(TESTFILES_DIR, "sn76044902-ST96.xml"));
            CollectionAssert.Contains(t66.TSDRData.TSDRMulti.Keys, "InternationalClassDescriptionList");
            CollectionAssert.Contains(t66.TSDRData.TSDRMulti.Keys, "DomesticClassDescriptionList");
            CollectionAssert.Contains(t66.TSDRData.TSDRMulti.Keys, "FirstUseDateList");
            CollectionAssert.Contains(t96.TSDRData.TSDRMulti.Keys, "InternationalClassDescriptionList");
            CollectionAssert.Contains(t96.TSDRData.TSDRMulti.Keys, "DomesticClassDescriptionList");
            CollectionAssert.Contains(t96.TSDRData.TSDRMulti.Keys, "FirstUseDateList");
        }


        [Test]
        public void Test_H002_verify_intl_class_consistency()
        /*
        Make sure that all international classes are reported consistently and correctly
          InternationalClassDescriptionList / InternationalClassNumber (both formats)
          DomesticClassDescriptionList / PrimaryClassNumber (both formats)
          DomesticClassDescriptionList / NiceClassNumber (ST.96 only)
          FirstUseDateList / PrimaryClassNumber (both formats)

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
            t66.getTSDRInfo(Path.Combine(TESTFILES_DIR, "sn76044902-ST66.xml"));
            // reset already-called flag after each call to avoid delays in test
            TSDRReq._SetPriorTSDRCallTime(INITIAL_PRIOR_TSDR_CALL_TIME);
            tsdrmulti = t66.TSDRData.TSDRMulti;
            ICD_List = tsdrmulti["InternationalClassDescriptionList"];
            HashSet<string> ST66_IC_nos = (from s in ICD_List select s["InternationalClassNumber"]).ToHashSet();
            DCD_List = tsdrmulti["DomesticClassDescriptionList"];
            HashSet<string> ST66_DC_nos = (from s in DCD_List select s["PrimaryClassNumber"]).ToHashSet();
            FUD_List = tsdrmulti["FirstUseDateList"];
            HashSet<string> ST66_FUD_PrimaryClass_nos = (from s in FUD_List select s["PrimaryClassNumber"]).ToHashSet();

            // gather ST.96 class info
            TSDRReq t96 = new TSDRReq();
            t96.getTSDRInfo(Path.Combine(TESTFILES_DIR, "sn76044902-ST96.xml"));
            tsdrmulti = t96.TSDRData.TSDRMulti;
            ICD_List = tsdrmulti["InternationalClassDescriptionList"];
            HashSet<string> ST96_IC_nos = (from s in ICD_List select s["InternationalClassNumber"]).ToHashSet();
            DCD_List = tsdrmulti["DomesticClassDescriptionList"];
            HashSet<string> ST96_DC_nos = (from s in DCD_List select s["PrimaryClassNumber"]).ToHashSet();
            // following is ST.96 only:
            HashSet<string> ST96_DC_NiceClass_nos = (from s in DCD_List select s["NiceClassNumber"]).ToHashSet();
            FUD_List = tsdrmulti["FirstUseDateList"];
            HashSet<string> ST96_FUD_PrimaryClass_nos = (from s in FUD_List select s["PrimaryClassNumber"]).ToHashSet();

            // Confirm all of these match the control set
            Assert.AreEqual(ST66_IC_nos, control_set);
            Assert.AreEqual(ST66_DC_nos, control_set);
            Assert.AreEqual(ST66_FUD_PrimaryClass_nos, control_set); 

            Assert.AreEqual(ST96_IC_nos, control_set);
            Assert.AreEqual(ST96_DC_nos, control_set);
            Assert.AreEqual(ST96_DC_NiceClass_nos, control_set); // ST96 only; ST66 does not support NiceClassNumber
            Assert.AreEqual(ST96_FUD_PrimaryClass_nos, control_set);
        }

        // Group I
        // Timing tests

        private int execute_one_timed_call(double? fake_delay=null)
        /*
        Fake delay is the amount of time, in seconds, to delay before calling

        Returns the number of millisconds from prior call (or from initiation, if no prior call) 
        */
        {
            // on first call (null indicates not set in prior call) use current-time
            DateTime prior_call_time = TSDRReq._GetPriorTSDRCallTime() ?? DateTime.Now; 
            TSDRReq t = new TSDRReq();
            string testfile = Path.Combine(TESTFILES_DIR, "sn76044902.zip");
            if (fake_delay != null)
            {
                int sleeptime = (int)(fake_delay * 1000.0);
                Thread.Sleep(sleeptime);
            }
            t.getTSDRInfo(testfile);
            DateTime ending_time = DateTime.Now;
            Assert.IsTrue(t.TSDRData.TSDRMapIsValid);
            // int time_between_TSDR_calls_in_ms = (ending_time - prior_call_time);
            int time_between_TSDR_calls_in_ms = (int)((ending_time - prior_call_time).TotalMilliseconds);
            return (time_between_TSDR_calls_in_ms);
        }

      
        [Test, NonParallelizable]
        public void Test_I001_default_delay()
        /*
        Test to make sure data calls are delayed to keep from looking like a denial-of-service attack against PTO
        */
        {
            int total_time_in_ms;
            int TOLERANCE = 100;

            // First run should be almost instantaneous:
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.EqualTo(0).Within(TOLERANCE));

            // But second run should be delayed about a second (1000 ms)
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.GreaterThan(1000)); // should be more than one second
            Assert.That(total_time_in_ms, Is.LessThan(1200));    // But not a whole lot more
            Thread.Sleep(3000); // temp-force 3 second wait, confirm next test starts later
        }

        [Test, NonParallelizable]
        public void Test_I002_default_delay_with_faked_workload()
        /*
        This test ensures that there are no pointless delays, if processing itself is already taking time.
        For example, if we want at least a one-second between calls to TSDR, and processing the info itself 
        takes more than one second, there should be no added delay at all.
        */
        {
            int total_time_in_ms;
            int TOLERANCE = 100;

            // First run, should be almost instantaneous:

            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.EqualTo(0).Within(TOLERANCE));

            // Second run, pretend it takes 1.2 seconds of work between calls
            // Should take at least 1 sec; should be around 1.2 seconds total
            total_time_in_ms = execute_one_timed_call(fake_delay: 1.2);
            Assert.That(total_time_in_ms, Is.GreaterThan(1000));
            Assert.That(total_time_in_ms, Is.EqualTo(1200).Within(TOLERANCE));

            // Try again with three-second delay;
            // with three seconds spent between calls, calls should not be additionally delayed
            total_time_in_ms = execute_one_timed_call(fake_delay: 3);
            Assert.That(total_time_in_ms, Is.GreaterThan(1000));
            Assert.That(total_time_in_ms, Is.EqualTo(3000).Within(TOLERANCE));
        }

        [Test, NonParallelizable]
        public void Test_I003_override_delay()
        /*
        This test verifies the delay can be overriden (including eliminated) if we want
        */
        {
            int total_time_in_ms;
            int TOLERANCE = 100;

            // Override to zero delay
            // First run should be almost instantaneous, as usual:
            TSDRReq.SetIntervalTime(0);
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.EqualTo(0).Within(TOLERANCE));

            // Subsequent runs should also now be almost instantaneous, given the override 
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.EqualTo(0).Within(TOLERANCE));
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.EqualTo(0).Within(TOLERANCE));
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.EqualTo(0).Within(TOLERANCE));

            // Now override to a two-second delay, and confirm we see 2-second (2000-ms) delays
            TSDRReq.SetIntervalTime(2);
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.GreaterThan(2000));
            Assert.That(total_time_in_ms, Is.EqualTo(2000).Within(TOLERANCE));
            total_time_in_ms = execute_one_timed_call(); 
            Assert.That(total_time_in_ms, Is.GreaterThan(2000));
            Assert.That(total_time_in_ms, Is.EqualTo(2000).Within(TOLERANCE));
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.GreaterThan(2000));
            Assert.That(total_time_in_ms, Is.EqualTo(2000).Within(TOLERANCE));

            // Return to default, and we should see one-second delays again
            TSDRReq.ResetIntervalTime();
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.GreaterThan(1000));
            Assert.That(total_time_in_ms, Is.EqualTo(1000).Within(TOLERANCE));
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.GreaterThan(1000));
            Assert.That(total_time_in_ms, Is.EqualTo(1000).Within(TOLERANCE));
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.GreaterThan(1000));
            Assert.That(total_time_in_ms, Is.EqualTo(1000).Within(TOLERANCE));
        }

        [Test, NonParallelizable]
        public void Test_I004_fractional_delay()
        /*
        Verify a non-integer number of seconds works as expected
        */
        {
            int total_time_in_ms;
            int TOLERANCE = 100;

            // Override to 1.5-second delay
            TSDRReq.SetIntervalTime(1.5);

            // First run should be almost instantaneous, as usual:
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.EqualTo(0).Within(TOLERANCE));

            // Subsequent runs should also now be delayed about 1.5 seconds (1500 ms)
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.GreaterThan(1500));
            Assert.That(total_time_in_ms, Is.EqualTo(1500).Within(TOLERANCE));
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.GreaterThan(1500));
            Assert.That(total_time_in_ms, Is.EqualTo(1500).Within(TOLERANCE));
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.GreaterThan(1500));
            Assert.That(total_time_in_ms, Is.EqualTo(1500).Within(TOLERANCE));

            // even with a second or so of delay outside of the TSDRReq call
            total_time_in_ms = execute_one_timed_call(fake_delay: 0.8);
            Assert.That(total_time_in_ms, Is.GreaterThan(1500));
            Assert.That(total_time_in_ms, Is.EqualTo(1500).Within(TOLERANCE));
            total_time_in_ms = execute_one_timed_call(fake_delay: 1.2);
            Assert.That(total_time_in_ms, Is.GreaterThan(1500));
            Assert.That(total_time_in_ms, Is.EqualTo(1500).Within(TOLERANCE));
        }

        [Test, NonParallelizable]
        public void Test_I005_negative_delay()
        /*
        a negative delay will not let you time travel, but guaranteeing an interval of at 
        least a negative number of seconds is just like saying zero
        */
        {
            int total_time_in_ms;
            int TOLERANCE = 100;

            // set a negative ten-second delay
            TSDRReq.SetIntervalTime(-10);

            // First run should be almost instantaneous, as usual:
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.EqualTo(0).Within(TOLERANCE));

            // And so should subsequent runs;
            // waiting "at least -10 seconds" is the same as not waiting at all
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.EqualTo(0).Within(TOLERANCE));
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.EqualTo(0).Within(TOLERANCE));
            total_time_in_ms = execute_one_timed_call();
            Assert.That(total_time_in_ms, Is.EqualTo(0).Within(TOLERANCE));
        }

        [Test, NonParallelizable]
        public void Test_I006_nonnumeric_delay()
        /*
        Placeholder: not needed or possible in strongly-typed language; errors of this type are not compilable
        Test method retained for consistency with Plumage-py
        */
        {
            /*

            Examples of (compile-time) errors:

            TSDRReq.SetIntervalTime("1");  // Using a string instead of a number
            TSDRReq.SetIntervalTime(null); // using null
            TSDRReq.SetIntervalTime();     // not specifying anything
            */

        }


        // Group X
        // placeholder in which to develop tests
        [Test]
        public void Test_X001_placeholder()
        {
            TSDRReq t = new TSDRReq();
            t.getTSDRInfo(Path.Combine(TESTFILES_DIR, "sn76044902.zip"));
            TSDRMap tsdrdata = t.TSDRData;
            // Asserts go here
        }

    }
}
