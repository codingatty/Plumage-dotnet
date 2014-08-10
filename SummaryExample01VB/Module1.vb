Module Module1

    Sub Main()
        Console.WriteLine("In the VB.net summary")
        Dim t As Plumage.TSDRReq = New Plumage.TSDRReq
        t.getTSDRInfo("2564831", "r")   ' get info on reg. no 2,564,831
        If t.TSDRMapIsValid Then
            Console.WriteLine("Application serial no: " + t.TSDRMap("ApplicationNumber"))
            Console.WriteLine("Trademark text: " + t.TSDRMap("MarkVerbalElementText"))
            Console.WriteLine("Application filing date: " + t.TSDRMap("ApplicationDate"))
            Console.WriteLine("Registration no: " + t.TSDRMap("RegistrationNumber"))
            Console.WriteLine("Status: " + t.TSDRMap("MarkCurrentStatusExternalDescriptionText"))
            ' Owner info is in most recent (0th) entry in ApplicantList
            Dim applicant_list As ArrayList = t.TSDRMap("ApplicantList")
            Dim current_owner_info As Dictionary(Of String, Object) = applicant_list(0)
            Console.WriteLine("Owner: " + current_owner_info("ApplicantName"))
            Console.WriteLine("Owner address: " + current_owner_info("ApplicantCombinedAddress"))
            ' Get most recent event: 0th entry in event list
            Dim event_list As ArrayList = t.TSDRMap("MarkEventList")
            Dim most_recent_event As Dictionary(Of String, Object) = event_list(0)
            Console.WriteLine("Most recent event: " + most_recent_event("MarkEventDescription"))
            Console.WriteLine("Event date: " + most_recent_event("MarkEventDate"))
        End If

    End Sub

End Module
