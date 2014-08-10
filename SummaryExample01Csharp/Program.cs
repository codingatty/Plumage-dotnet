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
            if (t.TSDRMapIsValid){
                Console.WriteLine("Application serial no: " + t.TSDRMap["ApplicationNumber"]);
                Console.WriteLine("Trademark text: " + t.TSDRMap["MarkVerbalElementText"]);
                Console.WriteLine("Application filing date: " + t.TSDRMap["ApplicationDate"]);
                Console.WriteLine("Registration no: " + t.TSDRMap["RegistrationNumber"]);
                Console.WriteLine("Status: " + t.TSDRMap["MarkCurrentStatusExternalDescriptionText"]);
                // Owner info is in most recent (0th) entry in ApplicantList
                ArrayList applicant_list = (ArrayList)t.TSDRMap["ApplicantList"];
                Dictionary<string, Object> current_owner_info = (Dictionary<string, Object>)applicant_list[0];
                Console.WriteLine("Owner: " + current_owner_info["ApplicantName"]);
                Console.WriteLine("Owner address: " + current_owner_info["ApplicantCombinedAddress"]);
                // Get most recent event: 0th entry in event list
                ArrayList event_list = (ArrayList)t.TSDRMap["MarkEventList"];
                Dictionary<string, Object> most_recent_event = (Dictionary<string, Object>)event_list[0];
                Console.WriteLine("Most recent event: " + most_recent_event["MarkEventDescription"]);
                Console.WriteLine("Event date: " + most_recent_event["MarkEventDate"]);
            }
            
        }
    }
}
