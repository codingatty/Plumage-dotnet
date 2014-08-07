using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Plumage
{
    public class TSDRReq
    {
        public static string __author__ = "Terry Carroll";
        public static string __version__ = "0.9.1";
        public static string __last_updated__ = "2014-08-07";
        public static string __URL__ = "https://github.com/codingatty";
        public static string __copyright__ = "Copyright 2014 Terry Carroll";
        public static string __license__ = "Apache License, version 2.0 (January 2004)";
        public static string __licenseURL__ = "http://www.apache.org/licenses/LICENSE-2.0";
        private static Dictionary<string, string> TSDRSubstitutions;
        private static Dictionary<string, XSLTDescriptor> xslt_table;
        // options fields
        public string XSLT, PTOFormat;
        // diagnostic fields
        public string ErrorCode, ErrorMessage;
        // substantive data fields
        //  from XML fetch step:
        public string XMLData, CSVData;
        public Dictionary<string, Object> TSDRMap;
        public byte[] ZipData, ImageFull, ImageThumb;
        //  data validity flags
        public Boolean XMLDataIsValid, CSVDataIsValid, TSDRMapIsValid;

        static TSDRReq()
        {
            TSDRSubstitutions = new Dictionary<string, string> {
                {"$XSLTFILENAME$","Not Set"},               // XSLT stylesheet file name
                {"$XSLTLOCATION$","Not Set"},               // XSLT stylesheet location
                {"$IMPLEMENTATIONNAME$","Fritz-C#"},        // TSDR implementation identifier
                {"$IMPLEMENTATIONVERSION$",__version__},    // TSDR implementation version no.
                {"IMPLEMENTATIONDATE$",__last_updated__},   // Implementation last-updated date
                {"$IMPLEMENTATIONAUTHOR$",__author__},      // Implementation author
                {"$IMPLEMENTATIONURL$",__URL__},            // implementation URL
                {"$IMPLEMENTATIONCOPYRIGHT$",__copyright__},    // implementation copyright notice
                {"$IMPLEMENTATIONLICENSE$",__license__},    // implementation license
                {"$IMPLEMENTATIONLICENSEURL$",__licenseURL__},  // Implementation license URL
                {"$EXECUTIONDATETIME$","Not Set"},          // Execution time
                {"$XMLSOURCE$","Not Set"}                   // URL or pathname of XML source
            };
            XSLTDescriptor ST66Table = new XSLTDescriptor("ST66");
            XSLTDescriptor ST96Table = new XSLTDescriptor("ST96");
            xslt_table = new Dictionary<string, XSLTDescriptor>();
            xslt_table["ST66"] = ST66Table;
            xslt_table["ST96"] = ST96Table;
        }

        public TSDRReq()
        {
            reset();
        }

        public void reset()
        {
            // reset control fields (XML transform, PTO format)
            unsetXSLT();
            unsetPTOFormat();
            // reset fetched data
            resetXMLData(); // resetting XML will cascade to CSV and TSDR map, too
        }

        public void setXSLT(string xslt)
        {
            XSLT = xslt;
        }

        public void unsetXSLT()
        {
            XSLT = null;
        }

        public void setPTOFormat(string format)
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

        public void unsetPTOFormat()
        {
            setPTOFormat("zip");
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
            resetTSDRMap();
        }

        public void resetTSDRMap()
        {
            TSDRMap = null;
            TSDRMapIsValid = false;
        }

        public void getTSDRInfo(string identifier, string tmtype = null)
        {
            getXMLData(identifier, tmtype);
            if (XMLDataIsValid)
            {
                getCSVData();
                if (CSVDataIsValid)
                {
                    getTSDRMap();
                }
            }
            return;
        }

        public System.Xml.Xsl.XslCompiledTransform ConvertStringToXSLT(string xsl_string)
        // not used
        {
            System.Xml.Xsl.XslCompiledTransform transform = new System.Xml.Xsl.XslCompiledTransform();
            using (StringReader sr = new StringReader(xsl_string))
            {
                using (System.Xml.XmlReader xr = System.Xml.XmlReader.Create(sr))
                {
                    transform.Load(xr);
                }
            }
            return transform;
        }

        public void getXMLData(string identifier, string tmtype = null)
        {
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
            byte[] filedata = File.ReadAllBytes(path);
            TSDRSubstitutions["$XMLSOURCE$"] = path;
            String now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            TSDRSubstitutions["$EXECUTIONDATETIME$"] = now;
            processFileContents(filedata);
            return;
        }

        public void getXMLDataFromPTO(string number, string tmtype)
        {
            byte[] filedata;
            validatePTOParameters(number, tmtype);
            string xml_url_template_st66 = "https://tsdrapi.uspto.gov/ts/cd/status66/{0}n{1}/info.html";
            string xml_url_template_st96 = "https://tsdrapi.uspto.gov/ts/cd/casestatus/{0}n{1}/info.html";
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
            String now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            TSDRSubstitutions["$EXECUTIONDATETIME$"] = now;
            processFileContents(filedata);
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
            resetCSVData();
            if (!XMLDataIsValid)    /// make sure we have some valid XMLData to process
            {
                ErrorCode = "CSV-NoValidXML";
                ErrorMessage = "No valid XML Data found";
                return;
            }
            XslCompiledTransform transform = new XslCompiledTransform();
            XmlDocument parsed_xml = new XmlDocument();
            parsed_xml.LoadXml(XMLData);
            string rawCSVData;

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
                string xml_format = determine_xml_format(parsed_xml);
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
            CSVData = perform_substitution(rawCSVData);
            ArrayList csvresults = validateCSV();
            Boolean csvvalid = (Boolean)csvresults[0];
            string errcode = (string)csvresults[1];
            string errmsg = (string)csvresults[2];
            if (csvvalid)
            {
                CSVDataIsValid = true;
            }
            else
            {
                CSVDataIsValid = false;
                ErrorCode = errcode;
                ErrorMessage = errmsg;
            }
            return;
        }

        private ArrayList validateCSV()
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

            Returns a list consisting of:
              CSV_OK (boolean): True if no errors found, else False;
              error_code (string): short error code, designed to be inspected by calling
              program;
              error_message (string): detailed error message, designed to be read by humans.
            */
            Boolean CSV_OK = true; // Assume no errors found
            string no_error_found_message = "getCSVData: No obvious errors  parsing XML to CSV.";
            string error_code = null;
            string error_message = no_error_found_message;
            char comma = ',';
            // following line is to inject errors for testing
            //CSVData = "MadeupKey1,\"MadeupValue1\"" + Environment.NewLine + "MadeupKey2,\"MadeupValue2\"";
            try
            {
                string[] lines = CSVData.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                // condition 1: parse to at least 2 lines
                if (lines.Length < 2)
                {
                    error_code = "CSV-ShortCSV";
                    error_message = "getCSVData: XML parsed to fewer than 2 lines of CSV.";
                    throw new ArgumentException();
                }
                for (int line_number_offset = 0; line_number_offset < lines.Length; line_number_offset++)
                {
                    string line = lines[line_number_offset];
                    // condition 2: comma-separated
                    if (!line.Contains(comma))
                    {
                        error_code = "CSV-InvalidKeyValuePair";
                        error_message = "getCSVData [line " + line_number_offset + 1.ToString() + "]: " +
                            "no key-value pair found in line <" + line + "> (missing comma).";
                        throw new ArgumentException();
                    }
                    int comma_position = line.IndexOf(comma);
                    string k = line.Substring(0, comma_position);
                    string v = line.Substring(comma_position + 1);

                    // condition 3: key is alphanumeric
                    if (!k.All(char.IsLetterOrDigit))
                    {
                        error_code = "CSV-InvalidKey";
                        error_message = "getCSVData [line " + line_number_offset + 1.ToString() + "]: " +
                            "Invalid key <" + k + "> found (invalid characters in key).";
                        throw new ArgumentException();
                    }

                    // condition 4: value is in quotes with no trailing whitespace
                    string stripped_v = v.Substring(1, v.Length - 2); //strip off what should be first & last quote marks
                    if (v != '"' + stripped_v + '"')
                    {
                        error_code = "CSV-InvalidValue";
                        error_message = "getCSVData [line " + line_number_offset + 1.ToString() + "]: " +
                            "Invalid value <" + v + "> found for key <" + k + " (does not begin and end with double-quote character).";
                        throw new ArgumentException();
                    }
                }
            }
            catch (ArgumentException e)
            {
                if (error_code == null)
                {
                    // not good, something we didn't count on went wrong; we should never get here
                    error_code = "CSV-UnknownError";
                    error_message = "getCSVData: unknown error validating CSV data.";
                }
                CSV_OK = false;
            }
            ArrayList resultlist = new ArrayList { CSV_OK, error_code, error_message };
            return resultlist;
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
            string ST96_root_namespace = "http://www.wipo.int/standards/XMLSchema/Trademark/1";
            Dictionary<string, string> namespace_map = new Dictionary<string, string>
            {
                {ST66_root_namespace, "ST66"},
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

        public void getTSDRMap()
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
            resetTSDRMap();
            if (!CSVDataIsValid)
            {
                ErrorCode = "Map-NoValidCSV";
                ErrorMessage = "No valid CSV data.";
                return;
            }
            // Dictionary<string,List<Dictionary<string,string>>> repeated_item_dict = new Dictionary<string,List<Dictionary<string,string>>> { };
            // Dictionary<string, Object> output_dict = new Dictionary<string, Object> { };
            Dictionary<string, Object> repeated_item_dict = new Dictionary<string, Object> { };
            Dictionary<string, Object> output_dict = new Dictionary<string, Object> { };
            char comma = ',';
            string quotemark = "\"";

            // Dictionary<string,Object> current_dict = output_dict;
            Dictionary<string, Object> current_dict = output_dict;
            string[] lines = CSVData.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
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
                            // Console.WriteLine("In begin-state for key: " + key + ", data: " + data);
                            Dictionary<string, Object> temp_dict = new Dictionary<string, Object>();
                            current_dict = temp_dict;
                            break;
                        }

                    case "EndRepeatedField":
                        {
                            // Console.WriteLine("In end-state for key: " + key + ", data: " + data);
                            // First time, allocate a new empty list to be added to;
                            // otherwise, re-use the existing list
                            string listkey = data + "List";
                            if (!repeated_item_dict.ContainsKey(listkey))
                            {
                                repeated_item_dict[listkey] = new ArrayList();
                            }
                            ((ArrayList)repeated_item_dict[listkey]).Add(current_dict);
                            // dumpdict(current_dict);
                            // dumpdict(repeated_item_dict);
                            // done processing special list, resume regular processing
                            current_dict = output_dict;
                            break;
                        }
                    default:
                        {
                            //Console.WriteLine("In neither state for key: " + key + ", data: " + data);
                            current_dict[key] = data;
                            // dumpdict(current_dict);
                            break;
                        }
                }
            }
            foreach (KeyValuePair<string, Object> item in repeated_item_dict)
            {
                output_dict[item.Key] = item.Value;
            }

            TSDRMap = output_dict;
            TSDRMapIsValid = true;
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
            // old kludge
            // private string xslt_directory = @"C:\Users\TJC\Documents\Visual Studio 2013\Projects\FritzProg";
            public string filename;
            public string pathname;
            public string location;
            public string transformtext;
            public XslCompiledTransform transform = new XslCompiledTransform();

            public XSLTDescriptor(string xsltformat)
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                String asmname = asm.GetName().Name;
                string xsl_resource_name = asmname + "." + xsltformat + ".xsl";
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
                // filename = xsltformat + ".xsl";
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

    }
}
