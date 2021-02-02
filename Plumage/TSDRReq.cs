using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Xml;
using System.Xml.Xsl;

/*

Plumage: C# (.NET) module to obtain trademark status information from USPTO's TSDR system

Copyright 2014-2021 Terry Carroll
carroll@tjc.com

License information:

This program is licensed under Apache License, version 2.0 (January 2004);
see http://www.apache.org/licenses/LICENSE-2.0
SPX-License-Identifier: Apache-2.0

Anyone who makes use of, or who modifies, this code is encouraged
(but not required) to notify the author.

 */

namespace Plumage
{
    public class TSDRReq
    {
        // public static string __version__ = "1.4.0-pre";
        //public static string __last_updated__ = "2021-01-25";

        public static string __environment_version__ = Environment.Version.ToString();
        private static Dictionary<string, string> TSDRSubstitutions;
        private static Dictionary<string, XSLTDescriptor> xslt_table;
        // options fields
        public string APIKey;
        public string XSLT;
        public string PTOFormat;
        // diagnostic fields
        public string ErrorCode, ErrorMessage;
        // substantive data fields
        //  from XML fetch step:
        public string XMLData, CSVData;
        public TSDRMap TSDRData;
        public byte[] ZipData, ImageFull, ImageThumb;
        //  data validity flags
        public Boolean XMLDataIsValid, CSVDataIsValid;

        private string COMMA = ",";
        private string LINE_SEPARATOR = Environment.NewLine;

        // public static Dictionary<string, string> MetaInfo;
        public static Dictionary<string, string>  MetaInfo = new Dictionary<string, string>();

        public static DateTime? _prior_TSDR_call_time = null;     // time of previous TSDR call (real or simulated), if any
        private static double _default_TSDR_minimum_interval = 1.0; // at least one second between calls to TSDR (real or simulated)

        private static double _TSDR_minimum_interval = _default_TSDR_minimum_interval;

