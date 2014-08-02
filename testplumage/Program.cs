using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testplumage
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = "sss";
            Plumage.TSDRReq t = new Plumage.TSDRReq();
            Console.WriteLine("author: {0}", Plumage.TSDRReq.__author__);
            Console.WriteLine("last-updated: {0}", Plumage.TSDRReq.__last_updated__);
            t.getTSDRInfo("76044902", "s");
            Console.WriteLine("XMLDataIsValid:" + t.XMLDataIsValid);
            Console.WriteLine("CSVDataIsValid:" + t.CSVDataIsValid);
            Console.WriteLine("TSDRMapIsValid: " + t.TSDRMapIsValid);
            Console.ReadLine();
            if (t.TSDRMapIsValid)
            {
                Console.WriteLine("TSDRMapIsValid:" + t.TSDRMapIsValid);
                Console.WriteLine("TSDRMap subset:");
                foreach (string k in t.TSDRMap.Keys)
                {
                    Console.WriteLine("key: " + k + " value: " + t.TSDRMap[k]);
                }
                Console.WriteLine("(end of TSDRMap subset)");
                ArrayList things_to_print = new ArrayList { "ApplicationNumber", "MarkVerbalElementText", 
                                            "ApplicationDate", "RegistrationNumber",
                                            "MarkCurrentStatusExternalDescriptionText"};
                foreach (string k in things_to_print)
                {
                    string v = (string)t.TSDRMap[k];
                    Console.WriteLine(k + ": " + v);
                }
                Console.WriteLine("zorp");
                string[] diag_keys = t.TSDRMap.Keys.Where(key => key.StartsWith("Diag")).ToArray<string>();
                Console.WriteLine("Diagnostic info");
                foreach (string k in diag_keys)
                {
                    string v = (string)t.TSDRMap[k];
                    Console.WriteLine(k + ": " + v);
                }
                Console.WriteLine("End diagnostic info");
                ArrayList applicants = (ArrayList)t.TSDRMap["ApplicantList"];
                Dictionary<string, Object> firstapplicant = (Dictionary<string, Object>)applicants[0];
                foreach (string k in firstapplicant.Keys)
                {
                    string v = (string)firstapplicant[k];
                    Console.WriteLine(k + ": " + v);
                }

            }

        }
    }
}
