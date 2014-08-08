// SummaryExample01CPP.cpp : main project file.

#include "stdafx.h"

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;


int main(array<System::String^>^args)
{
	Console::WriteLine("In Summary example c++");
	Plumage::TSDRReq^ t =  gcnew Plumage::TSDRReq;
	t->getTSDRInfo("2564831", "r");  // get info on reg. no 2,564,831
	Console::WriteLine(t->CSVData);
	if (t->TSDRMapIsValid){
		Console::WriteLine("Application serial no: " + t->TSDRMap["ApplicationNumber"]);
		Console::WriteLine("Trademark text: " + t->TSDRMap["MarkVerbalElementText"]);
		Console::WriteLine("Application filing date: " + t->TSDRMap["ApplicationDate"]);
		Console::WriteLine("Registration no: " + t->TSDRMap["RegistrationNumber"]);
		Console::WriteLine("Status: " + t->TSDRMap["MarkCurrentStatusExternalDescriptionText"]);
		// Owner info is in most recent (0th) entry in ApplicantList
		ArrayList^ applicant_list = (ArrayList^)(t->TSDRMap["ApplicantList"]);
		Dictionary<String^, Object^>^ current_owner_info = (Dictionary<String^, Object^>^)applicant_list[0];
		Console::WriteLine("Owner: " + current_owner_info["ApplicantName"]);
		Console::WriteLine("Owner address: " + current_owner_info["ApplicantCombinedAddress"]);
	};


    return 0;
}
