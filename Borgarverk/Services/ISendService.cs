﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Borgarverk.Models;

namespace Borgarverk
{
	public interface ISendService
	{
		string EntryToJSon(EntryModel entry);
		string EntriesToJson(List<EntryModel> entries);
		Task<Boolean> SendEntry(EntryModel entry);
		bool SendEntries(List<EntryModel> entries);
	}
}
