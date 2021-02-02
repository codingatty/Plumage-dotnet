Module Sandbox

    Sub Main()
        Dim t As Plumage.TSDRReq = New Plumage.TSDRReq
        Dim s As String
        Dim metainfo As Dictionary(Of String, String) = Plumage.TSDRReq.GetMetainfo()
        Console.WriteLine("In the VB.net test")
        Console.WriteLine("author: {0}", metainfo.Item("MetaInfoLibraryAuthor"))
        Console.WriteLine("last-updated: {0}", metainfo.Item("MetaInfoLibraryDate"))
        t.setAPIKey("PTO API KEY GOES HERE")
        t.getTSDRInfo("76044902", "s")
        Console.WriteLine("XMLDataIsValid:" + t.XMLDataIsValid.ToString)
        Console.WriteLine("CSVDataIsValid:" + t.CSVDataIsValid.ToString)
        Console.WriteLine("TSDRMapIsValid: " + t.TSDRData.TSDRMapIsValid.ToString)
        Console.ReadLine()
        If t.TSDRData.TSDRMapIsValid Then
            For Each k As String In t.TSDRData.TSDRSingle.Keys
                If Not k.EndsWith("List") Then
                    s = t.TSDRData.TSDRSingle.Item(k)
                    Console.WriteLine("key: " + k + " value: " + s)
                End If
            Next k
            Console.WriteLine("Diagnostic info")
            Dim things_to_print As ArrayList = New ArrayList From {"ApplicationNumber", "MarkVerbalElementText",
            "ApplicationDate", "RegistrationNumber", "MarkCurrentStatusExternalDescriptionText"}
            For Each k As String In things_to_print
                Console.WriteLine("key: " + k + " value: " + t.TSDRData.TSDRSingle.Item(k))
            Next
            Console.WriteLine("End of diagnostic info")
        End If


    End Sub

End Module
