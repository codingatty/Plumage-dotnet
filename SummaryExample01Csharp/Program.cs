using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummaryExample01Csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("In Summary example c#");
            Plumage.TSDRReq t = new Plumage.TSDRReq();
            t.getTSDRInfo("2564831", "r");  // get info on reg. no 2,564,831
            if (t.TSDRData.TSDRMapIsValid){
                Console.WriteLine("Application serial no: " + t.TSDRData.TSDRSingle["ApplicationNumber"]);
                Console.WriteLine("Trademark text: " + t.TSDRData.TSDRSingle["MarkVerbalElementText"]);
                Console.WriteLine("Application filing date: " + t.TSDRData.TSDRSingle["ApplicationDate"]);
                Console.WriteLine("Registration no: " + t.TSDRData.TSDRSingle["RegistrationNumber"]);
                // Owner info is in most recent (0th) entry in ApplicantList
                List<Dictionary<string, string>> applicant_list = t.TSDRData.TSDRMulti["ApplicantList"];
                Dictionary<string, string> current_owner_info = applicant_list[0];
                Console.WriteLine("Owner: " + current_owner_info["ApplicantName"]);
                Console.WriteLine("Owner address: " + current_owner_info["ApplicantCombinedAddress"]);
                // Get most recent event: 0th entry in event list
                List<Dictionary<string, string>> event_list = t.TSDRData.TSDRMulti["MarkEventList"];
                Dictionary<string, string> most_recent_event = event_list[0];
                Console.WriteLine("Most recent event: " + most_recent_event["MarkEventDescription"]);
                Console.WriteLine("Event date: " + most_recent_event["MarkEventDate"]);
            }
            
        }
    }
}
