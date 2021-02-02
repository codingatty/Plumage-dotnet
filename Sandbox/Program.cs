using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = "sss";
            Console.WriteLine("In the sandbox");
            Dictionary<string, string> metainfo = Plumage.TSDRReq.GetMetainfo();
            Plumage.TSDRReq t = new Plumage.TSDRReq();
            // t.setPTOFormat("ST96");
            Console.WriteLine("author: {0}", metainfo["MetaInfoLibraryAuthor"]);
            Console.WriteLine("last-updated: {0}", metainfo["MetaInfoXSLTDate"]);
            // t.getTSDRInfo("76044902", "s");
            //t.getTSDRInfo("C:/test/PlumageTestdata/rn2178784-ST-962.2.1.xml");
            t.getTSDRInfo("C:/test/PlumageTestdata/sn76044902.zip");
            Console.WriteLine("XMLDataIsValid:" + t.XMLDataIsValid);
            Console.WriteLine("CSVDataIsValid:" + t.CSVDataIsValid);
            Plumage.TSDRMap tsdrmap = t.TSDRData;
            Console.WriteLine("TSDRMapIsValid: " + tsdrmap.TSDRMapIsValid);
            Console.WriteLine("hit enter...");
            Console.ReadLine();
            if (tsdrmap.TSDRMapIsValid)
            {
                Console.WriteLine("TSDRMapIsValid:" + tsdrmap.TSDRMapIsValid);
                s = tsdrmap.TSDRSingle["ApplicationDate"];
                s = s.Substring(0, 7);
                Console.WriteLine("App date 0-7 s=" + s);
                Console.ReadLine();
                Console.WriteLine("TSDRMap subset:");
                foreach (string k in tsdrmap.TSDRSingle.Keys)
                {
                    Console.WriteLine("key: " + k + " value: " + tsdrmap.TSDRSingle[k]);
                }
                Console.WriteLine("(end of TSDRMap subset)");
                ArrayList things_to_print = new ArrayList { "ApplicationNumber", "MarkVerbalElementText", 
                                            "ApplicationDate", "RegistrationNumber",
                                            "MarkCurrentStatusExternalDescriptionText", "StaffName",    "StaffOfficialTitle"};
                foreach (string k in things_to_print)
                {
                    string v;
                    if (tsdrmap.TSDRSingle.ContainsKey(k))
                    {
                        v = tsdrmap.TSDRSingle[k];
                    }
                    else {
                        v = "(key not found)";
                    }
                    Console.WriteLine(k + ": " + v);
                }
                Console.WriteLine("zorp");
                string[] diag_keys = tsdrmap.TSDRSingle.Keys.Where(key => key.StartsWith("Diag")).ToArray<string>();
                Console.WriteLine("Diagnostic info");
                foreach (string k in diag_keys)
                {
                    string v = tsdrmap.TSDRSingle[k];
                    Console.WriteLine(k + ": " + v);
                }
                Console.WriteLine("End diagnostic info");
                List<Dictionary<string, string>> applicants = tsdrmap.TSDRMulti["ApplicantList"];
                Dictionary<string, string> firstapplicant = applicants[0];
                foreach (string k in firstapplicant.Keys)
                {
                    string v = firstapplicant[k];
                    Console.WriteLine(k + ": " + v);
                }
            }
            else
            {
                Console.WriteLine("Error code = " + t.ErrorCode);
                Console.WriteLine("Error message = " + t.ErrorMessage);
            }
            Console.WriteLine("hit enter...");
            Console.ReadLine();
        }
    }
}
