﻿using System;
using System.Collections.Generic;
using Borgarverk.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;

namespace Borgarverk
{
	public class SendService : ISendService
	{
		public SendService()
		{
		}

		public string EntryToJSon(EntryModel entry)
		{
			var json = JsonConvert.SerializeObject(entry);
			return json;
		}

		public string EntriesToJson(List<EntryModel> entries)
		{
			var json = JsonConvert.SerializeObject(entries);
			return json;
		}

		public async Task<Boolean> SendEntry(EntryModel entry)
		{
			entry.TimeSent = DateTime.Now;
			var myContent = JsonConvert.SerializeObject(entry);
			var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
			var byteContent = new ByteArrayContent(buffer);
			byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			var client = new HttpClient(new HttpClientHandler
			{
				UseProxy = false
			});

			try
			{
				var response = await client.PostAsync("https://httpbin.org/post", byteContent).ConfigureAwait(continueOnCapturedContext: false);
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}