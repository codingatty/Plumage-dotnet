﻿Module Module1

    Sub Main()
        Dim t As Plumage.TSDRReq = New Plumage.TSDRReq
        Dim s As String
        Console.WriteLine("IN the Basic test")
        Console.WriteLine("author: {0}", Plumage.TSDRReq.__author__)
        Console.WriteLine("last-updated: {0}", Plumage.TSDRReq.__last_updated__)
        t.getTSDRInfo("76044902", "s")
        Console.WriteLine("XMLDataIsValid:" + t.XMLDataIsValid.ToString)
        Console.WriteLine("CSVDataIsValid:" + t.CSVDataIsValid.ToString)
        Console.WriteLine("TSDRMapIsValid: " + t.TSDRMapIsValid.ToString)
        Console.ReadLine()
        If t.TSDRMapIsValid Then
            For Each k As String In t.TSDRMap.Keys

                Console.WriteLine("key: " + k)
                If Not k.EndsWith("List") Then
                    s = t.TSDRMap.Item(k)
                    Console.WriteLine("key: " + k + " value: " + s)
                End If
            Next k
            Console.WriteLine("Diagnostic info")
            Dim things_to_print As ArrayList = New ArrayList From {"ApplicationNumber", "MarkVerbalElementText",
            "ApplicationDate", "RegistrationNumber", "MarkCurrentStatusExternalDescriptionText"}
            For Each k As String In things_to_print
                Console.WriteLine("key: " + k + " value: " + t.TSDRMap.Item(k))
            Next
            Console.WriteLine("End of diagnostic info")
        End If


    End Sub

End Module