        static TSDRReq()
        {
            TSDRSubstitutions = new Dictionary<string, string> {
                {"$XSLTFILENAME$","Not Set"},               // XSLT stylesheet file name
                {"$XSLTLOCATION$","Not Set"},               // XSLT stylesheet location
                {"$IMPLEMENTATIONNAME$", ImplementationInfo.libraryName},         // TSDR implementation identifier
                {"$IMPLEMENTATIONVERSION$", ImplementationInfo.libraryVersion},   // TSDR implementation version no.
                {"$IMPLEMENTATIONDATE$", ImplementationInfo.libraryDate},   // Implementation last-updated date
                {"$IMPLEMENTATIONAUTHOR$", ImplementationInfo.libraryAuthor},      // Implementation author
                {"$IMPLEMENTATIONURL$", ImplementationInfo.libraryURL},            // implementation URL
                {"$IMPLEMENTATIONCOPYRIGHT$", ImplementationInfo.libraryCopyright},    // implementation copyright notice
                {"$IMPLEMENTATIONLICENSE$", ImplementationInfo.libraryLicense},    // implementation license
                {"$IMPLEMENTATIONSPDXLID$", ImplementationInfo.librarySPDXLicenseIdentifier},   // implementation license SPDX ID
                {"$IMPLEMENTATIONLICENSEURL$", ImplementationInfo.libraryLicenseURL},  // Implementation license URL
                {"$EXECUTIONDATETIME$","Not Set"},          // Execution timestamp, YYYY-MM-DD HH:MM:SS format (set at runtime)
                {"$TSDRSTARTDATETIME$","Not Set"},            // TSDR call start timestamp, ISO-8601 format to nearest microsec (set at runtime)
                {"$TSDRCOMPLETEDATETIME$","Not Set"},         // TSDR call start timestamp, ISO-8601 format to nearest microsec (set at runtime)
                {"$XMLSOURCE$","Not Set"},                   // URL or pathname of XML source
                {"$MetaInfoExecEnvironment$", ImplementationInfo.ExecEnvironment}  // Environment (.NET) version info
            };
            XSLTDescriptor ST66Table = new XSLTDescriptor("ST66");
            XSLTDescriptor ST96Table = new XSLTDescriptor("ST96");
            xslt_table = new Dictionary<string, XSLTDescriptor>();
            xslt_table["ST66"] = ST66Table;
            xslt_table["ST96"] = ST96Table;

            // Set up Meta info

            // Get metadata via Plumage-XSLT-metadata.json resource
            // This is the metainfo inherited from Plumage-XSL
            Assembly asm = Assembly.GetExecutingAssembly();
            string asmname = asm.GetName().Name;
            string json_filename = "Plumage-XSLT-metadata.json";
            string json_resource_path = asmname + "." + json_filename;
            string json_text = "Not set";
            using (Stream stream = asm.GetManifestResourceStream(json_resource_path))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    json_text = sr.ReadToEnd();
                }
            }
            var temp_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json_text);
            foreach (KeyValuePair<string, string> item in temp_dict)
            {
                MetaInfo.Add(item.Key, item.Value);
            }

            // Add Plumage-dotnet-specific info
            MetaInfo.Add("MetaInfoLibraryName", ImplementationInfo.libraryName);
            MetaInfo.Add("MetaInfoLibraryVersion", ImplementationInfo.libraryVersion);
            MetaInfo.Add("MetaInfoLibraryDate", ImplementationInfo.libraryDate);
            MetaInfo.Add("MetaInfoLibraryAuthor", ImplementationInfo.libraryAuthor);
            MetaInfo.Add("MetaInfoLibraryURL", ImplementationInfo.libraryURL);
            MetaInfo.Add("MetaInfoLibraryCopyright", ImplementationInfo.libraryCopyright);
            MetaInfo.Add("MetaInfoLibraryLicense", ImplementationInfo.libraryLicense);
            MetaInfo.Add("MetaInfoLibrarySPDXLicenseIdentifier", ImplementationInfo.librarySPDXLicenseIdentifier);
            MetaInfo.Add("MetaInfoLibraryLicenseURL", ImplementationInfo.libraryLicenseURL);
            MetaInfo.Add("MetaInfoExecEnvironment", ImplementationInfo.ExecEnvironment);

        }


        static public Dictionary<string,string> GetMetainfo()
        {
            return MetaInfo;
        } 

        static public void SetIntervalTime(double value)
        {
            _TSDR_minimum_interval = value;
        }

        static public void ResetIntervalTime()
        {
            _TSDR_minimum_interval = _default_TSDR_minimum_interval;
        }

        static public DateTime? _GetPriorTSDRCallTime()
            // For unit-testing only, should not really be public
        {
            // return ((DateTime)_prior_TSDR_call_time);
            return ((DateTime?)_prior_TSDR_call_time);

        }

        static public void _SetPriorTSDRCallTime(DateTime? value)
        // For unit-testing only, should not really be public
        {
            _prior_TSDR_call_time = value;

        }

        public TSDRReq()
        {
            reset();
        }

        public void reset()
        {
            // reset control fields (API Key, XML transform, PTO format)
            resetAPIKey();
            resetXSLT();
            resetPTOFormat();
            // reset fetched data
            resetXMLData(); // resetting XML will cascade to CSV and TSDR map, too
        }

        public void setAPIKey(string key)
        {
            // Set the USPTO-provided API Key to be used iin HTTP calls to TSDR
            // See https://developer.uspto.gov/api-catalog/tsdr-data-api

            APIKey = key;
        }

        public void resetAPIKey()
        {
            // Resets self.APIKey to null, causing no API key to be passed to TSDR
            // (without a key set, expect System.Net.WebException: "HTTP Error 401: Unauthorized")

            APIKey = null;
        }

        public void setXSLT(string xslt)
        {
            XSLT = xslt;
        }

        public void resetXSLT()
        {
            XSLT = null;
        }

        public void setPTOFormat(string format)
        // Determines what format file will be fetched from the PTO.
        //    "ST66": ST66-format XML
        //    "ST96": ST96-format XML
        //     "zip": zip file. The zip file obtained from the PTO is currently ST66-format XML.
        // If this is reset, "ST96" will be assumed.
        {
            List<string> valid_formats = new List<string> { "ST66", "ST96", "zip" };
            if (!valid_formats.Contains(format))
            {
                throw new ArgumentException(string.Format("Invalid PTO format '{0}'.", format));
            }
            else
            {
                PTOFormat = format;
            }
        }

        public void resetPTOFormat()
        //  Resets PTO format to "ST96" (default)
        {
            setPTOFormat("ST96");
        }

        public void resetXMLData()
        {
            XMLData = null;
            ZipData = null;
            ImageFull = null;
            ImageThumb = null;
            ErrorCode = null;
            ErrorMessage = null;
            XMLDataIsValid = false;
            resetCSVData();
        }

        public void resetCSVData()
        {
            CSVData = null;
            CSVDataIsValid = false;
            resetTSDRData();
        }

        public void resetTSDRData()
        {
            TSDRData = new TSDRMap();
        }

        public void getTSDRInfo(string identifier, string tmtype = null)
        {
            getXMLData(identifier, tmtype);
            if (XMLDataIsValid)
            {
                getCSVData();
                if (CSVDataIsValid)
                {
                    getTSDRData();
                }
            }
            return;
        }

        public void getXMLData(string identifier, string tmtype = null)
        {
            if (_prior_TSDR_call_time != null)
            {
                _waitFromTime((DateTime)_prior_TSDR_call_time, _TSDR_minimum_interval);
            }
            _prior_TSDR_call_time = DateTime.Now;
            resetXMLData();
            if (tmtype == null)
            {
                getXMLDataFromFile(identifier);
            }
            else
            {
                getXMLDataFromPTO(identifier, tmtype);
            }
            return;
        }

        public void getXMLDataFromFile(string path)
        {
            System.DateTime now;
            byte[] filedata = File.ReadAllBytes(path);
            TSDRSubstitutions["$XMLSOURCE$"] = path;
            now = DateTime.Now;
            TSDRSubstitutions["$TSDRSTARTDATETIME$"] = now.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture);
            TSDRSubstitutions["$EXECUTIONDATETIME$"] = now.ToString("yyyy-MM-dd HH:mm:ss");
            processFileContents(filedata);
            now = DateTime.Now;
            TSDRSubstitutions["$TSDRCOMPLETEDATETIME$"] = now.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture);
            return;
        }

        public void getXMLDataFromPTO(string number, string tmtype)
        {
            System.DateTime now;
            byte[] filedata;
            validatePTOParameters(number, tmtype);
            string xml_url_template_st66 = "https://tsdrapi.uspto.gov/ts/cd/status66/{0}n{1}/info.xml";
            string xml_url_template_st96 = "https://tsdrapi.uspto.gov/ts/cd/casestatus/{0}n{1}/info.xml";
            string zip_url_template = "https://tsdrapi.uspto.gov/ts/cd/casestatus/{0}n{1}/content.zip";
            Dictionary<string, string> pto_url_templates = new Dictionary<string, string> {
                {"ST66", xml_url_template_st66},
                {"ST96", xml_url_template_st96},
                {"zip", zip_url_template}
            };
            string pto_url_template = pto_url_templates[PTOFormat];
            string PTO_URL = string.Format(pto_url_template, tmtype, number);
            using (WebClient wc = new WebClient())
            {
                if (APIKey != null)
                {
                    wc.Headers.Add("USPTO-API-KEY", APIKey);
                }
                try
                {
                    filedata = wc.DownloadData(PTO_URL);
                }
                catch (System.Net.WebException ex)
                {
                    HttpWebResponse response = ex.Response as HttpWebResponse;
                    if ((response != null) &&
                        (response.StatusCode == HttpStatusCode.NotFound))
                    {
                        ErrorCode = "Fetch-404";
                        ErrorMessage = string.Format("getXMLDataFromPTO: Error fetching from PTO. Errorcode: 404 (not found); URL: {0}", PTO_URL);
                        return;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            TSDRSubstitutions["$XMLSOURCE$"] = PTO_URL;
            now = DateTime.Now;
            TSDRSubstitutions["$TSDRSTARTDATETIME$"] = now.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture);
            TSDRSubstitutions["$EXECUTIONDATETIME$"] = now.ToString("yyyy-MM-dd HH:mm:ss");
            processFileContents(filedata);
            now = DateTime.Now;
            TSDRSubstitutions["$TSDRCOMPLETEDATETIME$"] = now.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture);
            return;
        }

        private void processFileContents(byte[] filedata)
        {
            try
            {
                // assume it's a zip file
                using (MemoryStream ms = new MemoryStream(filedata))
                {
                    ZipArchive za =
                      new ZipArchive(ms, ZipArchiveMode.Read);
                    processZip(za, filedata);
                    ZipData = filedata;
                }
            }
            catch (System.IO.InvalidDataException)
            {
                // otherwise, it's not a zip file (plain XML)
                XMLData = Encoding.UTF8.GetString(filedata, 0, filedata.Length);
            }
            string error_reason = xml_sanity_check(XMLData);
            if (error_reason != "")
            {
                XMLDataIsValid = false;
                ErrorCode = "CSV-NoValidXML";
                ErrorMessage = error_reason;
            }
            else
            {
                XMLDataIsValid = true;
            }
            return;
        }

        private void validatePTOParameters(string number, string tmtype)
        {
            /// number: string made up of 7 or 8 digits;
            ///   7 digits for registrations, 8 for applications
            /// tmtype: string;
            ///   's' (serial number of application) or 'r' (registration number)
            List<string> validTMTypes = new List<string> { "s", "r" };
            if (!validTMTypes.Contains(tmtype))
            {
                throw new System.ArgumentException("Invalid tmtype value '" + tmtype +
                    "' specified: 's' or 'r' required.", "tmtype");
            }

            if (!number.All(Char.IsDigit))
            {
                throw new System.ArgumentException("Invalid identification number '" + number +
                                    "' specified: must be all-numeric", "number");
            }

            int expected_length;
            if (tmtype == "s")
            {
                expected_length = 8;
            }
            else
            {
                expected_length = 7;
            }
            if (!(number.Length == expected_length))
            {
                throw new System.ArgumentException("Invalid identification number '" + number +
                                    "' specified: type " + tmtype + " must be " +
                                    expected_length + " digits.", "number");
            }
            return;
        }

        private void processZip(ZipArchive za, byte[] filedata)
        {
            ZipArchiveEntry zentry;
            StreamReader sr;
            BinaryReader br;

            // XML
            zentry = za.GetEntry("status_st66.xml");
            using (sr = new StreamReader(zentry.Open()))
            {
                XMLData = sr.ReadToEnd();
            }

            // Full image
            zentry = za.GetEntry("markImage.jpg");
            if (zentry != null)
            {
                using (br = new BinaryReader(zentry.Open()))
                {
                    ImageFull = br.ReadBytes(1048576);
                }
            }

            // Thumbnail image
            zentry = za.GetEntry("markThumbnailImage.jpg");
            if (zentry != null)
            {
                using (br = new BinaryReader(zentry.Open()))
                {
                    ImageThumb = br.ReadBytes(1048576);
                }
            }
            return;
        }

        public void getCSVData()
        {
            string xml_format;
            string rawCSVData;
            resetCSVData();
            if (!XMLDataIsValid)    /// make sure we have some valid XMLData to process
            {
                CSVDataIsValid = false;
                ErrorCode = "CSV-NoValidXML";
                ErrorMessage = "No valid XML Data found";
                return;
            }
            XslCompiledTransform transform = new XslCompiledTransform();
            XmlDocument parsed_xml = new XmlDocument();
            parsed_xml.LoadXml(XMLData);

            if (XSLT != null)
            {
                string transformtext = XSLT;
                using (StringReader sr = new StringReader(transformtext))
                {
                    using (XmlReader xr = XmlReader.Create(sr))
                    {
                        transform.Load(xr);
                    }
                }
                TSDRSubstitutions["$XSLTFILENAME$"] = "CALLER-PROVIDED XSLT";
                TSDRSubstitutions["$XSLTLOCATION$"] = "CALLER-PROVIDED XSLT";

                /// throw new System.NotImplementedException();
            }
            else
            {
                List<string> supported_xml_formats = new List<string> { "ST66", "ST96" };
                xml_format = determine_xml_format(parsed_xml);
                if (!supported_xml_formats.Contains(xml_format))
                {
                    CSVDataIsValid = false;
                    ErrorCode = "CSV-UnsupportedXML";
                    ErrorMessage = "Unsupported XML format found: " + xml_format;
                    return;
                }
                XSLTDescriptor xslt_transform_info = xslt_table[xml_format];
                transform = xslt_transform_info.transform;
                TSDRSubstitutions["$XSLTFILENAME$"] = xslt_transform_info.filename;
                TSDRSubstitutions["$XSLTLOCATION$"] = xslt_transform_info.location;
            }
            using (StringWriter sw = new StringWriter())
            {
                transform.Transform(parsed_xml, null, sw);
                rawCSVData = sw.ToString();
            }
            string csv_string = perform_substitution(rawCSVData);
            CSVData = normalize_blank_lines(csv_string);
            validateCSVResponse csvresults = validateCSV();
            if (csvresults.CSV_OK)
            {
                CSVDataIsValid = true;
            }
            else
            {
                CSVDataIsValid = false;
                ErrorCode = csvresults.error_code;
                ErrorMessage = csvresults.error_message;
            }
            return;
        }

        private string normalize_blank_lines(string string_of_lines)
        {
            /*
             This internal method takes a string of lines, separated by the system line separator
             character (e.g. \n), and eliminates lines that are empty or consisting entirely of
             whitespace. Its purpose is to relax what input is accepted from the the XSLT process,
             so that including blank/empty lines is permitted and whether the final line ends with a
             newline is immaterial.
             */
            string[] lines = string_of_lines.Split(new string[] { LINE_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
            // RemoveEmptyEntries gets rid of empty, but not blank, lines
            lines = drop_blank_lines(lines);
            string reassembled_string_of_lines = string.Join(LINE_SEPARATOR, lines) + LINE_SEPARATOR;
            return reassembled_string_of_lines;
        }

        private string[] drop_blank_lines(string[] input_lines)
        {
            List<string> winnowed_lines = new List<string>();
            foreach (string line in input_lines)
            {
                if (line.Trim().Length > 0)
                {
                    winnowed_lines.Add(line);
                }
            }
            string[] output_lines = winnowed_lines.ToArray();
            return output_lines;
            // output_lines = from line in input_lines where (line.Trim().Length > 0) select line;
        }

        private class validateCSVResponse
        {
            public Boolean CSV_OK = true;
            private static string no_error_found_message = "getCSVData: No obvious errors parsing XML to CSV.";
            public string error_code = null;
            public string error_message = no_error_found_message;
        }


        private validateCSVResponse validateCSV()
        {
            /*
            validateCSV performs a naive sanity-check of the CSV for obvious errors.
            It's not bullet-proof, but catches some of the more likely problems that would
            survive the XSLT transform without errors, but potentially produce erroneous
            data that might cause hard-to-find problems downstream.

            It checks:  
             * CSV data consists of more than one line (a common error is a bad XSLT
               transform that slurps in entire file as one line, especially if using an
               XSLT transform designed for ST66 on ST96 data, or vice-versa).
             * Each line must be:
                  - in the format [START-OF-LINE]KEYNAME,"VALUE"[END-OF-LiNE]
                  - KEYNAME consists only of letters (A-Z, a-z) and digits (0-9);
                  - VALUE is enclosed in double-quotes;
                  - No spaces or other whitespace anywhere except in VALUE, inside the
                    quotes; not even before/after the comma or after "VALUE".

            Returns a validateCSVResponse consisting of:
              CSV_OK (boolean): True if no errors found, else False;
              error_code (string): short error code, designed to be inspected by calling
              program;
              error_message (string): detailed error message, designed to be read by humans.
            */
            validateCSVResponse result = new validateCSVResponse();
            // following lines are to inject errors for testing
            //CSVData = "MadeupKey1,\"MadeupValue1\""; //NG
            //CSVData = "MadeupKey1,\"MadeupValue1\"" + LINE_SEPARATOR; //NG
            //CSVData = "MadeupKey1,\"MadeupValue1\"" + LINE_SEPARATOR + "MadeupKey2,\"MadeupValue2\""; //OK
            //CSVData = "MadeupKey1,\"MadeupValue1\"" + LINE_SEPARATOR + "MadeupKey2,\"MadeupValue2\"" + LINE_SEPARATOR; //OK

            try
            {
                string[] lines = CSVData.Split(new string[] { LINE_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
                // lines = drop_empty_lines(lines);
                // condition 1: parse to at least 2 lines
                if (lines.Length < 2)
                {
                    result.error_code = "CSV-ShortCSV";
                    result.error_message = "getCSVData: XML parsed to fewer than 2 lines of CSV.";
                    throw new ArgumentException();
                }
                for (int line_number_offset = 0; line_number_offset < lines.Length; line_number_offset++)
                {
                    string line = lines[line_number_offset];
                    // condition 2: comma-separated

                    // following line is to inject errors for testing
                    // line = "MadeupKey1\"MadeupValue1\"";
                    if (!line.Contains(COMMA))
                    {
                        result.error_code = "CSV-InvalidKeyValuePair";
                        result.error_message = "getCSVData [line " + (line_number_offset + 1).ToString() + "]: " +
                            "no key-value pair found in line <" + line + "> (missing comma).";
                        throw new ArgumentException();
                    }
                    int comma_position = line.IndexOf(COMMA);
                    string k = line.Substring(0, comma_position);
                    string v = line.Substring(comma_position + 1);

                    // condition 3: key is alphanumeric
                    if (!k.All(char.IsLetterOrDigit))
                    {
                        result.error_code = "CSV-InvalidKey";
                        result.error_message = "getCSVData [line " + line_number_offset + 1.ToString() + "]: " +
                            "Invalid key <" + k + "> found (invalid characters in key).";
                        throw new ArgumentException();
                    }

                    // condition 4: value is in quotes with no trailing whitespace
                    string stripped_v = v.Substring(1, v.Length - 2);
                    // strip off what should be first & last quote marks
                    // Note C# and Java have differing substring semantics
                    // C#: 2nd operand is length' Java: 2nd operand is ending index
                    if (v != '"' + stripped_v + '"')
                    {
                        result.error_code = "CSV-InvalidValue";
                        result.error_message = "getCSVData [line " + line_number_offset + 1.ToString() + "]: " +
                            "Invalid value <" + v + "> found for key <" + k + " (does not begin and end with double-quote character).";
                        throw new ArgumentException();
                    }
                }
            }
            catch (ArgumentException e)
            {
                if (result.error_code == null)
                {
                    // not good, something we didn't count on went wrong; we should never get here
                    result.error_code = "CSV-UnknownError";
                    result.error_message = "getCSVData: unknown error validating CSV data <" + e.Message + ">";
                }
                result.CSV_OK = false;
            }
            return result;
        }

        private string perform_substitution(string rawCSVData)
        {
            // Substitute run-time data for $PLACEHOLDERS$ from XSLT
            string s;
            s = rawCSVData;
            foreach (string variable in TSDRSubstitutions.Keys)
            {
                s = s.Replace(variable, TSDRSubstitutions[variable]);
            }
            return s;

        }

        private string determine_xml_format(XmlDocument parsed_xml)
        {
            /*
            Given a parsed XML tree, determine its format ("ST66" or "ST96";
            or null, if not determinable)
            */
            string ST66_root_namespace = "http://www.wipo.int/standards/XMLSchema/trademarks";
            // Former tag value for ST-96 1_D3; keeping it around as known but unsupported for diagnostic value 
            string ST96_1_D3_root_namespace = "http://www.wipo.int/standards/XMLSchema/Trademark/1";
            string ST96_root_namespace = "http://www.wipo.int/standards/XMLSchema/ST96/Trademark";
            Dictionary<string, string> namespace_map = new Dictionary<string, string>
            {
                {ST66_root_namespace, "ST66"},
                {ST96_1_D3_root_namespace, "ST96-1_D3"},
                {ST96_root_namespace, "ST96"}
            };
            XmlElement root = parsed_xml.DocumentElement;
            string root_namespace = root.NamespaceURI;
            string result = null;
            if (namespace_map.ContainsKey(root_namespace))
            {
                result = namespace_map[root_namespace];
            }
            return result;
        }

        private string xml_sanity_check(string text)
        {
            /*
            Quick check to see if text is valid XML; not comprehensive, just a sanity check.
            Returns string containing an error reason if XML fails, or "" if XML passes.
            */
            string error_reason = "";
            if (string.IsNullOrEmpty(XMLData))
            {
                error_reason = "getXMLData: XML data is null or empty.";
            }
            else
            {
                try
                {
                    /// see if this triggers an XMLException
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(text);
                    /// no exception, passes sanity check
                }
                catch (XmlException e)
                {
                    error_reason = "getXMLData: exception (System.Xml.XmlException) parsing purported XML data.  " +
                    "Reason: " + e.Message;
                }
            }

            return error_reason;
        }

        private void _waitFromTime(DateTime fromtime, double duration)
            /*
             Wait until the specified duration (in seconds) after fromtime (DateTime) has occurred
            */
        {
            DateTime now = DateTime.Now;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)(duration*1000));  // days, hours, minutes, seconds, milliseconds
            DateTime end_time = fromtime + ts;
            int pause_time_in_ms = (int)((end_time - now).TotalMilliseconds);
            if (pause_time_in_ms > 0)
            {
                Thread.Sleep(pause_time_in_ms);
            }

        }
        

        public void getTSDRData()
        {
            /*
                Refactor key/data pairs to dictionary.
                getCSVData must be successfully invoked before using this method.

            
            note on general strategy:
            generally, we read key/value pairs and simply add to dictionary
            repeated fields will be temporarily kept in a repeated_item_dict,
            and added at end of processing.
        
            When a "BeginRepeatedField" key is hit we process as follows:
              - allocate an empty temp dictionary for this field;
              - for each key/value pair encountered in the repeated field, add to the
                temp dictionary;
              - When the "EndRepeatedField" key is encountered (say, for field "FOO"):
                  - if no "ListFOO" exists yet, create an empty list to describe the FOO
                    field in the repeated_item dict;
                  - add the temp dictionary to the ListFOO entry;
                  - resume sending updates to regular dictionary, not subdict.
            At end of processing, merge repeated-item dict into main dict.
            First-found "Applicant" is deemed current applicant, so copy that one to
            the main entry.
            */
            resetTSDRData();
            if (!CSVDataIsValid)
            {
                ErrorCode = "Map-NoValidCSV";
                ErrorMessage = "No valid CSV data.";
                return;
            }
            Dictionary<string, List<Dictionary<string, string>>> repeated_item_dict =
                new Dictionary<string, List<Dictionary<string, string>>> { };
            Dictionary<string, string> output_dict = new Dictionary<string, string> { };
            char comma = ',';
            string quotemark = "\"";

            // Dictionary<string,Object> current_dict = output_dict;
            Dictionary<string, string> current_dict = output_dict;
            string[] lines = CSVData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            // RemoveEmptyEntries gets rid of empty, but not blank, lines
            lines = drop_blank_lines(lines);
            foreach (string line in lines)
            {
                int comma_position = line.IndexOf(comma);
                string key = line.Substring(0, comma_position);
                string data = line.Substring(comma_position + 1);
                // should be quotes on the value; strip them off:
                string firstchar = data.Substring(0, 1);
                string lastchar = data.Substring(data.Length - 1, 1);
                if ((firstchar == lastchar) &&
                    (firstchar == quotemark))
                {
                    data = data.Substring(1, data.Length - 2);
                }
                switch (key)
                {
                    case "BeginRepeatedField":
                        {
                            Dictionary<string, string> temp_dict = new Dictionary<string, string>();
                            current_dict = temp_dict;
                            break;
                        }

                    case "EndRepeatedField":
                        {
                            // First time, allocate a new empty list to be added to;
                            // otherwise, re-use the existing list
                            string listkey = data + "List";
                            if (!repeated_item_dict.ContainsKey(listkey))
                            {
                                repeated_item_dict[listkey] = new List<Dictionary<string, string>>();
                            }
                            (repeated_item_dict[listkey]).Add(current_dict);
                            // dumpdict(current_dict);
                            // dumpdict(repeated_item_dict);
                            // done processing special list, resume regular processing
                            current_dict = output_dict;
                            break;
                        }
                    default:
                        {
                            current_dict[key] = data;
                            // dumpdict(current_dict);
                            break;
                        }
                }
            }

            TSDRData.TSDRSingle = output_dict;
            TSDRData.TSDRMulti = repeated_item_dict;
            TSDRData.TSDRMapIsValid = true;
            return;
        }

        private void dumpdict(Dictionary<string, Object> ht)
        {

            string[] allkeys = ht.Keys.Cast<string>().ToArray();
            Console.WriteLine("Keys: " + allkeys.Length);
            foreach (string k in allkeys) Console.WriteLine(k);
            // Console.ReadLine();
        }

        private class XSLTDescriptor
        {
            public string filename;
            public string pathname;
            public string location;
            public string transformtext;
            public XslCompiledTransform transform = new XslCompiledTransform();

            public XSLTDescriptor(string XMLformat)
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                String asmname = asm.GetName().Name;
                string xsl_resource_name = asmname + "." + XMLformat + ".xsl";
                filename = xsl_resource_name;
                pathname = "N/A";
                location = "Assembly resource";
                using (Stream stream = asm.GetManifestResourceStream(xsl_resource_name))
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        transformtext = sr.ReadToEnd();
                    }
                }
                // old kludge, read external file
                // filename = XMLformat + ".xsl";
                // pathname = System.IO.Path.Combine(xslt_directory, filename);
                // transformtext = File.ReadAllText(pathname);
                using (StringReader sr = new StringReader(transformtext))
                {
                    using (System.Xml.XmlReader xr = System.Xml.XmlReader.Create(sr))
                    {
                        transform.Load(xr);
                    }
                }

            }
        }

        private class ImplementationInfo
        /// Information about the implementation itself, largely for metainfo in support of documentation and diagnostics
        {
            public static string libraryName { get; } = "Plumage-dotnet";
            public static string libraryVersion { get; } = "1.4.0";
            public static string libraryDate { get; } = "2021-02-02";
            public static string libraryAuthor { get; } = "Terry Carroll";
            public static string libraryURL { get; } = "https://github.com/codingatty/Plumage-dotnet";
            public static string libraryCopyright { get; } = "Copyright 2014-2021 Terry Carroll";
            public static string libraryLicense { get; } = "Apache License, version 2.0 (January 2004)";
            public static string librarySPDXLicenseIdentifier { get; } = "Apache-2.0";
            public static string libraryLicenseURL { get; } = "http://www.apache.org/licenses/LICENSE-2.0"; 
            public static string ExecEnvironment { get; } = Environment.Version.ToString();
            
        }
    }
        
    public class TSDRMap
    {
        public Dictionary<string, string> TSDRSingle = null;
        public Dictionary<string, List<Dictionary<string, string>>> TSDRMulti = null;
        public Boolean TSDRMapIsValid = false;
    }

}
